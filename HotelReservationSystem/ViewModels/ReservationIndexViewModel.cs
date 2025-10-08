namespace HotelReservationSystem.ViewModels
{
    public class ReservationIndexViewModel
    {
        public int ReservationId { get; set; }
        public string ReservationNumber { get; set; } = string.Empty;
        public string GuestName { get; set; } = string.Empty;
        public string RoomType { get; set; } = string.Empty;
        public DateTime ArrivalDate { get; set; }
        public DateTime DepartureDate { get; set; }
        public bool IsConfirmed { get; set; }
        public int Nights => (DepartureDate.Date - ArrivalDate.Date).Days;
    }
}