using HotelManagement.Api.DTOs;
using HotelManagement.Api.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace HotelManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoomsController : ControllerBase
{
    private readonly IRoomService _service;

    public RoomsController(IRoomService service)
    {
        _service = service;
    }

    // GET: api/rooms?status=AVAILABLE&search=101
    [HttpGet]
    public async Task<ActionResult<List<RoomDto>>> GetAll([FromQuery] string? status, [FromQuery] string? search)
    {
        return Ok(await _service.GetAllAsync(status, search));
    }

    // GET: api/rooms/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RoomDto>> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    // POST: api/rooms
    [HttpPost]
    public async Task<ActionResult<RoomDto>> Create([FromBody] CreateRoomDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // PUT: api/rooms/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRoomDto dto)
    {
        var success = await _service.UpdateAsync(id, dto);
        return success ? NoContent() : NotFound();
    }

    // PATCH: api/rooms/{id}/status
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateRoomStatusDto dto)
    {
        var success = await _service.UpdateStatusAsync(id, dto);
        return success ? NoContent() : BadRequest("ID không tồn tại hoặc trạng thái không hợp lệ.");
    }

    // DELETE: api/rooms/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _service.DeleteAsync(id);
        return success ? NoContent() : NotFound();
    }
    
    [HttpPatch("{id:guid}/confirm-cleaning")]
    [Authorize(Roles = "Admin,Housekeeping")]
    public async Task<IActionResult> ConfirmCleaning(Guid id)
    {
        var employeeId = Guid.Parse(User.FindFirstValue("employeeId")!);
        var (success, error) = await _service.ConfirmCleaningAsync(id, employeeId);
        return success ? NoContent() : BadRequest(error);
    }
    [HttpGet("{id:guid}/check-availability")]
    public async Task<ActionResult<RoomAvailabilityDto>> CheckAvailability(
        Guid id, [FromQuery] DateOnly checkIn, [FromQuery] DateOnly checkOut)
    {
        var result = await _service.CheckAvailabilityAsync(id, checkIn, checkOut);
        return result is null ? NotFound() : Ok(result);
    }
}