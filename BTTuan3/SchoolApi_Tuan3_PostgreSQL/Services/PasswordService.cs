using Microsoft.AspNetCore.Identity;
using SchoolApi.Models;

namespace SchoolApi.Services;

public interface IPasswordService
{
    string Hash(AppUser user, string password);
    bool Verify(AppUser user, string password);
}

public sealed class PasswordService : IPasswordService
{
    private readonly PasswordHasher<AppUser> hasher = new();

    public string Hash(AppUser user, string password)
        => hasher.HashPassword(user, password);

    public bool Verify(AppUser user, string password)
        => hasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            password) != PasswordVerificationResult.Failed;
}
