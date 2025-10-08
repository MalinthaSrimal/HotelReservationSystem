using System.ComponentModel.DataAnnotations;

namespace HotelReservationSystem.ViewModels
{
    public class CustomerCheckInViewModel
    {
        public string? ReservationNumber { get; set; } // Optional for walk-ins

        [Required(ErrorMessage = "Full name is required.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "ID/Passport is required.")]
        public string IdNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Room assignment is required.")]
        public string RoomNumber { get; set; } = string.Empty;

        public DateTime CheckoutDate { get; set; } = DateTime.Today.AddDays(1);
    }
}