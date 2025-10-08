using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelReservationSystem.Data;
using HotelReservationSystem.Models;

namespace HotelReservationSystem.Controllers
{
    public class BillingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BillingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: Billing/ProcessCheckout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessCheckout(
            string SearchTerm,
            bool Restaurant = false,
            bool RoomService = false,
            bool Laundry = false,
            string PaymentMethod = "Cash")
        {
            var reservation = await _context.Reservations
                .Include(r => r.Customer)
                .Include(r => r.Room)
                .FirstOrDefaultAsync(r =>
                    r.IsCheckedIn && !r.CheckedOutAt.HasValue &&
                    (r.Customer!.FullName.Contains(SearchTerm) || r.Room!.RoomNumber == SearchTerm));

            if (reservation == null)
            {
                TempData["ErrorMessage"] = "Guest not found.";
                return RedirectToAction("CheckOut", "Customer");
            }

            // Calculate charges
            var nights = (DateTime.Today - reservation.ArrivalDate.Date).Days;
            if (nights <= 0) nights = 1;

            decimal roomCharge = reservation.Room!.NightlyRate * nights;
            bool isOverdue = DateTime.Today > reservation.DepartureDate.Date;
            decimal overstayCharge = isOverdue ? reservation.Room.NightlyRate : 0;

            var billing = new BillingRecord
            {
                ReservationId = reservation.ReservationId,
                RoomCharge = roomCharge,
                RestaurantCharge = Restaurant ? 45 : 0,
                RoomServiceCharge = RoomService ? 30 : 0,
                LaundryCharge = Laundry ? 20 : 0,
                OverstayCharge = overstayCharge,
                PaymentMethod = PaymentMethod,
                IsPaid = true,
                BilledAt = DateTime.UtcNow
            };

            _context.BillingRecords.Add(billing);
            reservation.CheckedOutAt = DateTime.UtcNow;
            _context.Reservations.Update(reservation);

            await _context.SaveChangesAsync();

            // Prepare view data
            ViewBag.Bill = new
            {
                FullName = reservation.Customer!.FullName,
                RoomId = reservation.Room.RoomNumber,
                CheckInDate = reservation.ArrivalDate,
                CheckOutDate = DateTime.Today,
                Nights = nights,
                RoomCharge = billing.RoomCharge,
                Restaurant = billing.RestaurantCharge,
                RoomService = billing.RoomServiceCharge,
                Laundry = billing.LaundryCharge,
                OverstayCharge = billing.OverstayCharge,
                TotalAmount = billing.TotalAmount,
                PaymentMethod = billing.PaymentMethod
            };

            return View("CheckoutStatement");
        }
    }
}