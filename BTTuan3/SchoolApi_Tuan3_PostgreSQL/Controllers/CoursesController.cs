using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolApi.Dtos;
using SchoolApi.Services;

namespace SchoolApi.Controllers;

[ApiController]
[Route("api/courses")]
[Authorize]
public class CoursesController(ICourseService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CourseResponseDto>>> GetAll(
        CancellationToken ct)
        => Ok(await service.GetAllAsync(ct));

    [HttpGet("{id:long}")]
    public async Task<ActionResult<CourseResponseDto>> GetById(
        long id,
        CancellationToken ct)
    {
        var item = await service.GetByIdAsync(id, ct);

        return item is null
            ? NotFound()
            : Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<CourseResponseDto>> Create(
        CourseCreateDto dto,
        CancellationToken ct)
    {
        var item = await service.CreateAsync(dto, ct);

        return CreatedAtAction(
            nameof(GetById),
            new { id = item.CourseId },
            item);
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<CourseResponseDto>> Update(
        long id,
        CourseUpdateDto dto,
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