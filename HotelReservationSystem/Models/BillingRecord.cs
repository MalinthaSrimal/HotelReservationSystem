using HotelReservationSystem.Models;
using System.ComponentModel.DataAnnotations.Schema;

public class BillingRecord
{
    public int BillingRecordId { get; set; } // ✅ Now EF recognizes this as PK

    public int ReservationId { get; set; }
    public Reservation Reservation { get; set; } = null!;

    public decimal RoomCharge { get; set; }
    public decimal RestaurantCharge { get; set; } = 0;
    public decimal RoomServiceCharge { get; set; } = 0;
    public decimal LaundryCharge { get; set; } = 0;
    public decimal TelephoneCharge { get; set; } = 0;
    public decimal ClubFacilityCharge { get; set; } = 0;
    public decimal OverstayCharge { get; set; } = 0;

    [NotMapped]
    public decimal TotalAmount =>
        RoomCharge + RestaurantCharge + RoomServiceCharge +
        LaundryCharge + TelephoneCharge + ClubFacilityCharge + OverstayCharge;

    public string PaymentMethod { get; set; } = "Cash";
    public bool IsPaid { get; set; } = false;
    public DateTime BilledAt { get; set; } = DateTime.UtcNow;
    public bool IsNoShowBill { get; set; } = false;
}