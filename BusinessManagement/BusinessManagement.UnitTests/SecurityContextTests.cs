using System.Security.Claims;
using BusinessManagement.Api.Security;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace BusinessManagement.UnitTests;

public sealed class SecurityContextTests
{
    [Fact]
    public void HttpCurrentUser_reads_the_guid_subject_claim()
    {
        Guid userId = Guid.NewGuid();
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim("sub", userId.ToString())],
                authenticationType: "Bearer"))
        };
        var accessor = new HttpContextAccessor { HttpContext = context };
        var currentUser = new HttpCurrentUser(accessor);

        Assert.True(currentUser.IsAuthenticated);
        Assert.Equal(userId, currentUser.UserId);
    }

    [Fact]
    public void HttpCurrentUser_returns_no_user_for_anonymous_context()
    {
        var accessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
        var currentUser = new HttpCurrentUser(accessor);

        Assert.False(currentUser.IsAuthenticated);
        Assert.Null(currentUser.UserId);
    }

    [Fact]
    public void HttpCurrentBusiness_is_unresolved_until_membership_is_set()
    {
        var currentBusiness = new HttpCurrentBusiness();
        Guid businessId = Guid.NewGuid();
        Guid roleId = Guid.NewGuid();

        Assert.False(currentBusiness.IsResolved);

        currentBusiness.Set(businessId, roleId);

        Assert.True(currentBusiness.IsResolved);
        Assert.Equal(businessId, currentBusiness.BusinessId);
        Assert.Equal(roleId, currentBusiness.RoleId);
    }
}
