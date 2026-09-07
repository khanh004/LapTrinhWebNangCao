using System.Security.Claims;
using HotelManagement.Api.DTOs;
using HotelManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Receptionist")]
public class WaitlistController : ControllerBase
{
    private readonly IRoomWaitlistService _service;

    public WaitlistController(IRoomWaitlistService service)
    {
        _service = service;
    }

    private Guid CurrentEmployeeId => Guid.Parse(User.FindFirstValue("employeeId")!);

    [HttpGet("room/{roomId:guid}")]
    public async Task<ActionResult<List<WaitlistDto>>> GetByRoom(Guid roomId)
        => Ok(await _service.GetByRoomAsync(roomId));

    [HttpPost]
    public async Task<ActionResult<WaitlistDto>> Join([FromBody] JoinWaitlistDto dto)
    {
        var (entry, error) = await _service.JoinAsync(dto);
        return error is not null ? BadRequest(error) : Ok(entry);
    }

    [HttpPost("{id:guid}/convert")]
    public async Task<ActionResult<BookingDto>> Convert(Guid id)
    {
        var (booking, error) = await _service.ConvertToBookingAsync(id, CurrentEmployeeId);
        return error is not null ? BadRequest(error) : Ok(booking);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var success = await _service.CancelAsync(id);
        return success ? NoContent() : NotFound();
    }

    [HttpGet("notified")]
public async Task<ActionResult<List<WaitlistDto>>> GetAllNotified()
    => Ok(await _service.GetAllNotifiedAsync());
}