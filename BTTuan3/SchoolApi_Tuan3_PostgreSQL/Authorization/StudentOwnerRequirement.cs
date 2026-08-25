using Microsoft.AspNetCore.Authorization;

namespace SchoolApi.Authorization;

public sealed class StudentOwnerRequirement : IAuthorizationRequirement
{
}
