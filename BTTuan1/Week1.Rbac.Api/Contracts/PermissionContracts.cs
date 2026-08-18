using System.ComponentModel.DataAnnotations;

namespace Week1.Rbac.Api.Contracts;

public sealed record CreatePermissionRequest(
    [property: Required, MinLength(2)] string Code,
    string Description);

public sealed record UpdatePermissionRequest(
    [property: Required, MinLength(2)] string Code,
    string Description);

public sealed record PermissionResponse(
    Guid Id,
    string Code,
    string Description);