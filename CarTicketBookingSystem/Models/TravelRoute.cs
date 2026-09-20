using System.ComponentModel.DataAnnotations;

namespace CarTicketBookingSystem.Models
{
    public class TravelRoute
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string From { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string To { get; set; } = string.Empty;

        [Range(1, 100000)]
        public decimal Fare { get; set; }

        [Required]
        [StringLength(30)]
        public string TravelTime { get; set; } = string.Empty;
    }
}