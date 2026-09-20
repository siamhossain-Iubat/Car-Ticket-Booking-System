
using CarTicketBookingSystem.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarTicketBookingSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var routes = await _context.TravelRoutes
                .OrderBy(route => route.Id)
                .ToListAsync();

            return View(routes);
        }
    }
}