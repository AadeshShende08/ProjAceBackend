using BusinessManagement.Domain.Common;
using BusinessManagement.Domain.Enums;

namespace BusinessManagement.Domain.Entities;

public sealed class Business : EntityBase
{
    public required string Name { get; set; }
    public string? BusinessType { get; set; }
    public required string CurrencyCode { get; set; }
    public required string TimeZone { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<BusinessUser> Members { get; set; } = [];
    public ICollection<AuditLog> AuditLogs { get; set; } = [];
}

public sealed class UserAccount : EntityBase
{
    public required string Email { get; set; }
    public required string NormalizedEmail { get; set; }
    public required string DisplayName { get; set; }
    public string? AuthenticationProviderId { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<BusinessUser> Memberships { get; set; } = [];
}

public sealed class BusinessUser : EntityBase
{
    public Guid BusinessId { get; set; }
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public MembershipStatus Status { get; set; } = MembershipStatus.Invited;
    public DateTimeOffset JoinedAtUtc { get; set; }
    public Business Business { get; set; } = null!;
    public UserAccount User { get; set; } = null!;
    public Role Role { get; set; } = null!;
}

public sealed class Role : EntityBase
{
    public required RoleName Name { get; set; }
    public ICollection<BusinessUser> Members { get; set; } = [];
    public ICollection<RolePermission> Permissions { get; set; } = [];
}

public sealed class Permission : EntityBase
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public ICollection<RolePermission> Roles { get; set; } = [];
}

public sealed class RolePermission : EntityBase
{
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }
    public Role Role { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
}

public sealed class AuditLog : BusinessEntity
{
    public Guid? ActorUserId { get; set; }
    public required string Action { get; set; }
    public required string EntityType { get; set; }
    public Guid? EntityId { get; set; }
    public DateTimeOffset OccurredAtUtc { get; set; }
    public string? CorrelationId { get; set; }
    public string? MetadataJson { get; set; }
}
