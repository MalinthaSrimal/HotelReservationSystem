using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using HotelReservationSystem.Data;
using HotelReservationSystem.Models;

namespace HotelReservationSystem.Services
{
    public class AutoCleanupService : IHostedService, IDisposable
    {
        private Timer? _timer;
        private readonly IServiceProvider _services;
        private readonly ILogger<AutoCleanupService> _logger;

        public AutoCleanupService(IServiceProvider services, ILogger<AutoCleanupService> logger)
        {
            _services = services;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("AutoCleanupService is starting.");

            // Calculate time until next 7:00 PM
            var now = DateTime.Now;
            var nextRun = now.Date.AddHours(19); // 7:00 PM today

            if (now >= nextRun)
                nextRun = nextRun.AddDays(1); // Run tomorrow if already past 7 PM

            var delay = nextRun - now;

            _timer = new Timer(DoWork, null, delay, TimeSpan.FromHours(24));

            return Task.CompletedTask;
        }

        private async void DoWork(object? state)
        {
            _logger.LogInformation("AutoCleanupService executing at {Time}", DateTime.Now);

            using var scope = _services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            try
            {
                await ProcessAutoCancellations(context);
                await ProcessNoShowBilling(context);
                await context.SaveChangesAsync();
                _logger.LogInformation("AutoCleanupService completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AutoCleanupService");
            }
        }

        private async Task ProcessAutoCancellations(ApplicationDbContext context)
        {
            var today = DateTime.Today;

            // Find non-CC reservations for today or past that are still active
            var cancellable = await context.Reservations
                .Where(r => r.HasCreditCard == false
                            && r.ArrivalDate.Date <= today
                            && r.IsCheckedIn == false
                            && r.IsNoShow == false)
                .ToListAsync();

            foreach (var res in cancellable)
            {
                _logger.LogInformation("Auto-cancelling reservation {Id} (no CC, arrival: {Date})",
                    res.ReservationId, res.ArrivalDate);
                context.Reservations.Remove(res);
            }

            _logger.LogInformation("Auto-cancelled {Count} reservations.", cancellable.Count);
        }

        private async Task ProcessNoShowBilling(ApplicationDbContext context)
        {
            var yesterday = DateTime.Today.AddDays(-1);

            // Find guests who should have arrived yesterday but didn't check in
            var noShows = await context.Reservations
                .Where(r => r.ArrivalDate.Date == yesterday
                            && r.IsCheckedIn == false
                            && r.IsNoShow == false
                            && !context.BillingRecords.Any(b => b.ReservationId == r.ReservationId && b.IsNoShowBill))
                .Include(r => r.Room)
                .ToListAsync();

            foreach (var ns in noShows)
            {
                _logger.LogInformation("Billing no-show reservation {Id}", ns.ReservationId);

                var billing = new BillingRecord
                {
                    ReservationId = ns.ReservationId,
                    RoomCharge = ns.Room!.NightlyRate,
                    PaymentMethod = "Auto (No-Show)",
                    IsPaid = true,
                    IsNoShowBill = true,
                    BilledAt = DateTime.Now
                    // TotalAmount is computed — do NOT set it
                };

                context.BillingRecords.Add(billing);
                ns.IsNoShow = true;
            }

            _logger.LogInformation("Billed {Count} no-shows.", noShows.Count);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("AutoCleanupService is stopping.");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}