using SchoolApi.Dtos;

namespace SchoolApi.Services;

public interface IProgrammeService
{
    Task<IReadOnlyList<ProgrammeResponseDto>> GetAllAsync(
        CancellationToken ct);

    Task<ProgrammeResponseDto?> GetByIdAsync(
        long id, CancellationToken ct);

    Task<ProgrammeResponseDto> CreateAsync(
        ProgrammeCreateDto dto, CancellationToken ct);

    Task<ProgrammeResponseDto?> UpdateAsync(
        long id, ProgrammeUpdateDto dto, CancellationToken ct);

    Task<bool> DeleteAsync(
        long id, CancellationToken ct);
}

public interface ICourseService
{
    Task<IReadOnlyList<CourseResponseDto>> GetAllAsync(
        CancellationToken ct);

    Task<CourseResponseDto?> GetByIdAsync(
        long id, CancellationToken ct);

    Task<CourseResponseDto> CreateAsync(
        CourseCreateDto dto, CancellationToken ct);

    Task<CourseResponseDto?> UpdateAsync(
        long id, CourseUpdateDto dto, CancellationToken ct);

    Task<bool> DeleteAsync(
        long id, CancellationToken ct);
}

public interface IStudentService
{
    Task<IReadOnlyList<StudentResponseDto>> GetAllAsync(
        CancellationToken ct);

    Task<StudentResponseDto?> GetByIdAsync(
        long id, CancellationToken ct);

    Task<StudentResponseDto> CreateAsync(
        StudentCreateDto dto, CancellationToken ct);

    Task<StudentResponseDto?> UpdateAsync(
        long id, StudentUpdateDto dto, CancellationToken ct);

    Task<bool> DeleteAsync(
        long id, CancellationToken ct);
}