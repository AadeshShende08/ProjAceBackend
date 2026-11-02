using BusinessManagement.Domain.Entities;
using BusinessManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BusinessManagement.IntegrationTests;

public sealed class ApplicationDbContextModelTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ApplicationDbContextModelTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder => builder.UseEnvironment("Testing"));
    }

    [Fact]
    public void Model_contains_phase_two_tables_and_constraints()
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var product = context.Model.FindEntityType(typeof(Product));
        var stockBalance = context.Model.FindEntityType(typeof(StockBalance));

        Assert.Equal("products", product?.GetTableName());
        Assert.Contains(product!.GetIndexes(), index => index.IsUnique && index.Properties.Select(property => property.Name).SequenceEqual(["BusinessId", "SKU"]));
        Assert.Contains(stockBalance!.GetIndexes(), index => index.IsUnique && index.Properties.Select(property => property.Name).SequenceEqual(["BusinessId", "WarehouseId", "ProductId"]));
        Assert.Equal(18, product.FindProperty(nameof(Product.CostPrice))!.GetPrecision());
        Assert.Equal(2, product.FindProperty(nameof(Product.CostPrice))!.GetScale());
    }
}
