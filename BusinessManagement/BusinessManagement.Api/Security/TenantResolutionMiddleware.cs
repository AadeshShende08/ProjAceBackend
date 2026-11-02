using BusinessManagement.Application.Security;

namespace BusinessManagement.Api.Security;

public sealed class TenantResolutionMiddleware(
    RequestDelegate next,
    ILogger<TenantResolutionMiddleware> logger)
{
    public async Task InvokeAsync(
        HttpContext context,
        ICurrentUser currentUser,
        ICurrentBusiness currentBusiness,
        IBusinessMembershipValidator membershipValidator)
    {
        if (!context.Request.Path.StartsWithSegments("/api"))
        {
            await next(context);
            return;
        }

        if (context.Request.Path.StartsWithSegments("/api/v1/auth"))
        {
            await next(context);
            return;
        }

        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        if (!Guid.TryParse(context.Request.Headers["X-Business-Id"], out Guid businessId))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new { error = "A valid X-Business-Id header is required." });
            return;
        }

        BusinessMembershipResult membership = await membershipValidator.ValidateAsync(
            currentUser.UserId.Value,
            businessId,
            context.RequestAborted);

        if (!membership.IsValid || membership.RoleId is null)
        {
            logger.LogWarning("Rejected tenant request for user {UserId} and business {BusinessId}: {Reason}", currentUser.UserId, businessId, membership.Reason);
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }

        currentBusiness.Set(businessId, membership.RoleId.Value);
        await next(context);
    }
}
