using System.ComponentModel.DataAnnotations;

namespace HotelReservationSystem.Models
{
    public class Customer
    {
        public int CustomerId { get; set; }

        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string IdNumber { get; set; } = string.Empty; // Passport/ID

        public string? PhoneNumber { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}