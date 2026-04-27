using HotelLakeview.Application.Abstractions;
using HotelLakeview.Domain.Entities;
using HotelLakeview.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HotelLakeview.Application.Services;

public sealed class PricingService : IPricingService
{
    public decimal CalculateNightlyRate(RoomType roomType, DateOnly date)
    {
        var multiplier = IsHighSeason(date) ? 1.30m : 1.0m;
        return decimal.Round(roomType.BaseNightlyRate * multiplier, 2, MidpointRounding.AwayFromZero);
    }

    public decimal CalculateTotal(RoomType roomType, DateOnly checkInDate, DateOnly checkOutDate)
    {
        if (checkOutDate <= checkInDate)
        {
            throw new ArgumentException("Check-out date must be after check-in date.");
        }

        var total = 0m;
        for (var day = checkInDate; day < checkOutDate; day = day.AddDays(1))
        {
            total += CalculateNightlyRate(roomType, day);
        }

        return decimal.Round(total, 2, MidpointRounding.AwayFromZero);
    }

    private static bool IsHighSeason(DateOnly date)
        => (date.Month >= 6 && date.Month <= 8)
           || (date.Month == 12 && date.Day >= 20)
           || (date.Month == 1 && date.Day <= 6);
}

public sealed class CustomerService(IHotelDbContext dbContext) : ICustomerService
{
    public async Task<IReadOnlyList<CustomerDto>> GetAllAsync(CancellationToken cancellationToken = default)
        => await dbContext.Customers.AsNoTracking()
            .OrderBy(customer => customer.LastName)
            .ThenBy(customer => customer.FirstName)
            .Select(customer => new CustomerDto(customer.Id, customer.FirstName, customer.LastName, customer.Email, customer.Phone, customer.Notes))
            .ToListAsync(cancellationToken);

    public async Task<CustomerDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await dbContext.Customers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return customer is null ? null : new CustomerDto(customer.Id, customer.FirstName, customer.LastName, customer.Email, customer.Phone, customer.Notes);
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var customer = new Customer
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            Phone = request.Phone.Trim(),
            Notes = request.Notes?.Trim()
        };

        dbContext.Customers.Add(customer);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new CustomerDto(customer.Id, customer.FirstName, customer.LastName, customer.Email, customer.Phone, customer.Notes);
    }

    public async Task<CustomerDto?> UpdateAsync(Guid id, UpdateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var customer = await dbContext.Customers.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (customer is null)
        {
            return null;
        }

        customer.FirstName = request.FirstName.Trim();
        customer.LastName = request.LastName.Trim();
        customer.Email = request.Email.Trim().ToLowerInvariant();
        customer.Phone = request.Phone.Trim();
        customer.Notes = request.Notes?.Trim();
        customer.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return new CustomerDto(customer.Id, customer.FirstName, customer.LastName, customer.Email, customer.Phone, customer.Notes);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await dbContext.Customers.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (customer is null)
        {
            return false;
        }

        var hasBookings = await dbContext.Bookings.AsNoTracking()
            .AnyAsync(booking => booking.CustomerId == id, cancellationToken);

        if (hasBookings)
        {
            throw new InvalidOperationException("Customer cannot be deleted because bookings exist for this customer.");
        }

        dbContext.Customers.Remove(customer);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public sealed class RoomService(IHotelDbContext dbContext) : IRoomService
{
    public async Task<IReadOnlyList<RoomTypeDto>> GetRoomTypesAsync(CancellationToken cancellationToken = default)
        => await dbContext.RoomTypes.AsNoTracking()
            .Where(roomType => roomType.IsActive)
            .OrderBy(roomType => roomType.Name)
            .Select(roomType => new RoomTypeDto(roomType.Id, roomType.Name, roomType.Description, roomType.BaseNightlyRate, roomType.MaxGuests, roomType.IsActive))
            .ToListAsync(cancellationToken);

    public async Task<RoomTypeDto> CreateRoomTypeAsync(CreateRoomTypeRequest request, CancellationToken cancellationToken = default)
    {
        var roomType = new RoomType
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            BaseNightlyRate = request.BaseNightlyRate,
            MaxGuests = request.MaxGuests,
            IsActive = true
        };

        dbContext.RoomTypes.Add(roomType);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new RoomTypeDto(roomType.Id, roomType.Name, roomType.Description, roomType.BaseNightlyRate, roomType.MaxGuests, roomType.IsActive);
    }

    public async Task<RoomTypeDto?> UpdateRoomTypeAsync(Guid id, UpdateRoomTypeRequest request, CancellationToken cancellationToken = default)
    {
        var roomType = await dbContext.RoomTypes.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (roomType is null)
        {
            return null;
        }

        roomType.Name = request.Name.Trim();
        roomType.Description = request.Description?.Trim();
        roomType.BaseNightlyRate = request.BaseNightlyRate;
        roomType.MaxGuests = request.MaxGuests;
        roomType.IsActive = request.IsActive;
        roomType.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return new RoomTypeDto(roomType.Id, roomType.Name, roomType.Description, roomType.BaseNightlyRate, roomType.MaxGuests, roomType.IsActive);
    }

    public async Task<bool> DeleteRoomTypeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var roomType = await dbContext.RoomTypes.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (roomType is null)
        {
            return false;
        }

        roomType.IsActive = false;
        roomType.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<RoomDto>> GetRoomsAsync(CancellationToken cancellationToken = default)
        => await dbContext.Rooms.AsNoTracking()
            .Include(room => room.RoomType)
            .Where(room => room.Status == RoomStatus.Active && room.RoomType != null && room.RoomType.IsActive)
            .OrderBy(room => room.RoomNumber)
            .Select(room => new RoomDto(room.Id, room.RoomNumber, room.RoomTypeId, room.RoomType!.Name, room.Status, room.Status == RoomStatus.Active))
            .ToListAsync(cancellationToken);

    public async Task<RoomDto> CreateRoomAsync(CreateRoomRequest request, CancellationToken cancellationToken = default)
    {
        var roomType = await dbContext.RoomTypes.FirstOrDefaultAsync(roomType => roomType.Id == request.RoomTypeId && roomType.IsActive, cancellationToken)
            ?? throw new InvalidOperationException("Room type not found.");

        var room = new Room
        {
            RoomNumber = request.RoomNumber.Trim(),
            RoomTypeId = roomType.Id,
            Status = RoomStatus.Active
        };

        dbContext.Rooms.Add(room);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new RoomDto(room.Id, room.RoomNumber, room.RoomTypeId, roomType.Name, room.Status, true);
    }

    public async Task<RoomDto?> UpdateRoomAsync(Guid id, UpdateRoomRequest request, CancellationToken cancellationToken = default)
    {
        var room = await dbContext.Rooms.Include(room => room.RoomType).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (room is null)
        {
            return null;
        }

        var roomType = await dbContext.RoomTypes.FirstOrDefaultAsync(roomType => roomType.Id == request.RoomTypeId && roomType.IsActive, cancellationToken)
            ?? throw new InvalidOperationException("Room type not found.");

        room.RoomNumber = request.RoomNumber.Trim();
        room.RoomTypeId = roomType.Id;
        room.Status = request.Status;
        room.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return new RoomDto(room.Id, room.RoomNumber, room.RoomTypeId, roomType.Name, room.Status, room.Status == RoomStatus.Active);
    }

    public async Task<bool> DeleteRoomAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var room = await dbContext.Rooms.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (room is null)
        {
            return false;
        }

        room.Status = RoomStatus.Inactive;
        room.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public sealed class BookingService(IHotelDbContext dbContext, IPricingService pricingService) : IBookingService
{
    public async Task<IReadOnlyList<AvailabilityResultDto>> SearchAvailabilityAsync(AvailabilitySearchRequest request, CancellationToken cancellationToken = default)
    {
        ValidateDates(request.CheckInDate, request.CheckOutDate);

        var rooms = await dbContext.Rooms.AsNoTracking()
            .Include(room => room.RoomType)
            .Where(room => room.Status == RoomStatus.Active && room.RoomType!.IsActive)
            .ToListAsync(cancellationToken);

        if (request.RoomTypeId is not null)
        {
            rooms = rooms.Where(room => room.RoomTypeId == request.RoomTypeId.Value).ToList();
        }

        if (request.GuestCount is not null)
        {
            rooms = rooms.Where(room => room.RoomType!.MaxGuests >= request.GuestCount.Value).ToList();
        }

        var overlappingRoomIds = await dbContext.Bookings.AsNoTracking()
            .Where(booking => booking.Status != BookingStatus.Cancelled && booking.CheckInDate < request.CheckOutDate && booking.CheckOutDate > request.CheckInDate)
            .Select(booking => booking.RoomId)
            .Distinct()
            .ToListAsync(cancellationToken);

        return rooms
            .Where(room => !overlappingRoomIds.Contains(room.Id))
            .Select(room => new AvailabilityResultDto(
                room.Id,
                room.RoomNumber,
                room.RoomTypeId,
                room.RoomType!.Name,
                pricingService.CalculateNightlyRate(room.RoomType, request.CheckInDate),
                pricingService.CalculateTotal(room.RoomType, request.CheckInDate, request.CheckOutDate)))
            .OrderBy(room => room.RoomNumber)
            .ToList();
    }

    public async Task<IReadOnlyList<BookingDto>> GetAllAsync(CancellationToken cancellationToken = default)
        => await dbContext.Bookings.AsNoTracking()
            .Where(booking => booking.Status != BookingStatus.Cancelled)
            .Include(booking => booking.Customer)
            .Include(booking => booking.Room)
                .ThenInclude(room => room.RoomType)
            .OrderByDescending(booking => booking.CheckInDate)
            .Select(booking => MapBookingDto(booking))
            .ToListAsync(cancellationToken);

    public async Task<BookingDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var booking = await dbContext.Bookings.AsNoTracking()
            .Where(item => item.Status != BookingStatus.Cancelled)
            .Include(item => item.Customer)
            .Include(item => item.Room)
                .ThenInclude(room => room.RoomType)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        return booking is null ? null : MapBookingDto(booking);
    }

    public async Task<BookingDto> CreateAsync(CreateBookingRequest request, CancellationToken cancellationToken = default)
    {
        ValidateDates(request.CheckInDate, request.CheckOutDate);

        var customer = await dbContext.Customers.FirstOrDefaultAsync(customer => customer.Id == request.CustomerId, cancellationToken)
            ?? throw new InvalidOperationException("Customer not found.");

        var room = await dbContext.Rooms.Include(room => room.RoomType).FirstOrDefaultAsync(room => room.Id == request.RoomId, cancellationToken)
            ?? throw new InvalidOperationException("Room not found.");

        if (room.Status != RoomStatus.Active || room.RoomType is null || !room.RoomType.IsActive)
        {
            throw new InvalidOperationException("Room is not available for booking.");
        }

        if (request.GuestCount > room.RoomType.MaxGuests)
        {
            throw new InvalidOperationException("Guest count exceeds room capacity.");
        }

        var hasConflict = await HasOverlapAsync(room.Id, request.CheckInDate, request.CheckOutDate, null, cancellationToken);
        if (hasConflict)
        {
            throw new InvalidOperationException("Room already has an overlapping booking.");
        }

        var booking = new Booking
        {
            CustomerId = customer.Id,
            RoomId = room.Id,
            RoomTypeId = room.RoomTypeId,
            CheckInDate = request.CheckInDate,
            CheckOutDate = request.CheckOutDate,
            GuestCount = request.GuestCount,
            SpecialRequests = request.SpecialRequests?.Trim(),
            Status = BookingStatus.Confirmed,
            NightlyRateSnapshot = room.RoomType.BaseNightlyRate,
            TotalPrice = pricingService.CalculateTotal(room.RoomType, request.CheckInDate, request.CheckOutDate)
        };

        dbContext.Bookings.Add(booking);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new BookingDto(
            booking.Id,
            customer.Id,
            $"{customer.FirstName} {customer.LastName}",
            room.Id,
            room.RoomNumber,
            room.RoomTypeId,
            room.RoomType.Name,
            booking.CheckInDate,
            booking.CheckOutDate,
            booking.Status,
            booking.NightlyRateSnapshot,
            booking.TotalPrice,
            booking.SpecialRequests,
            booking.GuestCount);
    }

    public async Task<BookingDto?> UpdateAsync(Guid id, UpdateBookingRequest request, CancellationToken cancellationToken = default)
    {
        ValidateDates(request.CheckInDate, request.CheckOutDate);

        var booking = await dbContext.Bookings.Include(item => item.Customer).Include(item => item.Room).ThenInclude(room => room.RoomType).FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (booking is null)
        {
            return null;
        }

        var room = await dbContext.Rooms.Include(item => item.RoomType).FirstOrDefaultAsync(item => item.Id == request.RoomId, cancellationToken)
            ?? throw new InvalidOperationException("Room not found.");

        if (room.Status != RoomStatus.Active || room.RoomType is null || !room.RoomType.IsActive)
        {
            throw new InvalidOperationException("Room is not available for booking.");
        }

        if (request.GuestCount > room.RoomType.MaxGuests)
        {
            throw new InvalidOperationException("Guest count exceeds room capacity.");
        }

        var hasConflict = await HasOverlapAsync(room.Id, request.CheckInDate, request.CheckOutDate, booking.Id, cancellationToken);
        if (hasConflict)
        {
            throw new InvalidOperationException("Room already has an overlapping booking.");
        }

        booking.RoomId = room.Id;
        booking.RoomTypeId = room.RoomTypeId;
        booking.CheckInDate = request.CheckInDate;
        booking.CheckOutDate = request.CheckOutDate;
        booking.GuestCount = request.GuestCount;
        booking.SpecialRequests = request.SpecialRequests?.Trim();
        booking.NightlyRateSnapshot = room.RoomType.BaseNightlyRate;
        booking.TotalPrice = pricingService.CalculateTotal(room.RoomType, request.CheckInDate, request.CheckOutDate);
        booking.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return MapBookingDto(booking);
    }

    public async Task<bool> CancelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var booking = await dbContext.Bookings.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (booking is null)
        {
            return false;
        }

        booking.Status = BookingStatus.Cancelled;
        booking.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<bool> HasOverlapAsync(Guid roomId, DateOnly checkInDate, DateOnly checkOutDate, Guid? excludeBookingId, CancellationToken cancellationToken)
        => await dbContext.Bookings.AsNoTracking()
            .AnyAsync(booking => booking.RoomId == roomId
                                 && booking.Status != BookingStatus.Cancelled
                                 && booking.CheckInDate < checkOutDate
                                 && booking.CheckOutDate > checkInDate
                                 && booking.Id != excludeBookingId, cancellationToken);

    private static void ValidateDates(DateOnly checkInDate, DateOnly checkOutDate)
    {
        if (checkOutDate <= checkInDate)
        {
            throw new ArgumentException("Check-out date must be after check-in date.");
        }
    }

    private static BookingDto MapBookingDto(Booking booking)
        => new(
            booking.Id,
            booking.CustomerId,
            $"{booking.Customer?.FirstName} {booking.Customer?.LastName}".Trim(),
            booking.RoomId,
            booking.Room?.RoomNumber ?? string.Empty,
            booking.RoomTypeId,
            booking.Room?.RoomType?.Name ?? string.Empty,
            booking.CheckInDate,
            booking.CheckOutDate,
            booking.Status,
            booking.NightlyRateSnapshot,
            booking.TotalPrice,
            booking.SpecialRequests,
            booking.GuestCount);
}

public sealed class AnalyticsService(IHotelDbContext dbContext) : IAnalyticsService
{
    public async Task<IReadOnlyList<OccupancyPointDto>> GetOccupancyAsync(DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken = default)
    {
        if (toDate < fromDate)
        {
            throw new ArgumentException("End date must not be before start date.");
        }

        var totalRooms = await dbContext.Rooms.AsNoTracking().CountAsync(room => room.Status == RoomStatus.Active, cancellationToken);

        var bookings = await dbContext.Bookings.AsNoTracking()
            .Where(booking => booking.Status != BookingStatus.Cancelled && booking.CheckInDate <= toDate && booking.CheckOutDate >= fromDate)
            .ToListAsync(cancellationToken);

        var points = new List<OccupancyPointDto>();
        for (var day = fromDate; day <= toDate; day = day.AddDays(1))
        {
            var bookedRooms = bookings.Count(booking => booking.CheckInDate <= day && booking.CheckOutDate > day);
            var occupancy = totalRooms == 0 ? 0 : decimal.Round((decimal)bookedRooms / totalRooms * 100m, 2);
            points.Add(new OccupancyPointDto(day, bookedRooms, totalRooms, occupancy));
        }

        return points;
    }

    public async Task<IReadOnlyList<RevenuePointDto>> GetRevenueAsync(DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken = default)
    {
        if (toDate < fromDate)
        {
            throw new ArgumentException("End date must not be before start date.");
        }

        var bookings = await dbContext.Bookings.AsNoTracking()
            .Where(booking => booking.Status != BookingStatus.Cancelled && booking.CheckInDate <= toDate && booking.CheckOutDate >= fromDate)
            .ToListAsync(cancellationToken);

        return bookings
            .GroupBy(booking => new DateOnly(booking.CheckInDate.Year, booking.CheckInDate.Month, 1))
            .Select(group => new RevenuePointDto(group.Key.ToString("yyyy-MM"), group.Sum(booking => booking.TotalPrice)))
            .OrderBy(point => point.Period)
            .ToList();
    }

    public async Task<IReadOnlyList<PopularRoomTypeDto>> GetPopularRoomTypesAsync(DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken = default)
    {
        if (toDate < fromDate)
        {
            throw new ArgumentException("End date must not be before start date.");
        }

        var bookings = await dbContext.Bookings.AsNoTracking()
            .Include(booking => booking.Room)
                .ThenInclude(room => room.RoomType)
            .Where(booking => booking.Status != BookingStatus.Cancelled && booking.CheckInDate <= toDate && booking.CheckOutDate >= fromDate)
            .ToListAsync(cancellationToken);

        return bookings
            .GroupBy(booking => new { booking.RoomTypeId, RoomTypeName = booking.Room?.RoomType?.Name ?? string.Empty })
            .Select(group => new PopularRoomTypeDto(group.Key.RoomTypeId, group.Key.RoomTypeName, group.Count(), group.Sum(booking => booking.TotalPrice)))
            .OrderByDescending(point => point.BookingCount)
            .ToList();
    }
}
