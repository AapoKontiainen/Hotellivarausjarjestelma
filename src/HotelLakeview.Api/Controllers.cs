using HotelLakeview.Application.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace HotelLakeview.Api.Controllers;

[ApiController]
[Route("api/customers")]
public sealed class CustomersController(ICustomerService customerService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CustomerDto>>> GetAll(CancellationToken cancellationToken)
        => Ok(await customerService.GetAllAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CustomerDto>> GetById(Guid id, CancellationToken cancellationToken)
        => await customerService.GetByIdAsync(id, cancellationToken) is { } customer ? Ok(customer) : NotFound();

    [HttpPost]
    public async Task<ActionResult<CustomerDto>> Create(CreateCustomerRequest request, CancellationToken cancellationToken)
    {
        var customer = await customerService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CustomerDto>> Update(Guid id, UpdateCustomerRequest request, CancellationToken cancellationToken)
        => await customerService.UpdateAsync(id, request, cancellationToken) is { } customer ? Ok(customer) : NotFound();

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return await customerService.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(exception.Message);
        }
    }
}

[ApiController]
[Route("api/room-types")]
public sealed class RoomTypesController(IRoomService roomService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RoomTypeDto>>> GetAll(CancellationToken cancellationToken)
        => Ok(await roomService.GetRoomTypesAsync(cancellationToken));

    [HttpPost]
    public async Task<ActionResult<RoomTypeDto>> Create(CreateRoomTypeRequest request, CancellationToken cancellationToken)
        => Ok(await roomService.CreateRoomTypeAsync(request, cancellationToken));

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<RoomTypeDto>> Update(Guid id, UpdateRoomTypeRequest request, CancellationToken cancellationToken)
        => await roomService.UpdateRoomTypeAsync(id, request, cancellationToken) is { } roomType ? Ok(roomType) : NotFound();

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        => await roomService.DeleteRoomTypeAsync(id, cancellationToken) ? NoContent() : NotFound();
}

[ApiController]
[Route("api/rooms")]
public sealed class RoomsController(IRoomService roomService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RoomDto>>> GetAll(CancellationToken cancellationToken)
        => Ok(await roomService.GetRoomsAsync(cancellationToken));

    [HttpPost]
    public async Task<ActionResult<RoomDto>> Create(CreateRoomRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var room = await roomService.CreateRoomAsync(request, cancellationToken);
            return Ok(room);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<RoomDto>> Update(Guid id, UpdateRoomRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return await roomService.UpdateRoomAsync(id, request, cancellationToken) is { } room ? Ok(room) : NotFound();
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        => await roomService.DeleteRoomAsync(id, cancellationToken) ? NoContent() : NotFound();
}

[ApiController]
[Route("api/bookings")]
public sealed class BookingsController(IBookingService bookingService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BookingDto>>> GetAll(CancellationToken cancellationToken)
        => Ok(await bookingService.GetAllAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BookingDto>> GetById(Guid id, CancellationToken cancellationToken)
        => await bookingService.GetByIdAsync(id, cancellationToken) is { } booking ? Ok(booking) : NotFound();

    [HttpPost]
    public async Task<ActionResult<BookingDto>> Create(CreateBookingRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var booking = await bookingService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = booking.Id }, booking);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(exception.Message);
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<BookingDto>> Update(Guid id, UpdateBookingRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return await bookingService.UpdateAsync(id, request, cancellationToken) is { } booking ? Ok(booking) : NotFound();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(exception.Message);
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
        => await bookingService.CancelAsync(id, cancellationToken) ? NoContent() : NotFound();
}

[ApiController]
[Route("api/availability")]
public sealed class AvailabilityController(IBookingService bookingService) : ControllerBase
{
    [HttpPost("search")]
    public async Task<ActionResult<IReadOnlyList<AvailabilityResultDto>>> Search(AvailabilitySearchRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await bookingService.SearchAvailabilityAsync(request, cancellationToken));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
    }
}

[ApiController]
[Route("api/analytics")]
public sealed class AnalyticsController(IAnalyticsService analyticsService) : ControllerBase
{
    [HttpGet("occupancy")]
    public async Task<ActionResult<IReadOnlyList<OccupancyPointDto>>> Occupancy([FromQuery] DateOnly fromDate, [FromQuery] DateOnly toDate, CancellationToken cancellationToken)
        => Ok(await analyticsService.GetOccupancyAsync(fromDate, toDate, cancellationToken));

    [HttpGet("revenue")]
    public async Task<ActionResult<IReadOnlyList<RevenuePointDto>>> Revenue([FromQuery] DateOnly fromDate, [FromQuery] DateOnly toDate, CancellationToken cancellationToken)
        => Ok(await analyticsService.GetRevenueAsync(fromDate, toDate, cancellationToken));

    [HttpGet("popular-room-types")]
    public async Task<ActionResult<IReadOnlyList<PopularRoomTypeDto>>> PopularRoomTypes([FromQuery] DateOnly fromDate, [FromQuery] DateOnly toDate, CancellationToken cancellationToken)
        => Ok(await analyticsService.GetPopularRoomTypesAsync(fromDate, toDate, cancellationToken));
}
