namespace BusinessManagement.Application.Security;

public interface ICurrentUser
{
    Guid? UserId { get; }
    bool IsAuthenticated { get; }
}

public interface ICurrentBusiness
{
    Guid? BusinessId { get; }
    Guid? RoleId { get; }
    bool IsResolved { get; }
    void Set(Guid businessId, Guid roleId);
}

public interface IBusinessMembershipValidator
{
    Task<BusinessMembershipResult> ValidateAsync(Guid userId, Guid businessId, CancellationToken cancellationToken);
}

public sealed record BusinessMembershipResult(bool IsValid, Guid? RoleId, string? Reason);

public interface IAuditLogger
{
    Task RecordAsync(string action, string entityType, Guid? entityId, IReadOnlyDictionary<string, string>? metadata = null, CancellationToken cancellationToken = default);
}
