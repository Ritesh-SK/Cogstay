using Microsoft.EntityFrameworkCore;
using CogStayMVC.Models;

namespace CogStayMVC.Data;

public class HotelDbContext : DbContext
{
    public HotelDbContext(DbContextOptions<HotelDbContext> options) : base(options)
    {
    }

    public DbSet<Guest> Guests { get; set; } = null!;
    public DbSet<Room> Rooms { get; set; } = null!;
    public DbSet<Reservation> Reservations { get; set; } = null!;
    public DbSet<StayRecord> StayRecords { get; set; } = null!;
    public DbSet<HousekeepingTask> HousekeepingTasks { get; set; } = null!;
    public DbSet<Billing> Billings { get; set; } = null!;
    public DbSet<Staff> Staff { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- Guest Configuration ---
        modelBuilder.Entity<Guest>(entity =>
        {
            entity.HasKey(g => g.GuestId);
            entity.Property(g => g.FullName).IsRequired().HasMaxLength(100);
            entity.Property(g => g.Email).IsRequired().HasMaxLength(256);
            entity.Property(g => g.PhoneNumber).IsRequired().HasMaxLength(20);
            entity.Property(g => g.Address).IsRequired().HasMaxLength(500);
            entity.Property(g => g.PasswordHash).IsRequired().HasMaxLength(512);
            
            // Unique constraint on Email
            entity.HasIndex(g => g.Email).IsUnique();
        });

        // --- Room Configuration ---
        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(r => r.RoomId);
            entity.Property(r => r.RoomNumber).IsRequired().HasMaxLength(50);
            entity.Property(r => r.RoomType).IsRequired().HasMaxLength(100);
            entity.Property(r => r.PricePerNight).HasPrecision(18, 2);
            entity.Property(r => r.Status).HasConversion<string>().HasMaxLength(50);

            // Unique constraint on RoomNumber
            entity.HasIndex(r => r.RoomNumber).IsUnique();
        });

        // --- Reservation Configuration ---
        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.HasKey(res => res.ReservationId);
            entity.Property(res => res.ReservationStatus).HasConversion<string>().HasMaxLength(50);

            // One Guest -> Many Reservations
            entity.HasOne(res => res.Guest)
                .WithMany(g => g.Reservations)
                .HasForeignKey(res => res.GuestId)
                .OnDelete(DeleteBehavior.Restrict);

            // One Room -> Many Reservations
            entity.HasOne(res => res.Room)
                .WithMany(r => r.Reservations)
                .HasForeignKey(res => res.RoomId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // --- StayRecord Configuration ---
        modelBuilder.Entity<StayRecord>(entity =>
        {
            entity.HasKey(s => s.StayId);

            // Many StayRecords -> One Guest (One-to-Many)
            entity.HasOne(s => s.Guest)
                .WithMany(g => g.StayRecords)
                .HasForeignKey(s => s.GuestId)
                .OnDelete(DeleteBehavior.Restrict);

            // One Reservation -> One StayRecord (One-to-One)
            entity.HasOne(s => s.Reservation)
                .WithOne(res => res.StayRecord)
                .HasForeignKey<StayRecord>(s => s.ReservationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // --- HousekeepingTask Configuration ---
        modelBuilder.Entity<HousekeepingTask>(entity =>
        {
            entity.HasKey(t => t.TaskId);
            entity.Property(t => t.TaskDescription).IsRequired().HasMaxLength(1000);
            entity.Property(t => t.TaskStatus).HasConversion<string>().HasMaxLength(50);

            // One Room -> Many HousekeepingTasks
            entity.HasOne(t => t.Room)
                .WithMany(r => r.HousekeepingTasks)
                .HasForeignKey(t => t.RoomId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // --- Billing Configuration ---
        modelBuilder.Entity<Billing>(entity =>
        {
            entity.HasKey(b => b.BillId);
            entity.Property(b => b.TotalAmount).HasPrecision(18, 2);
            entity.Property(b => b.PaymentStatus).HasConversion<string>().HasMaxLength(50);
            entity.Property(b => b.Remarks).HasMaxLength(500);

            // One StayRecord -> One Billing (One-to-One)
            entity.HasOne(b => b.StayRecord)
                .WithOne(s => s.Billing)
                .HasForeignKey<Billing>(b => b.StayId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // --- Staff Configuration ---
        modelBuilder.Entity<Staff>(entity =>
        {
            entity.HasKey(s => s.StaffId);
            entity.Property(s => s.FullName).IsRequired().HasMaxLength(100);
            entity.Property(s => s.Email).IsRequired().HasMaxLength(256);
            entity.Property(s => s.PhoneNumber).IsRequired().HasMaxLength(20);
            entity.Property(s => s.PasswordHash).IsRequired().HasMaxLength(512);
            entity.Property(s => s.Role).HasConversion<string>().HasMaxLength(50);

            // Unique constraint on Email
            entity.HasIndex(s => s.Email).IsUnique();
        });
    }
}
