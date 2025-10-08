using System.ComponentModel.DataAnnotations;

namespace HotelReservationSystem.Models
{
    public class Room
    {
        public int RoomId { get; set; }

        [Required, MaxLength(10)]
        public string RoomNumber { get; set; } = string.Empty;

        public RoomType Type { get; set; }
        public decimal NightlyRate { get; set; }
        public decimal WeeklyRate { get; set; } // For ResidentialSuite
        public decimal MonthlyRate { get; set; } // For ResidentialSuite
        public bool IsAvailable { get; set; } = true;
        public string? Description { get; set; }
    }
}