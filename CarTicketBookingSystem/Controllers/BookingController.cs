
using CarTicketBookingSystem.Data;
using CarTicketBookingSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarTicketBookingSystem.Services;

namespace CarTicketBookingSystem.Controllers
{
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ISmsSender _smsSender;

        public BookingController(ApplicationDbContext context, ISmsSender smsSender)
        {
            _context = context;
            _smsSender = smsSender;
        }

        // GET: Booking/Create?routeId=1
        [HttpGet]
        public async Task<IActionResult> Create(int routeId)
        {
            var route = await _context.TravelRoutes
                .FirstOrDefaultAsync(r => r.Id == routeId);

            if (route == null)
            {
                return NotFound();
            }

            var booking = new Booking
            {
                TravelRouteId = route.Id,
                Fare = route.Fare
            };

            ViewBag.Route = route;

            return View(booking);
        }

        // POST: Booking/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Booking booking)
        {
            // Check whether the selected route exists
            var route = await _context.TravelRoutes
                .FirstOrDefaultAsync(r =>
                    r.Id == booking.TravelRouteId);

            if (route == null)
            {
                ModelState.AddModelError(
                    "TravelRouteId",
                    "Please select a valid travel route."
                );

                ViewBag.Route = null;
                return View(booking);
            }

            // Always use fare from the database
            booking.Fare = route.Fare;

            // Server-side booking values
            booking.TicketId = "CT" +
                Guid.NewGuid().ToString("N")[..8].ToUpper();

            booking.BookingDate = DateTime.Now;
            booking.Status = "Confirmed";

            // Validate model before checking seat availability
            if (!ModelState.IsValid)
            {
                ViewBag.Route = route;
                return View(booking);
            }

            // Check whether this seat is already booked
            var travelDay = booking.TravelDate.Date;
            var seatAlreadyBooked = await _context.Bookings
                .AnyAsync(b =>
                    b.TravelRouteId == booking.TravelRouteId &&
                    b.TravelDate.Date == travelDay &&
                    b.SeatNumber == booking.SeatNumber &&
                    b.Status == "Confirmed");

            if (seatAlreadyBooked)
            {
                ModelState.AddModelError(
                    "SeatNumber",
                    "This seat is already booked for this route and travel date."
                );

                ViewBag.Route = route;
                return View(booking);
            }

            // Save booking to database
            _context.Bookings.Add(booking);

            await _context.SaveChangesAsync();
            await TrySendBookingSmsAsync(booking, $"{route.From} to {route.To}");

            return RedirectToAction(nameof(History));
        }

        // GET: Booking/History
        [HttpGet]
        public async Task<IActionResult> History()
        {
            var bookings = await _context.Bookings
                .Include(b => b.TravelRoute)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            return View(bookings);
        }

        // POST: Booking/Cancel
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            booking.Status = "Cancelled";

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(History));
        }


        // AJAX endpoint used by the counter dashboard to save a real database booking.
        [HttpPost]
        public async Task<IActionResult> CreateCounter([FromBody] Booking booking)
        {
            if (booking == null) return BadRequest(new { success = false, message = "Booking details are missing." });
            var route = await _context.TravelRoutes.FirstOrDefaultAsync(r => r.Id == booking.TravelRouteId);
            if (route == null) return BadRequest(new { success = false, message = "Please select a valid route." });
            if (booking.TravelDate == default) return BadRequest(new { success = false, message = "Travel date is required." });
            if (string.IsNullOrWhiteSpace(booking.PassengerName) || string.IsNullOrWhiteSpace(booking.Phone) || string.IsNullOrWhiteSpace(booking.SeatNumber) || string.IsNullOrWhiteSpace(booking.PaymentMethod))
                return BadRequest(new { success = false, message = "Name, phone, seat and payment method are required." });

            booking.SeatNumber = booking.SeatNumber.Trim().ToUpperInvariant();
            var day = booking.TravelDate.Date;
            var taken = await _context.Bookings.AnyAsync(b => b.TravelRouteId == route.Id && b.TravelDate >= day && b.TravelDate < day.AddDays(1) && b.SeatNumber == booking.SeatNumber && b.Status == "Confirmed");
            if (taken) return Conflict(new { success = false, message = "This seat is already booked for this route and date." });

            booking.TicketId = "CT" + Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
            booking.BookingDate = DateTime.Now;
            booking.Status = "Confirmed";
            booking.Fare = route.Fare;
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();
            var smsSent = await TrySendBookingSmsAsync(booking, $"{route.From} to {route.To}");
            return Json(new { success = true, message = smsSent ? "Booking confirmed, saved and SMS submitted." : "Booking confirmed and saved. SMS could not be submitted; check SMS configuration/balance.", ticketId = booking.TicketId, fare = booking.Fare, smsSent });
        }

        // Counter dashboard: bookings made today (local server date).
        [HttpGet]
        public async Task<IActionResult> Today()
        {
            var today = DateTime.Today;
            var bookings = await _context.Bookings
                .Include(b => b.TravelRoute)
                .Where(b => b.BookingDate >= today && b.BookingDate < today.AddDays(1))
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();
            return View("History", bookings);
        }

        [HttpGet]
        public async Task<IActionResult> GetBookingsByDate(DateTime? date)
        {
            var day = (date ?? DateTime.Today).Date;
            var rows = await _context.Bookings.Include(b => b.TravelRoute)
                .Where(b => b.BookingDate >= day && b.BookingDate < day.AddDays(1))
                .OrderByDescending(b => b.BookingDate)
                .Select(b => new { b.TicketId, route = b.TravelRoute!.From + " → " + b.TravelRoute.To, b.SeatNumber, travelDate = b.TravelDate.ToString("yyyy-MM-dd"), b.PassengerName, b.Phone, b.Fare, b.PaymentMethod, b.Status })
                .ToListAsync();
            return Json(rows);
        }

        // Counter cancellation by ticket number; cancelled seats become available.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelByTicket(string ticketId)
        {
            if (string.IsNullOrWhiteSpace(ticketId))
                return BadRequest(new { success = false, message = "Ticket number is required." });

            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.TicketId == ticketId.Trim());
            if (booking == null)
                return NotFound(new { success = false, message = "Ticket number not found." });
            if (booking.Status == "Cancelled")
                return BadRequest(new { success = false, message = "This ticket is already cancelled." });

            booking.Status = "Cancelled";
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Ticket cancelled. Seat is available again." });
        }

        // GET: Booking/GetBookedSeats?routeId=1&travelDate=yyyy-MM-dd
        [HttpGet]
        public async Task<IActionResult> GetBookedSeats(int routeId, DateTime? travelDate)
        {
            // Check whether route exists
            var routeExists = await _context.TravelRoutes
                .AnyAsync(r => r.Id == routeId);

            if (!routeExists)
            {
                return NotFound(new
                {
                    message = "Route not found."
                });
            }

            if (travelDate == null)
                return BadRequest(new { message = "Travel date is required." });

            var travelDay = travelDate.Value.Date;
            var bookedSeats = await _context.Bookings
                .Where(b =>
                    b.TravelRouteId == routeId &&
                    b.TravelDate.Date == travelDay &&
                    b.Status == "Confirmed")
                .Select(b => b.SeatNumber)
                .ToListAsync();

            return Json(bookedSeats);
        }

        private async Task<bool> TrySendBookingSmsAsync(Booking booking, string routeLabel)
        {
            try
            {
                var message = $"Car Ticket Booking: Your ticket {booking.TicketId} is confirmed. Route: {routeLabel}. Travel date: {booking.TravelDate:dd-MM-yyyy}. Seat: {booking.SeatNumber}. Fare: BDT {booking.Fare}.";
                return await _smsSender.SendAsync(booking.Phone, message);
            }
            catch
            {
                // Booking is already saved; an SMS provider failure must not undo it.
                return false;
            }
        }
    }
}