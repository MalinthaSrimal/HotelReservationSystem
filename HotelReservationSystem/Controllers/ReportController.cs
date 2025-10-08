using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelReservationSystem.Data;
using HotelReservationSystem.Models;

namespace HotelReservationSystem.Controllers
{
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Report/Daily
        public async Task<IActionResult> Daily()
        {
            var reportDate = DateTime.Today.AddDays(-1); // Previous night

            // Occupied rooms (checked in, not checked out)
            var occupiedRooms = await _context.Reservations
                .CountAsync(r => r.IsCheckedIn && r.CheckedOutAt == null);

            // Total revenue (from billing records for reportDate)
            var billingRecords = await _context.BillingRecords
                .Where(b => b.BilledAt.Date == reportDate)
                .ToListAsync();

            var roomRevenue = billingRecords.Sum(b => b.RoomCharge);
            var totalRevenue = billingRecords.Sum(b => b.TotalAmount);

            // No-shows (reservations with arrival = reportDate + 1, not checked in, no CC or expired)
            var noShows = await _context.Reservations
                .Where(r => r.ArrivalDate.Date == reportDate.AddDays(1).Date &&
                           !r.IsCheckedIn &&
                           (!r.HasCreditCard || r.CreatedAt.Date < reportDate))
                .Include(r => r.Customer)
                .Include(r => r.Room)
                .ToListAsync();

            // Auto-bill no-shows (in real app: do this in background service at 7 PM)
            foreach (var ns in noShows)
            {
                if (!await _context.BillingRecords.AnyAsync(b => b.ReservationId == ns.ReservationId && b.IsNoShowBill))
                {
                    var noShowBill = new BillingRecord
                    {
                        ReservationId = ns.ReservationId,
                        RoomCharge = ns.Room!.NightlyRate,
                        // ✅ DO NOT set TotalAmount — it's computed!
                        PaymentMethod = "Auto (No-Show)",
                        IsPaid = true,
                        IsNoShowBill = true,
                        BilledAt = DateTime.Today.AddHours(19) // 7 PM
                    };
                    _context.BillingRecords.Add(noShowBill);
                    ns.IsNoShow = true;
                    _context.Reservations.Update(ns);
                }
            }
            await _context.SaveChangesAsync();

            var noShowItems = noShows.Select(ns => new NoShowItem
            {
                FullName = ns.Customer!.FullName,
                ArrivalDate = ns.ArrivalDate,
                DepartureDate = ns.DepartureDate,
                RoomType = ns.Room!.Type.ToString(),
                Amount = ns.Room.NightlyRate
            }).ToList();

            var report = new DailyReport
            {
                ReportDate = reportDate,
                TotalRooms = 50, // or get from DB
                OccupiedRooms = occupiedRooms,
                RoomRevenue = roomRevenue,
                TotalRevenue = totalRevenue,
                NoShowRevenue = noShowItems.Sum(n => n.Amount),
                NoShowCount = noShowItems.Count,
                NoShows = noShowItems
            };

            ViewBag.ReportDate = report.ReportDate;
            ViewBag.OccupiedRooms = report.OccupiedRooms;
            ViewBag.TotalRooms = report.TotalRooms;
            ViewBag.OccupancyRate = report.OccupancyRate;
            ViewBag.TotalRevenue = report.TotalRevenue;
            ViewBag.RoomRevenue = report.RoomRevenue;
            ViewBag.NoShowRevenue = report.NoShowRevenue;
            ViewBag.NoShowCount = report.NoShowCount;
            ViewBag.NoShows = report.NoShows;

            return View();
        }
    }
}