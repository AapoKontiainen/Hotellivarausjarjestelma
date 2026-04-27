using HotelLakeview.Application.Abstractions;
using HotelLakeview.Application.Services;
using HotelLakeview.Domain.Entities;
using HotelLakeview.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HotelLakeview.Infrastructure.Persistence;

public sealed class HotelLakeviewDbContext(DbContextOptions<HotelLakeviewDbContext> options) : DbContext(options), IHotelDbContext
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<RoomType> RoomTypes => Set<RoomType>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<RoomImage> RoomImages => Set<RoomImage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasIndex(customer => customer.Email).IsUnique();
            entity.Property(customer => customer.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(customer => customer.LastName).HasMaxLength(100).IsRequired();
            entity.Property(customer => customer.Email).HasMaxLength(200).IsRequired();
            entity.Property(customer => customer.Phone).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<RoomType>(entity =>
        {
            entity.HasIndex(roomType => roomType.Name).IsUnique();
            entity.Property(roomType => roomType.Name).HasMaxLength(100).IsRequired();
            entity.Property(roomType => roomType.Description).HasMaxLength(1000);
            entity.Property(roomType => roomType.BaseNightlyRate).HasPrecision(10, 2);
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasIndex(room => room.RoomNumber).IsUnique();
            entity.Property(room => room.RoomNumber).HasMaxLength(20).IsRequired();
            entity.HasOne(room => room.RoomType)
                .WithMany(roomType => roomType.Rooms)
                .HasForeignKey(room => room.RoomTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.Property(booking => booking.TotalPrice).HasPrecision(10, 2);
            entity.Property(booking => booking.NightlyRateSnapshot).HasPrecision(10, 2);
            entity.HasOne(booking => booking.Customer)
                .WithMany(customer => customer.Bookings)
                .HasForeignKey(booking => booking.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(booking => booking.Room)
                .WithMany(room => room.Bookings)
                .HasForeignKey(booking => booking.RoomId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(booking => new { booking.RoomId, booking.CheckInDate, booking.CheckOutDate });
        });

        modelBuilder.Entity<RoomImage>(entity =>
        {
            entity.Property(image => image.FileName).HasMaxLength(255).IsRequired();
            entity.Property(image => image.ContentType).HasMaxLength(100).IsRequired();
            entity.Property(image => image.StoragePath).HasMaxLength(500).IsRequired();
            entity.HasOne(image => image.Room)
                .WithMany(room => room.Images)
                .HasForeignKey(image => image.RoomId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        Seed(modelBuilder);
    }

    private static void Seed(ModelBuilder modelBuilder)
    {
        var roomTypes = new[]
        {
            new RoomType { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "Economy", BaseNightlyRate = 79m, MaxGuests = 1, IsActive = true, CreatedAtUtc = DateTime.UtcNow, UpdatedAtUtc = DateTime.UtcNow },
            new RoomType { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "Standard", BaseNightlyRate = 119m, MaxGuests = 2, IsActive = true, CreatedAtUtc = DateTime.UtcNow, UpdatedAtUtc = DateTime.UtcNow },
            new RoomType { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Name = "Superior", BaseNightlyRate = 159m, MaxGuests = 2, IsActive = true, CreatedAtUtc = DateTime.UtcNow, UpdatedAtUtc = DateTime.UtcNow },
            new RoomType { Id = Guid.Parse("44444444-4444-4444-4444-444444444444"), Name = "Junior Suite", BaseNightlyRate = 219m, MaxGuests = 3, IsActive = true, CreatedAtUtc = DateTime.UtcNow, UpdatedAtUtc = DateTime.UtcNow },
            new RoomType { Id = Guid.Parse("55555555-5555-5555-5555-555555555555"), Name = "Suite", BaseNightlyRate = 319m, MaxGuests = 4, IsActive = true, CreatedAtUtc = DateTime.UtcNow, UpdatedAtUtc = DateTime.UtcNow }
        };

        modelBuilder.Entity<RoomType>().HasData(roomTypes);

        var rooms = new List<Room>();
        AddRooms(rooms, "E", 8, roomTypes[0].Id);
        AddRooms(rooms, "S", 10, roomTypes[1].Id);
        AddRooms(rooms, "U", 6, roomTypes[2].Id);
        AddRooms(rooms, "J", 4, roomTypes[3].Id);
        AddRooms(rooms, "SU", 2, roomTypes[4].Id);

        modelBuilder.Entity<Room>().HasData(rooms);
    }

    private static void AddRooms(ICollection<Room> rooms, string prefix, int count, Guid roomTypeId)
    {
        for (var index = 1; index <= count; index++)
        {
            rooms.Add(new Room
            {
                Id = Guid.NewGuid(),
                RoomNumber = $"{prefix}{index:00}",
                RoomTypeId = roomTypeId,
                Status = RoomStatus.Active,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            });
        }
    }
}

public static class ServiceRegistration
{
    public static IServiceCollection AddHotelLakeviewInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<HotelLakeviewDbContext>(options => options.UseSqlite(connectionString));
        services.AddScoped<IHotelDbContext>(provider => provider.GetRequiredService<HotelLakeviewDbContext>());
        services.AddScoped<IPricingService, PricingService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IRoomService, RoomService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        return services;
    }
}
