using HotelReservationSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace HotelReservationSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; } = null!;
        public DbSet<Room> Rooms { get; set; } = null!;
        public DbSet<Reservation> Reservations { get; set; } = null!;
        public DbSet<TravelCompany> TravelCompanies { get; set; } = null!;
        public DbSet<BillingRecord> BillingRecords { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure RoomType enum to be stored as string in database
            modelBuilder.Entity<Room>()
                .Property(r => r.Type)
                .HasConversion<string>();

            // Seed sample rooms
            modelBuilder.Entity<Room>().HasData(
                new Room { RoomId = 1, RoomNumber = "101", Type = RoomType.Standard, NightlyRate = 120, IsAvailable = true },
                new Room { RoomId = 2, RoomNumber = "102", Type = RoomType.Standard, NightlyRate = 120, IsAvailable = true },
                new Room { RoomId = 3, RoomNumber = "201", Type = RoomType.Deluxe, NightlyRate = 180, IsAvailable = true },
                new Room { RoomId = 4, RoomNumber = "301", Type = RoomType.Suite, NightlyRate = 300, IsAvailable = true },
                new Room { RoomId = 5, RoomNumber = "R101", Type = RoomType.ResidentialSuite, NightlyRate = 0, WeeklyRate = 700, MonthlyRate = 2500, IsAvailable = true }
            );

            // Indexes for performance
            modelBuilder.Entity<Reservation>()
                .HasIndex(r => r.ArrivalDate);
            modelBuilder.Entity<Reservation>()
                .HasIndex(r => r.IsNoShow);

            modelBuilder.Entity<BillingRecord>()
       .HasKey(b => b.BillingRecordId); // 👈 Explicitly set primary key

            // Optional: configure property if needed
            modelBuilder.Entity<BillingRecord>()
                .Property(b => b.BillingRecordId)
                .ValueGeneratedOnAdd(); // auto-increment

            modelBuilder.Entity<Reservation>()
                .Property(r => r.CreatedAt)
                .HasDefaultValueSql("GETDATE()");
        }
    }
}