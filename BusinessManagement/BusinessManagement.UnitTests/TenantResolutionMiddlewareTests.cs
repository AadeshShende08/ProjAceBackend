using BusinessManagement.Api.Security;
using BusinessManagement.Application.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace BusinessManagement.UnitTests;

public sealed class TenantResolutionMiddlewareTests
{
    [Fact]
    public async Task Anonymous_api_request_returns_unauthorized()
    {
        var context = CreateContext("/api/v1/security/context");
        var currentUser = new TestCurrentUser(null, false);
        var currentBusiness = new HttpCurrentBusiness();
        var validator = new TestMembershipValidator(new(true, Guid.NewGuid(), null));
        bool nextCalled = false;
        var middleware = CreateMiddleware(() => nextCalled = true);

        await middleware.InvokeAsync(context, currentUser, currentBusiness, validator);

        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        Assert.False(nextCalled);
    }

    [Fact]
    public async Task Authenticated_api_request_without_business_header_returns_bad_request()
    {
        var context = CreateContext("/api/v1/security/context");
        var currentUser = new TestCurrentUser(Guid.NewGuid(), true);
        var currentBusiness = new HttpCurrentBusiness();
        var validator = new TestMembershipValidator(new(true, Guid.NewGuid(), null));
        bool nextCalled = false;
        var middleware = CreateMiddleware(() => nextCalled = true);

        await middleware.InvokeAsync(context, currentUser, currentBusiness, validator);

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.False(nextCalled);
        Assert.False(validator.Called);
    }

    [Fact]
    public async Task Invalid_membership_returns_forbidden_and_does_not_call_next()
    {
        var context = CreateContext("/api/v1/security/context");
        context.Request.Headers["X-Business-Id"] = Guid.NewGuid().ToString();
        var currentUser = new TestCurrentUser(Guid.NewGuid(), true);
        var currentBusiness = new HttpCurrentBusiness();
        var validator = new TestMembershipValidator(new(false, null, "Membership is inactive."));
        bool nextCalled = false;
        var middleware = CreateMiddleware(() => nextCalled = true);

        await middleware.InvokeAsync(context, currentUser, currentBusiness, validator);

        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
        Assert.True(validator.Called);
        Assert.False(nextCalled);
        Assert.False(currentBusiness.IsResolved);
    }

    [Fact]
    public async Task Active_membership_sets_context_and_calls_next()
    {
        Guid businessId = Guid.NewGuid();
        Guid roleId = Guid.NewGuid();
        var context = CreateContext("/api/v1/security/context");
        context.Request.Headers["X-Business-Id"] = businessId.ToString();
        var currentUser = new TestCurrentUser(Guid.NewGuid(), true);
        var currentBusiness = new HttpCurrentBusiness();
        var validator = new TestMembershipValidator(new(true, roleId, null));
        bool nextCalled = false;
        var middleware = CreateMiddleware(() => nextCalled = true);

        await middleware.InvokeAsync(context, currentUser, currentBusiness, validator);

        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
        Assert.True(nextCalled);
        Assert.Equal(businessId, currentBusiness.BusinessId);
        Assert.Equal(roleId, currentBusiness.RoleId);
    }

    private static DefaultHttpContext CreateContext(string path)
    {
        return new DefaultHttpContext { Request = { Path = path } };
    }

    private static TenantResolutionMiddleware CreateMiddleware(Action nextCalled)
    {
        RequestDelegate next = _ =>
        {
            nextCalled();
            return Task.CompletedTask;
        };

        return new TenantResolutionMiddleware(next, NullLogger<TenantResolutionMiddleware>.Instance);
    }

    private sealed class TestCurrentUser(Guid? userId, bool isAuthenticated) : ICurrentUser
    {
        public Guid? UserId { get; } = userId;
        public bool IsAuthenticated { get; } = isAuthenticated;
    }

    private sealed class TestMembershipValidator(BusinessMembershipResult result) : IBusinessMembershipValidator
    {
        public bool Called { get; private set; }

        public Task<BusinessMembershipResult> ValidateAsync(Guid userId, Guid businessId, CancellationToken cancellationToken)
        {
            Called = true;
            return Task.FromResult(result);
        }
    }
}
