namespace HotelLakeview.Domain.Common
{
    public abstract class AuditableEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}

namespace HotelLakeview.Domain.Enums
{
    public enum BookingStatus
    {
        Pending = 0,
        Confirmed = 1,
        Cancelled = 2,
        Completed = 3
    }

    public enum RoomStatus
    {
        Active = 0,
        Inactive = 1
    }
}

namespace HotelLakeview.Domain.Entities
{
    using HotelLakeview.Domain.Common;
    using HotelLakeview.Domain.Enums;

    public sealed class Customer : AuditableEntity
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }

    public sealed class RoomType : AuditableEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal BaseNightlyRate { get; set; }
        public int MaxGuests { get; set; }
        public bool IsActive { get; set; } = true;
        public ICollection<Room> Rooms { get; set; } = new List<Room>();
    }

    public sealed class Room : AuditableEntity
    {
        public string RoomNumber { get; set; } = string.Empty;
        public Guid RoomTypeId { get; set; }
        public RoomType? RoomType { get; set; }
        public RoomStatus Status { get; set; } = RoomStatus.Active;
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<RoomImage> Images { get; set; } = new List<RoomImage>();
    }

    public sealed class Booking : AuditableEntity
    {
        public Guid CustomerId { get; set; }
        public Customer? Customer { get; set; }
        public Guid RoomId { get; set; }
        public Room? Room { get; set; }
        public Guid RoomTypeId { get; set; }
        public DateOnly CheckInDate { get; set; }
        public DateOnly CheckOutDate { get; set; }
        public BookingStatus Status { get; set; } = BookingStatus.Confirmed;
        public decimal NightlyRateSnapshot { get; set; }
        public decimal TotalPrice { get; set; }
        public string? SpecialRequests { get; set; }
        public int GuestCount { get; set; }
    }

    public sealed class RoomImage : AuditableEntity
    {
        public Guid RoomId { get; set; }
        public Room? Room { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public string StoragePath { get; set; } = string.Empty;
        public int SortOrder { get; set; }
    }
}
