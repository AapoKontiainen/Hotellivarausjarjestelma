using HotelLakeview.Domain.Entities;
using HotelLakeview.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HotelLakeview.Application.Abstractions;

public interface IHotelDbContext
{
    DbSet<Customer> Customers { get; }
    DbSet<RoomType> RoomTypes { get; }
    DbSet<Room> Rooms { get; }
    DbSet<Booking> Bookings { get; }
    DbSet<RoomImage> RoomImages { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IPricingService
{
    decimal CalculateNightlyRate(RoomType roomType, DateOnly date);
    decimal CalculateTotal(RoomType roomType, DateOnly checkInDate, DateOnly checkOutDate);
}

public interface ICustomerService
{
    Task<IReadOnlyList<CustomerDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CustomerDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CustomerDto> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default);
    Task<CustomerDto?> UpdateAsync(Guid id, UpdateCustomerRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IRoomService
{
    Task<IReadOnlyList<RoomTypeDto>> GetRoomTypesAsync(CancellationToken cancellationToken = default);
    Task<RoomTypeDto> CreateRoomTypeAsync(CreateRoomTypeRequest request, CancellationToken cancellationToken = default);
    Task<RoomTypeDto?> UpdateRoomTypeAsync(Guid id, UpdateRoomTypeRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteRoomTypeAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RoomDto>> GetRoomsAsync(CancellationToken cancellationToken = default);
    Task<RoomDto> CreateRoomAsync(CreateRoomRequest request, CancellationToken cancellationToken = default);
    Task<RoomDto?> UpdateRoomAsync(Guid id, UpdateRoomRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteRoomAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IBookingService
{
    Task<IReadOnlyList<AvailabilityResultDto>> SearchAvailabilityAsync(AvailabilitySearchRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BookingDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<BookingDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<BookingDto> CreateAsync(CreateBookingRequest request, CancellationToken cancellationToken = default);
    Task<BookingDto?> UpdateAsync(Guid id, UpdateBookingRequest request, CancellationToken cancellationToken = default);
    Task<bool> CancelAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IAnalyticsService
{
    Task<IReadOnlyList<OccupancyPointDto>> GetOccupancyAsync(DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RevenuePointDto>> GetRevenueAsync(DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PopularRoomTypeDto>> GetPopularRoomTypesAsync(DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken = default);
}

public sealed record CustomerDto(Guid Id, string FirstName, string LastName, string Email, string Phone, string? Notes);
public sealed record RoomTypeDto(Guid Id, string Name, string? Description, decimal BaseNightlyRate, int MaxGuests, bool IsActive);
public sealed record RoomDto(Guid Id, string RoomNumber, Guid RoomTypeId, string RoomTypeName, RoomStatus Status, bool IsActive);
public sealed record BookingDto(Guid Id, Guid CustomerId, string CustomerName, Guid RoomId, string RoomNumber, Guid RoomTypeId, string RoomTypeName, DateOnly CheckInDate, DateOnly CheckOutDate, BookingStatus Status, decimal NightlyRateSnapshot, decimal TotalPrice, string? SpecialRequests, int GuestCount);
public sealed record AvailabilityResultDto(Guid RoomId, string RoomNumber, Guid RoomTypeId, string RoomTypeName, decimal NightlyRate, decimal TotalPrice);
public sealed record OccupancyPointDto(DateOnly Date, int BookedRooms, int TotalRooms, decimal OccupancyPercent);
public sealed record RevenuePointDto(string Period, decimal Revenue);
public sealed record PopularRoomTypeDto(Guid RoomTypeId, string RoomTypeName, int BookingCount, decimal Revenue);

public sealed record CreateCustomerRequest(string FirstName, string LastName, string Email, string Phone, string? Notes);
public sealed record UpdateCustomerRequest(string FirstName, string LastName, string Email, string Phone, string? Notes);
public sealed record CreateRoomTypeRequest(string Name, string? Description, decimal BaseNightlyRate, int MaxGuests);
public sealed record UpdateRoomTypeRequest(string Name, string? Description, decimal BaseNightlyRate, int MaxGuests, bool IsActive);
public sealed record CreateRoomRequest(string RoomNumber, Guid RoomTypeId);
public sealed record UpdateRoomRequest(string RoomNumber, Guid RoomTypeId, RoomStatus Status);
public sealed record CreateBookingRequest(Guid CustomerId, Guid RoomId, DateOnly CheckInDate, DateOnly CheckOutDate, int GuestCount, string? SpecialRequests);
public sealed record UpdateBookingRequest(Guid RoomId, DateOnly CheckInDate, DateOnly CheckOutDate, int GuestCount, string? SpecialRequests);
public sealed record AvailabilitySearchRequest(DateOnly CheckInDate, DateOnly CheckOutDate, int? GuestCount, Guid? RoomTypeId);
