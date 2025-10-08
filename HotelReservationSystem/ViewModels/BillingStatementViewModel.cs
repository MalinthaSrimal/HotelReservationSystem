namespace HotelReservationSystem.ViewModels
{
    public class BillingStatementViewModel
    {
        public string GuestName { get; set; } = string.Empty;
        public string RoomNumber { get; set; } = string.Empty;
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int Nights { get; set; }

        public decimal RoomCharge { get; set; }
        public decimal RestaurantCharge { get; set; }
        public decimal RoomServiceCharge { get; set; }
        public decimal LaundryCharge { get; set; }
        public decimal OverstayCharge { get; set; }

        public decimal TotalAmount =>
            RoomCharge + RestaurantCharge + RoomServiceCharge +
            LaundryCharge + OverstayCharge;

        public string PaymentMethod { get; set; } = "Cash";
    }
}