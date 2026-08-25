using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using SchoolApi.Models;

namespace SchoolApi.Services;

public sealed class TokenService(IConfiguration config)
{
    public string Create(AppUser user, IEnumerable<string> roles)
    {
        var signingKey = config["Jwt:SigningKey"]
            ?? throw new InvalidOperationException("Missing Jwt:SigningKey.");

        var issuer = config["Jwt:Issuer"]
            ?? throw new InvalidOperationException("Missing Jwt:Issuer.");

        var audience = config["Jwt:Audience"]
            ?? throw new InvalidOperationException("Missing Jwt:Audience.");

        if (Encoding.UTF8.GetByteCount(signingKey) < 32)
            throw new InvalidOperationException("Jwt:SigningKey must be at least 32 bytes.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Email, user.Email)
        };

        if (user.StudentId is not null)
        {
            claims.Add(new Claim("student_id", user.StudentId.Value.ToString()));
        }

        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var minutes = int.TryParse(config["Jwt:AccessTokenMinutes"], out var parsed)
            ? parsed
            : 30;

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = issuer,
            Audience = audience,
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(minutes),
            SigningCredentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256)
        };

        return new JsonWebTokenHandler().CreateToken(descriptor);
    }
}
