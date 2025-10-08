using HotelReservationSystem.Models;
using System.ComponentModel.DataAnnotations;

public class Reservation
{
    public int ReservationId { get; set; }

    [Required]
    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }

    [Required]
    public int RoomId { get; set; }
    public Room? Room { get; set; }

    public string? ReservationNumber { get; set; }

    [Required]
    public DateTime ArrivalDate { get; set; }

    [Required]
    public DateTime DepartureDate { get; set; }

    public int NumberOfOccupants { get; set; } = 1;

    public bool HasCreditCard { get; set; } = false;
    public string? CardNumber { get; set; }
    public string? Expiry { get; set; }
    // 🔒 REMOVED: public string? CVV { get; set; }  ← NEVER STORE CVV!

    public bool IsConfirmed { get; set; } = true;
    public bool IsCheckedIn { get; set; } = false;
    public bool IsNoShow { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // ← EF expects this column

    public DateTime? CheckedInAt { get; set; }
    public DateTime? CheckedOutAt { get; set; }

    public int? TravelCompanyId { get; set; }
    public TravelCompany? TravelCompany { get; set; }

    public int Nights => (DepartureDate.Date - ArrivalDate.Date).Days;
}