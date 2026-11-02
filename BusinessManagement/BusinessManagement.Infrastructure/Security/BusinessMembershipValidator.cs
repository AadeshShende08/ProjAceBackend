using BusinessManagement.Application.Security;
using BusinessManagement.Domain.Enums;
using BusinessManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BusinessManagement.Infrastructure.Security;

public sealed class BusinessMembershipValidator(ApplicationDbContext dbContext) : IBusinessMembershipValidator
{
    public async Task<BusinessMembershipResult> ValidateAsync(
        Guid userId,
        Guid businessId,
        CancellationToken cancellationToken)
    {
        var membership = await dbContext.BusinessUsers
            .AsNoTracking()
            .Where(item => item.UserId == userId && item.BusinessId == businessId)
            .Select(item => new { item.Status, item.RoleId, BusinessActive = item.Business.IsActive, UserActive = item.User.IsActive })
            .SingleOrDefaultAsync(cancellationToken);

        if (membership is null)
        {
            return new(false, null, "Membership was not found.");
        }

        if (membership.Status != MembershipStatus.Active)
        {
            return new(false, null, "Membership is not active.");
        }

        if (!membership.BusinessActive || !membership.UserActive)
        {
            return new(false, null, "The user or business is inactive.");
        }

        return new(true, membership.RoleId, null);
    }
}
