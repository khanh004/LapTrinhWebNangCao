using Microsoft.AspNetCore.Authorization;

namespace SchoolApi.Authorization;

public sealed class StudentOwnerHandler
    : AuthorizationHandler<StudentOwnerRequirement, long>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        StudentOwnerRequirement requirement,
        long studentId)
    {
        if (context.User.IsInRole("Staff") || context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var studentIdClaim = context.User.FindFirst("student_id")?.Value;

        if (long.TryParse(studentIdClaim, out var ownedStudentId)
            && ownedStudentId == studentId)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
