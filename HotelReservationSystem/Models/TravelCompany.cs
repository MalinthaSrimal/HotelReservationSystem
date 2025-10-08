using System.ComponentModel.DataAnnotations;

namespace HotelReservationSystem.Models
{
    public class TravelCompany
    {
        public int TravelCompanyId { get; set; }

        [Required, MaxLength(100)]
        public string CompanyName { get; set; } = string.Empty;

        [EmailAddress, MaxLength(100)]
        public string? ContactEmail { get; set; }

        [MaxLength(20)]
        public string? ContactPhone { get; set; }

        public decimal DiscountRate { get; set; } = 0.15m; // e.g., 15% off
        public bool IsActive { get; set; } = true;
    }
}