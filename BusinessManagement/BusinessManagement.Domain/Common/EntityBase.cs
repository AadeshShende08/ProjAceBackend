using BusinessManagement.Domain.Entities;

namespace BusinessManagement.Domain.Common;

public abstract class EntityBase
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}

public abstract class BusinessEntity : EntityBase
{
    public Guid BusinessId { get; set; }
    public Business? Business { get; set; }
}
