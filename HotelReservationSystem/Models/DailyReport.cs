namespace HotelReservationSystem.Models
{
    public class DailyReport
    {
        public DateTime ReportDate { get; set; }
        public int TotalRooms { get; set; } = 50; // configurable
        public int OccupiedRooms { get; set; }
        public decimal OccupancyRate => TotalRooms > 0 ? Math.Round((decimal)OccupiedRooms / TotalRooms * 100, 2) : 0;
        public decimal TotalRevenue { get; set; }
        public decimal RoomRevenue { get; set; }
        public decimal NoShowRevenue { get; set; }
        public int NoShowCount { get; set; }
        public List<NoShowItem> NoShows { get; set; } = new();
    }

    public class NoShowItem
    {
        public string FullName { get; set; } = string.Empty;
        public DateTime ArrivalDate { get; set; }
        public DateTime DepartureDate { get; set; }
        public string RoomType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}