using Microsoft.AspNetCore.Mvc;
using SchoolApi.Dtos;
using SchoolApi.Services;

namespace SchoolApi.Controllers;

[ApiController]
[Route("api/students")]
public class StudentsController(IStudentService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<StudentResponseDto>>> GetAll(
        CancellationToken ct)
        => Ok(await service.GetAllAsync(ct));

    [HttpGet("{id:long}")]
    public async Task<ActionResult<StudentResponseDto>> GetById(
        long id,
        CancellationToken ct)
    {
        var item = await service.GetByIdAsync(id, ct);

        return item is null
            ? NotFound()
            : Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<StudentResponseDto>> Create(
        StudentCreateDto dto,
        CancellationToken ct)
    {
        var item = await service.CreateAsync(dto, ct);

        return CreatedAtAction(
            nameof(GetById),
            new { id = item.StudentId },
            item);
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<StudentResponseDto>> Update(
        long id,
        StudentUpdateDto dto,
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