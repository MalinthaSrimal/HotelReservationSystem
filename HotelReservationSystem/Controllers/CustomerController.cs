using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelReservationSystem.Data;
using HotelReservationSystem.Models;

namespace HotelReservationSystem.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Customer/CheckIn
        public IActionResult CheckIn()
        {
            return View();
        }

        // POST: Customer/CheckIn
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckIn(
            string? ReservationId,
            string FullName,
            string IdNumber,
            string RoomId,
            DateTime? CheckoutDate)
        {
            Customer? customer = null;

            if (!string.IsNullOrEmpty(ReservationId))
            {
                // Find by reservation
                var reservation = await _context.Reservations
                    .Include(r => r.Customer)
                    .FirstOrDefaultAsync(r => r.ReservationNumber == ReservationId);
                if (reservation != null)
                {
                    customer = reservation.Customer;
                    reservation.IsCheckedIn = true;
                    reservation.CheckedInAt = DateTime.UtcNow;
                    _context.Reservations.Update(reservation);
                }
            }

            if (customer == null)
            {
                // Walk-in: create new customer
                customer = new Customer
                {
                    FullName = FullName,
                    IdNumber = IdNumber
                };
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = $"Guest {customer.FullName} checked in to Room {RoomId}.";
            return RedirectToAction("CheckOut");
        }

        // GET: Customer/CheckOut
        public IActionResult CheckOut()
        {
            return View();
        }

        // POST: Customer/CheckOut (search)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckOut(string SearchTerm, string? action)
        {
            if (action != "process")
            {
                // Search guest
                var guest = await _context.Reservations
                    .Include(r => r.Customer)
                    .Include(r => r.Room)
                    .Where(r => r.IsCheckedIn && !r.CheckedOutAt.HasValue)
                    .FirstOrDefaultAsync(r =>
                        r.Customer!.FullName.Contains(SearchTerm) ||
                        r.Room!.RoomNumber == SearchTerm);

                if (guest == null)
                {
                    TempData["ErrorMessage"] = "Guest not found.";
                    return View();
                }

                ViewBag.Guest = guest;
                ViewBag.IsOverdue = DateTime.Today > guest.DepartureDate.Date;
                return View();
            }

            return View();
        }
    }
}