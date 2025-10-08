using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelReservationSystem.Data;
using HotelReservationSystem.Models;

namespace HotelReservationSystem.Controllers
{
    public class ReservationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReservationController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Reservation/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Reservation/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            string FullName,
            string Email,
            string RoomType,
            DateTime ArrivalDate,
            DateTime DepartureDate,
            int Occupants,
            bool HasCreditCard = false,
            string? CardNumber = null,
            string? Expiry = null)
            //string? CVV = null)
        {
            if (DepartureDate <= ArrivalDate)
            {
                TempData["ErrorMessage"] = "Departure date must be after arrival date.";
                return View();
            }

            var customer = new Customer
            {
                FullName = FullName,
                Email = Email,
                IdNumber = "TEMP-" + DateTime.Now.Ticks.ToString() // Temporary ID number
            };
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            var reservation = new Reservation
            {
                CustomerId = customer.CustomerId,
                RoomId = 1, // In real app: assign available room or let clerk choose
                ArrivalDate = ArrivalDate,
                DepartureDate = DepartureDate,
                NumberOfOccupants = Occupants,
                HasCreditCard = HasCreditCard,
                CardNumber = HasCreditCard ? CardNumber : null,
                Expiry = HasCreditCard ? Expiry : null,
                //CVV = HasCreditCard ? CVV : null,
                IsConfirmed = HasCreditCard, // Non-CC reservations are pending
                CreatedAt = DateTime.UtcNow,
                ReservationNumber = $"RES-{customer.CustomerId}{DateTime.Now:HHmmss}"
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Reservation created successfully! Your reservation number is {reservation.ReservationNumber}.";
            return RedirectToAction("Index");
        }

        // GET: Reservation
        public async Task<IActionResult> Index()
        {
            try
            {
                // Test basic database connectivity first
                var customerCount = await _context.Customers.CountAsync();
                var roomCount = await _context.Rooms.CountAsync();
                
                ViewBag.DatabaseInfo = $"Customers: {customerCount}, Rooms: {roomCount}";
                
                var reservations = await _context.Reservations
                    .Include(r => r.Customer)
                    .Include(r => r.Room)
                    .OrderByDescending(r => r.CreatedAt)
                    .Take(10)
                    .ToListAsync();

                ViewBag.Reservations = reservations;
                return View();
            }
            catch (Exception ex)
            {
                // Log the error or display it for debugging
                TempData["ErrorMessage"] = $"Database error: {ex.Message}";
                ViewBag.Reservations = new List<Reservation>();
                return View();
            }
        }

        // GET: Reservation/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Customer)
                .Include(r => r.Room)
                .FirstOrDefaultAsync(r => r.ReservationId == id);
                
            if (reservation == null) return NotFound();

            ViewBag.ReservationId = reservation.ReservationId;
            ViewBag.FullName = reservation.Customer?.FullName;
            ViewBag.Email = reservation.Customer?.Email;
            ViewBag.RoomType = reservation.Room?.Type.ToString() ?? "Standard";
            ViewBag.ArrivalDate = reservation.ArrivalDate;
            ViewBag.DepartureDate = reservation.DepartureDate;

            return View();
        }

        // POST: Reservation/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int ReservationId,
            string FullName,
            string Email,
            string RoomType,
            DateTime ArrivalDate,
            DateTime DepartureDate)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Customer)
                .FirstOrDefaultAsync(r => r.ReservationId == ReservationId);

            if (reservation == null) return NotFound();

            if (DepartureDate <= ArrivalDate)
            {
                TempData["ErrorMessage"] = "Invalid dates.";
                return View();
            }

            // Update customer
            reservation.Customer!.FullName = FullName;
            reservation.Customer.Email = Email;

            // Update reservation
            reservation.ArrivalDate = ArrivalDate;
            reservation.DepartureDate = DepartureDate;
            // Note: RoomId assignment logic would need to find room by type
            // For now, keeping the existing room assignment

            _context.Update(reservation);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Reservation updated successfully.";
            return RedirectToAction("Index");
        }

        // GET: Reservation/Cancel/5
        public async Task<IActionResult> Cancel(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Customer)
                .FirstOrDefaultAsync(r => r.ReservationId == id);

            if (reservation == null) return NotFound();

            ViewBag.ReservationId = reservation.ReservationId;
            ViewBag.FullName = reservation.Customer?.FullName;

            return View();
        }

        // POST: Reservation/CancelConfirmed
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelConfirmed(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null) return NotFound();

            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Reservation cancelled successfully.";
            return RedirectToAction("Index");
        }
    }
}