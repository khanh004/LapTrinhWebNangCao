using System.Security.Claims;
using HotelManagement.Api.DTOs;
using HotelManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _service;

    public BookingsController(IBookingService service)
    {
        _service = service;
    }

    private Guid CurrentEmployeeId => Guid.Parse(User.FindFirstValue("employeeId")!);

    [HttpGet]
    public async Task<ActionResult<List<BookingDto>>> GetAll([FromQuery] string? status)
        => Ok(await _service.GetAllAsync(status));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BookingDto>> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("{id:guid}/logs")]
    public async Task<ActionResult<List<BookingLogDto>>> GetLogs(Guid id)
        => Ok(await _service.GetLogsAsync(id));

    [HttpPost]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<ActionResult<BookingDto>> Create([FromBody] CreateBookingDto dto)
    {
        var (booking, error) = await _service.CreateAsync(dto, CurrentEmployeeId);
        if (error is not null) return BadRequest(error);
        return CreatedAtAction(nameof(GetById), new { id = booking!.Id }, booking);
    }

    [HttpPatch("{id:guid}/confirm")]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> Confirm(Guid id)
    {
        var (success, error) = await _service.ConfirmAsync(id, CurrentEmployeeId);
        return success ? NoContent() : BadRequest(error);
    }

    [HttpPatch("{id:guid}/check-in")]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> CheckIn(Guid id)
    {
        var (success, error) = await _service.CheckInAsync(id, CurrentEmployeeId);
        return success ? NoContent() : BadRequest(error);
    }

    [HttpPatch("{id:guid}/check-out")]
[Authorize(Roles = "Admin,Receptionist")]
public async Task<IActionResult> CheckOut(Guid id, [FromBody] CheckOutRequestDto dto)
{
    var (success, error, total, description) = await _service.CheckOutAsync(id, dto, CurrentEmployeeId);
    return success ? Ok(new { totalAmount = total, invoiceDescription = description }) : BadRequest(error);
}

    [HttpPatch("{id:guid}/extend")]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> Extend(Guid id, [FromBody] ExtendBookingDto dto)
    {
        var (success, error) = await _service.ExtendAsync(id, dto, CurrentEmployeeId);
        return success ? NoContent() : BadRequest(error);
    }

    [HttpPatch("{id:guid}/cancel")]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var (success, error) = await _service.CancelAsync(id, CurrentEmployeeId);
        return success ? NoContent() : BadRequest(error);
    }
}