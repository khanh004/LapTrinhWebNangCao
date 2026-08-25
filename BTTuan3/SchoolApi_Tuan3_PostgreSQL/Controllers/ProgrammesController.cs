using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolApi.Dtos;
using SchoolApi.Services;

namespace SchoolApi.Controllers;

[ApiController]
[Route("api/programmes")]
[Authorize]
public class ProgrammesController(IProgrammeService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProgrammeResponseDto>>> GetAll(
        CancellationToken ct)
        => Ok(await service.GetAllAsync(ct));

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ProgrammeResponseDto>> GetById(
        long id,
        CancellationToken ct)
    {
        var item = await service.GetByIdAsync(id, ct);

        return item is null
            ? NotFound()
            : Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<ProgrammeResponseDto>> Create(
        ProgrammeCreateDto dto,
        CancellationToken ct)
    {
        var item = await service.CreateAsync(dto, ct);

        return CreatedAtAction(
            nameof(GetById),
            new { id = item.ProgrammeId },
            item);
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<ProgrammeResponseDto>> Update(
        long id,
        ProgrammeUpdateDto dto,
        CancellationToken ct)
    {
        var item = await service.UpdateAsync(id, dto, ct);

        return item is null
            ? NotFound()
            : Ok(item);
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(
        long id,
        CancellationToken ct)
        => await service.DeleteAsync(id, ct)
            ? NoContent()
            : NotFound();
}