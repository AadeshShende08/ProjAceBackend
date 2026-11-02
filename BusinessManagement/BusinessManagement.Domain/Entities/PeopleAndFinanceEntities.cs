using BusinessManagement.Domain.Common;
using BusinessManagement.Domain.Enums;

namespace BusinessManagement.Domain.Entities;

public sealed class Department : BusinessEntity
{
    public required string Name { get; set; }
    public ICollection<Employee> Employees { get; set; } = [];
}

public sealed class Employee : BusinessEntity
{
    public Guid? DepartmentId { get; set; }
    public required string EmployeeCode { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public DateOnly JoiningDate { get; set; }
    public EmploymentStatus EmploymentStatus { get; set; } = EmploymentStatus.Active;
    public Department? Department { get; set; }
    public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = [];
    public ICollection<LeaveRequest> LeaveRequests { get; set; } = [];
    public ICollection<SalaryStructure> SalaryStructures { get; set; } = [];
    public ICollection<SalaryRecord> SalaryRecords { get; set; } = [];
}

public sealed class AttendanceRecord : BusinessEntity
{
    public Guid EmployeeId { get; set; }
    public DateOnly WorkDate { get; set; }
    public AttendanceStatus Status { get; set; }
    public DateTimeOffset? CheckInUtc { get; set; }
    public DateTimeOffset? CheckOutUtc { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public DateTimeOffset? ApprovedAtUtc { get; set; }
    public Employee Employee { get; set; } = null!;
}

public sealed class LeaveRequest : BusinessEntity
{
    public Guid EmployeeId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public required string Reason { get; set; }
    public LeaveStatus Status { get; set; } = LeaveStatus.Requested;
    public Guid? ApprovedByUserId { get; set; }
    public DateTimeOffset? ApprovedAtUtc { get; set; }
    public Employee Employee { get; set; } = null!;
}

public sealed class SalaryStructure : BusinessEntity
{
    public Guid EmployeeId { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public decimal BasicAmount { get; set; }
    public decimal AllowanceAmount { get; set; }
    public decimal DeductionAmount { get; set; }
    public Employee Employee { get; set; } = null!;
}

public sealed class PayPeriod : BusinessEntity
{
    public required string Name { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public PayrollStatus Status { get; set; } = PayrollStatus.Draft;
    public ICollection<SalaryRecord> SalaryRecords { get; set; } = [];
}

public sealed class SalaryRecord : BusinessEntity
{
    public Guid EmployeeId { get; set; }
    public Guid PayPeriodId { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal DeductionAmount { get; set; }
    public decimal NetAmount { get; set; }
    public PayrollStatus Status { get; set; } = PayrollStatus.Draft;
    public DateTimeOffset CalculatedAtUtc { get; set; }
    public DateTimeOffset? FinalizedAtUtc { get; set; }
    public Employee Employee { get; set; } = null!;
    public PayPeriod PayPeriod { get; set; } = null!;
    public ICollection<SalaryRecordItem> Items { get; set; } = [];
}

public sealed class SalaryRecordItem : EntityBase
{
    public Guid SalaryRecordId { get; set; }
    public required string Name { get; set; }
    public decimal Amount { get; set; }
    public SalaryRecord SalaryRecord { get; set; } = null!;
}

public sealed class Payment : BusinessEntity
{
    public PayeeType PayeeType { get; set; }
    public Guid? PayeeId { get; set; }
    public decimal Amount { get; set; }
    public DateOnly PaymentDate { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? ReferenceNumber { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public Guid? CreatedByUserId { get; set; }
    public ICollection<PaymentAllocation> Allocations { get; set; } = [];
}

public sealed class PaymentAllocation : EntityBase
{
    public Guid PaymentId { get; set; }
    public string? ReferenceType { get; set; }
    public Guid? ReferenceId { get; set; }
    public decimal Amount { get; set; }
    public Payment Payment { get; set; } = null!;
}

public sealed class Expense : BusinessEntity
{
    public required string Description { get; set; }
    public decimal Amount { get; set; }
    public DateOnly ExpenseDate { get; set; }
    public ExpenseStatus Status { get; set; } = ExpenseStatus.Draft;
    public Guid? CreatedByUserId { get; set; }
    public ICollection<ExpenseAttachment> Attachments { get; set; } = [];
}

public sealed class ExpenseAttachment : EntityBase
{
    public Guid ExpenseId { get; set; }
    public required string FileName { get; set; }
    public required string StorageKey { get; set; }
    public Expense Expense { get; set; } = null!;
}
