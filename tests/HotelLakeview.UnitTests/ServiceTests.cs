using HotelLakeview.Application.Abstractions;
using HotelLakeview.Application.Services;
using HotelLakeview.Domain.Entities;
using HotelLakeview.Domain.Enums;
using HotelLakeview.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HotelLakeview.UnitTests;

public sealed class ServiceTests
{
    [Fact]
    public void PricingService_Applies_SummerMultiplier()
    {
        var service = new PricingService();
        var roomType = new RoomType { BaseNightlyRate = 100m };

        var price = service.CalculateNightlyRate(roomType, new DateOnly(2026, 7, 1));

        Assert.Equal(130m, price);
    }

    [Fact]
    public void PricingService_Applies_YearEndMultiplier()
    {
        var service = new PricingService();
        var roomType = new RoomType { BaseNightlyRate = 100m };

        var price = service.CalculateNightlyRate(roomType, new DateOnly(2026, 12, 25));

        Assert.Equal(130m, price);
    }

    [Fact]
    public async Task BookingService_Creates_Booking_And_Calculates_Total()
    {
        using var fixture = await CreateFixtureAsync();

        var booking = await fixture.BookingService.CreateAsync(new CreateBookingRequest(
            fixture.CustomerId,
            fixture.RoomId,
            new DateOnly(2026, 6, 1),
            new DateOnly(2026, 6, 3),
            2,
            null));

        Assert.Equal(260m, booking.TotalPrice);
    }

    [Fact]
    public async Task BookingService_Detects_Overlap()
    {
        using var fixture = await CreateFixtureAsync();

        await fixture.BookingService.CreateAsync(new CreateBookingRequest(
            fixture.CustomerId,
            fixture.RoomId,
            new DateOnly(2026, 6, 1),
            new DateOnly(2026, 6, 3),
            2,
            null));

        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.BookingService.CreateAsync(new CreateBookingRequest(
            fixture.CustomerId,
            fixture.RoomId,
            new DateOnly(2026, 6, 2),
            new DateOnly(2026, 6, 4),
            2,
            null)));
    }

    [Fact]
    public async Task BookingService_Returns_Available_Room_When_Cancelled()
    {
        using var fixture = await CreateFixtureAsync();

        var created = await fixture.BookingService.CreateAsync(new CreateBookingRequest(
            fixture.CustomerId,
            fixture.RoomId,
            new DateOnly(2026, 6, 1),
            new DateOnly(2026, 6, 3),
            2,
            null));

        await fixture.BookingService.CancelAsync(created.Id);

        var results = await fixture.BookingService.SearchAvailabilityAsync(new AvailabilitySearchRequest(
            new DateOnly(2026, 6, 1),
            new DateOnly(2026, 6, 3),
            2,
            null));

        Assert.Contains(results, result => result.RoomId == fixture.RoomId);
    }

    private static async Task<TestFixture> CreateFixtureAsync()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<HotelLakeviewDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new HotelLakeviewDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var roomType = new RoomType { Id = Guid.NewGuid(), Name = "Test Standard", BaseNightlyRate = 100m, MaxGuests = 2, IsActive = true };
        var room = new Room { Id = Guid.NewGuid(), RoomNumber = "101", RoomTypeId = roomType.Id, Status = RoomStatus.Active };
        var customer = new Customer { Id = Guid.NewGuid(), FirstName = "Liisa", LastName = "Järvinen", Email = "liisa@example.com", Phone = "0501234567" };

        context.RoomTypes.Add(roomType);
        context.Rooms.Add(room);
        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        return new TestFixture(connection, context, new BookingService(context, new PricingService()), customer.Id, room.Id);
    }

    private sealed class TestFixture(SqliteConnection connection, HotelLakeviewDbContext context, BookingService bookingService, Guid customerId, Guid roomId) : IDisposable
    {
        public BookingService BookingService { get; } = bookingService;
        public Guid CustomerId { get; } = customerId;
        public Guid RoomId { get; } = roomId;

        public void Dispose()
        {
            context.Dispose();
            connection.Dispose();
        }
    }
}
