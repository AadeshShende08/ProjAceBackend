using BusinessManagement.Application.Security;
using BusinessManagement.Domain.Entities;
using Xunit;

namespace BusinessManagement.UnitTests;

public sealed class TenantQueryExtensionsTests
{
    [Fact]
    public void ForCurrentBusiness_filters_to_the_active_business()
    {
        Guid activeBusinessId = Guid.NewGuid();
        var currentBusiness = new TestCurrentBusiness(activeBusinessId);
        var products = new[]
        {
            new Product { BusinessId = activeBusinessId, SKU = "A", Name = "A", Unit = "each" },
            new Product { BusinessId = Guid.NewGuid(), SKU = "B", Name = "B", Unit = "each" }
        }.AsQueryable();

        Product[] result = products.ForCurrentBusiness(currentBusiness).ToArray();

        Assert.Single(result);
        Assert.Equal(activeBusinessId, result[0].BusinessId);
    }

    [Fact]
    public void EnsureOwnedByCurrentBusiness_rejects_foreign_entities()
    {
        var currentBusiness = new TestCurrentBusiness(Guid.NewGuid());
        var product = new Product { BusinessId = Guid.NewGuid(), SKU = "A", Name = "A", Unit = "each" };

        Assert.Throws<UnauthorizedAccessException>(() => product.EnsureOwnedByCurrentBusiness(currentBusiness));
    }

    private sealed class TestCurrentBusiness(Guid businessId) : ICurrentBusiness
    {
        public Guid? BusinessId { get; private set; } = businessId;
        public Guid? RoleId { get; private set; }
        public bool IsResolved => BusinessId.HasValue && RoleId.HasValue;

        public void Set(Guid resolvedBusinessId, Guid roleId)
        {
            BusinessId = resolvedBusinessId;
            RoleId = roleId;
        }
    }
}
