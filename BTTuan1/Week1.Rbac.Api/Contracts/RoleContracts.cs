using System.ComponentModel.DataAnnotations;

namespace Week1.Rbac.Api.Contracts;

public sealed record CreateRoleRequest(
    [property: Required, MinLength(2)] string Name,
    string Description);

public sealed record UpdateRoleRequest(
    [property: Required, MinLength(2)] string Name,
    string Description);

public sealed record RoleResponse(
    Guid Id,
    string Name,
    string Description,
    IReadOnlyCollection<string> Permissions);