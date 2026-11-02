using BusinessManagement.Domain.Common;

namespace BusinessManagement.Application.Security;

public static class TenantQueryExtensions
{
    public static IQueryable<TEntity> ForCurrentBusiness<TEntity>(
        this IQueryable<TEntity> query,
        ICurrentBusiness currentBusiness)
        where TEntity : BusinessEntity
    {
        Guid businessId = currentBusiness.BusinessId
            ?? throw new InvalidOperationException("An active business is required for tenant-owned data access.");

        return query.Where(entity => entity.BusinessId == businessId);
    }

    public static void EnsureOwnedByCurrentBusiness<TEntity>(
        this TEntity entity,
        ICurrentBusiness currentBusiness)
        where TEntity : BusinessEntity
    {
        Guid businessId = currentBusiness.BusinessId
            ?? throw new InvalidOperationException("An active business is required for tenant-owned data access.");

        if (entity.BusinessId != businessId)
        {
            throw new UnauthorizedAccessException("The resource does not belong to the active business.");
        }
    }
}
