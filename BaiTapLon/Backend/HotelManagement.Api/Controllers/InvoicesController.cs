using HotelManagement.Api.DTOs;
using HotelManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceService _service;

    public InvoicesController(IInvoiceService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<InvoiceDto>>> GetAll([FromQuery] string? paymentStatus)
        => Ok(await _service.GetAllAsync(paymentStatus));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<InvoiceDto>> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPatch("{id:guid}/mark-paid")]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> MarkPaid(Guid id)
    {
        var success = await _service.MarkAsPaidAsync(id);
        return success ? NoContent() : NotFound();
    }
}