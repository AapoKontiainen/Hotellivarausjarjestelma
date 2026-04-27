using HotelLakeview.Application.Abstractions;
using HotelLakeview.Domain.Entities;
using HotelLakeview.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using Xunit;

namespace HotelLakeview.IntegrationTests;

public sealed class BookingFlowTests : IClassFixture<CustomFactory>
{
    private readonly CustomFactory factory;

    public BookingFlowTests(CustomFactory factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task Api_Creates_Booking_And_Prevents_Overlap()
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<HotelLakeviewDbContext>();

        var roomType = new RoomType { Id = Guid.NewGuid(), Name = "Test Standard", BaseNightlyRate = 119m, MaxGuests = 2, IsActive = true };
        var room = new Room { Id = Guid.NewGuid(), RoomNumber = "201", RoomTypeId = roomType.Id, Status = Domain.Enums.RoomStatus.Active };
        var customer = new Customer { Id = Guid.NewGuid(), FirstName = "Liisa", LastName = "Järvinen", Email = "liisa2@example.com", Phone = "0509999999" };

        dbContext.RoomTypes.Add(roomType);
        dbContext.Rooms.Add(room);
        dbContext.Customers.Add(customer);
        await dbContext.SaveChangesAsync();

        var client = factory.CreateClient();

        var createResponse = await client.PostAsJsonAsync("/api/bookings", new CreateBookingRequest(customer.Id, room.Id, new DateOnly(2026, 5, 10), new DateOnly(2026, 5, 12), 2, null));
        createResponse.EnsureSuccessStatusCode();

        var overlapResponse = await client.PostAsJsonAsync("/api/bookings", new CreateBookingRequest(customer.Id, room.Id, new DateOnly(2026, 5, 11), new DateOnly(2026, 5, 13), 2, null));
        Assert.Equal(System.Net.HttpStatusCode.Conflict, overlapResponse.StatusCode);
    }
}

public sealed class CustomFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(service => service.ServiceType == typeof(DbContextOptions<HotelLakeviewDbContext>));
            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<HotelLakeviewDbContext>(options => options.UseSqlite("DataSource=file:integration?mode=memory&cache=shared"));
        });
    }
}