using System.ComponentModel.DataAnnotations;

namespace HotelReservationSystem.ViewModels
{
    public class ReservationCreateViewModel
    {
        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Room type is required.")]
        public string RoomType { get; set; } = "Standard";

        [Required(ErrorMessage = "Arrival date is required.")]
        public DateTime ArrivalDate { get; set; }

        [Required(ErrorMessage = "Departure date is required.")]
        public DateTime DepartureDate { get; set; }

        [Range(1, 6, ErrorMessage = "Number of guests must be between 1 and 6.")]
        public int Occupants { get; set; } = 1;

        public bool HasCreditCard { get; set; }

        [CreditCard]
        public string? CardNumber { get; set; }

        [RegularExpression(@"^(0[1-9]|1[0-2])\/?([0-9]{2})$", ErrorMessage = "Expiry must be MM/YY")]
        public string? Expiry { get; set; }

        // ⚠️ NEVER include CVV in ViewModel or store it!
    }

    public class ReservationEditViewModel
    {
        public int ReservationId { get; set; }

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string RoomType { get; set; } = "Standard";

        [Required]
        public DateTime ArrivalDate { get; set; }

        [Required]
        public DateTime DepartureDate { get; set; }
    }
}