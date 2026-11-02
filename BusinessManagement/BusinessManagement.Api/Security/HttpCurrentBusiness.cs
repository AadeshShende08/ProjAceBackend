using BusinessManagement.Application.Security;

namespace BusinessManagement.Api.Security;

public sealed class HttpCurrentBusiness : ICurrentBusiness
{
    public Guid? BusinessId { get; private set; }
    public Guid? RoleId { get; private set; }
    public bool IsResolved => BusinessId.HasValue && RoleId.HasValue;

    public void Set(Guid businessId, Guid roleId)
    {
        BusinessId = businessId;
        RoleId = roleId;
    }
}
