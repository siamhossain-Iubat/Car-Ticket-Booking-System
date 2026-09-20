
using System.ComponentModel.DataAnnotations;

namespace CarTicketBookingSystem.Models
{
    public class Booking
    {
        public int Id { get; set; }

        [Required]
        [StringLength(30)]
        public string TicketId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string PassengerName { get; set; } = string.Empty;

        [Required]
        [Phone]
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string SeatNumber { get; set; } = string.Empty;

        [Range(1, 100000)]
        public decimal Fare { get; set; }

        [Required]
        [StringLength(30)]
        public string PaymentMethod { get; set; } = string.Empty;

        [Required]
        public DateTime TravelDate { get; set; }

        public DateTime BookingDate { get; set; } = DateTime.Now;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Confirmed";

        public int TravelRouteId { get; set; }

        public TravelRoute? TravelRoute { get; set; }
    }
}