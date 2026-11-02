using BusinessManagement.Domain.Common;
using BusinessManagement.Domain.Enums;

namespace BusinessManagement.Domain.Entities;

public sealed class Category : BusinessEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public ICollection<Product> Products { get; set; } = [];
}

public sealed class Product : BusinessEntity
{
    public Guid? CategoryId { get; set; }
    public required string SKU { get; set; }
    public required string Name { get; set; }
    public required string Unit { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public decimal ReorderLevel { get; set; }
    public bool IsActive { get; set; } = true;
    public Category? Category { get; set; }
    public ICollection<StockBalance> StockBalances { get; set; } = [];
    public ICollection<StockMovement> StockMovements { get; set; } = [];
}

public sealed class Warehouse : BusinessEntity
{
    public required string Name { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<StockBalance> StockBalances { get; set; } = [];
    public ICollection<StockMovement> StockMovements { get; set; } = [];
}

public sealed class StockBalance : BusinessEntity
{
    public Guid WarehouseId { get; set; }
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }
    public Warehouse Warehouse { get; set; } = null!;
    public Product Product { get; set; } = null!;
}

public sealed class StockMovement : BusinessEntity
{
    public Guid WarehouseId { get; set; }
    public Guid ProductId { get; set; }
    public MovementType MovementType { get; set; }
    public decimal QuantityDelta { get; set; }
    public decimal? UnitCost { get; set; }
    public string? ReferenceType { get; set; }
    public Guid? ReferenceId { get; set; }
    public string? Reason { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public DateTimeOffset OccurredAtUtc { get; set; }
    public Warehouse Warehouse { get; set; } = null!;
    public Product Product { get; set; } = null!;
}

public sealed class Supplier : BusinessEntity
{
    public required string Name { get; set; }
    public string? ContactName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public ICollection<Purchase> Purchases { get; set; } = [];
}

public sealed class Purchase : BusinessEntity
{
    public Guid SupplierId { get; set; }
    public required string PurchaseNumber { get; set; }
    public PurchaseStatus Status { get; set; } = PurchaseStatus.Draft;
    public DateOnly PurchaseDate { get; set; }
    public decimal TotalAmount { get; set; }
    public Supplier Supplier { get; set; } = null!;
    public ICollection<PurchaseItem> Items { get; set; } = [];
}

public sealed class PurchaseItem : EntityBase
{
    public Guid PurchaseId { get; set; }
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public Purchase Purchase { get; set; } = null!;
    public Product Product { get; set; } = null!;
}

public sealed class Customer : BusinessEntity
{
    public required string Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public ICollection<Sale> Sales { get; set; } = [];
}

public sealed class Sale : BusinessEntity
{
    public Guid? CustomerId { get; set; }
    public required string SaleNumber { get; set; }
    public SaleStatus Status { get; set; } = SaleStatus.Draft;
    public DateOnly SaleDate { get; set; }
    public decimal TotalAmount { get; set; }
    public Customer? Customer { get; set; }
    public ICollection<SaleItem> Items { get; set; } = [];
}

public sealed class SaleItem : EntityBase
{
    public Guid SaleId { get; set; }
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public Sale Sale { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
