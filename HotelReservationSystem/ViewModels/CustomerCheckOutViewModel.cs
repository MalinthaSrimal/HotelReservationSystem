using System.ComponentModel.DataAnnotations;

namespace HotelReservationSystem.ViewModels
{
    public class CustomerCheckOutSearchViewModel
    {
        [Required]
        public string SearchTerm { get; set; } = string.Empty;
    }

    public class CustomerCheckOutBillingViewModel
    {
        public int ReservationId { get; set; }
        public string GuestName { get; set; } = string.Empty;
        public string RoomNumber { get; set; } = string.Empty;
        public DateTime CheckInDate { get; set; }
        public DateTime ScheduledCheckout { get; set; }
        public bool IsOverdue { get; set; }

        // Optional charges
        public bool Restaurant { get; set; }
        public bool RoomService { get; set; }
        public bool Laundry { get; set; }

        [Required]
        public string PaymentMethod { get; set; } = "Cash"; // "Cash" or "CreditCard"
    }
}