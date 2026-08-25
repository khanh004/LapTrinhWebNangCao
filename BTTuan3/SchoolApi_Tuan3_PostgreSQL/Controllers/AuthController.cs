using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using SchoolApi.Data;
using SchoolApi.Services;

namespace SchoolApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    AppDbContext db,
    IPasswordService passwords,
    TokenService tokens) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login(
        LoginRequest request,
        CancellationToken ct)
    {
        var user = await db.AppUsers
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .SingleOrDefaultAsync(
                u => u.Email == request.Email && u.IsActive,
                ct);

        if (user is null || !passwords.Verify(user, request.Password))
        {
            return Unauthorized(new ProblemDetails
            {
                Title = "Email hoac mat khau khong dung",
                Status = StatusCodes.Status401Unauthorized
            });
        }

        var roles = user.UserRoles
            .Select(ur => ur.Role.RoleName)
            .ToList();

        return Ok(new LoginResponse(
            tokens.Create(user, roles),
            roles));
    }
}

public sealed record LoginRequest(
    [property: Required, EmailAddress] string Email,
    [property: Required, MinLength(8)] string Password);

public sealed record LoginResponse(
    string AccessToken,
    IEnumerable<string> Roles);
