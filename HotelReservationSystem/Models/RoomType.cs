using System.ComponentModel.DataAnnotations;

namespace HotelReservationSystem.Models
{
    public enum RoomType
    {
        Standard,
        Deluxe,
        Suite,
        ResidentialSuite // For weekly/monthly stays
    }
}