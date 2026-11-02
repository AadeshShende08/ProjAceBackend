using BusinessManagement.Application.Security;
using BusinessManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace BusinessManagement.Api.Security;

public sealed record PermissionRequirement(string PermissionName) : IAuthorizationRequirement;

public sealed class PermissionAuthorizationHandler(
    ApplicationDbContext dbContext,
    ICurrentUser currentUser,
    ICurrentBusiness currentBusiness) : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null || currentBusiness.BusinessId is null)
        {
            return;
        }

        bool allowed = await dbContext.BusinessUsers
            .AsNoTracking()
            .Where(membership => membership.UserId == currentUser.UserId.Value
                && membership.BusinessId == currentBusiness.BusinessId.Value
                && membership.RoleId == currentBusiness.RoleId)
            .Where(membership => membership.Role.Permissions.Any(mapping => mapping.Permission.Name == requirement.PermissionName))
            .AnyAsync();

        if (allowed)
        {
            context.Succeed(requirement);
        }
    }
}

public static class AuthorizationPolicyExtensions
{
    public static AuthorizationPolicyBuilder RequirePermission(
        this AuthorizationPolicyBuilder builder,
        string permissionName)
    {
        return builder.AddRequirements(new PermissionRequirement(permissionName));
    }
}
