using FluentValidation;
using HotelLakeview.Application.Abstractions;

namespace HotelLakeview.Application.Validation;

public sealed class CreateCustomerRequestValidator : AbstractValidator<CreateCustomerRequest>
{
    public CreateCustomerRequestValidator()
    {
        RuleFor(request => request.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(request => request.LastName).NotEmpty().MaximumLength(100);
        RuleFor(request => request.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(request => request.Phone).NotEmpty().MaximumLength(50);
        RuleFor(request => request.Notes).MaximumLength(1000).When(request => request.Notes is not null);
    }
}

public sealed class CreateRoomTypeRequestValidator : AbstractValidator<CreateRoomTypeRequest>
{
    public CreateRoomTypeRequestValidator()
    {
        RuleFor(request => request.Name).NotEmpty().MaximumLength(100);
        RuleFor(request => request.BaseNightlyRate).GreaterThan(0);
        RuleFor(request => request.MaxGuests).InclusiveBetween(1, 10);
    }
}

public sealed class CreateRoomRequestValidator : AbstractValidator<CreateRoomRequest>
{
    public CreateRoomRequestValidator()
    {
        RuleFor(request => request.RoomNumber).NotEmpty().MaximumLength(20);
        RuleFor(request => request.RoomTypeId).NotEmpty();
    }
}

public sealed class CreateBookingRequestValidator : AbstractValidator<CreateBookingRequest>
{
    public CreateBookingRequestValidator()
    {
        RuleFor(request => request.CustomerId).NotEmpty();
        RuleFor(request => request.RoomId).NotEmpty();
        RuleFor(request => request.CheckInDate).NotEmpty();
        RuleFor(request => request.CheckOutDate).NotEmpty();
        RuleFor(request => request).Must(request => request.CheckOutDate > request.CheckInDate).WithMessage("Check-out date must be after check-in date.");
        RuleFor(request => request.GuestCount).InclusiveBetween(1, 10);
        RuleFor(request => request.SpecialRequests).MaximumLength(1000).When(request => request.SpecialRequests is not null);
    }
}

public sealed class UpdateBookingRequestValidator : AbstractValidator<UpdateBookingRequest>
{
    public UpdateBookingRequestValidator()
    {
        RuleFor(request => request.RoomId).NotEmpty();
        RuleFor(request => request.CheckInDate).NotEmpty();
        RuleFor(request => request.CheckOutDate).NotEmpty();
        RuleFor(request => request).Must(request => request.CheckOutDate > request.CheckInDate).WithMessage("Check-out date must be after check-in date.");
        RuleFor(request => request.GuestCount).InclusiveBetween(1, 10);
        RuleFor(request => request.SpecialRequests).MaximumLength(1000).When(request => request.SpecialRequests is not null);
    }
}
