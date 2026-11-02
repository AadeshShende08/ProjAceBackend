using System.Text.Json;
using System.Diagnostics;
using BusinessManagement.Application.Security;
using BusinessManagement.Domain.Entities;
using BusinessManagement.Infrastructure.Persistence;

namespace BusinessManagement.Infrastructure.Security;

public sealed class EfAuditLogger(
    ApplicationDbContext dbContext,
    ICurrentUser currentUser,
    ICurrentBusiness currentBusiness) : IAuditLogger
{
    public async Task RecordAsync(
        string action,
        string entityType,
        Guid? entityId,
        IReadOnlyDictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        Guid businessId = currentBusiness.BusinessId
            ?? throw new InvalidOperationException("An active business is required to record an audit event.");

        var auditLog = new AuditLog
        {
            BusinessId = businessId,
            ActorUserId = currentUser.UserId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            OccurredAtUtc = DateTimeOffset.UtcNow,
            CorrelationId = Activity.Current?.Id,
            MetadataJson = metadata is null ? null : JsonSerializer.Serialize(metadata)
        };

        dbContext.AuditLogs.Add(auditLog);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
