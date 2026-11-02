namespace BusinessManagement.Domain.Enums;

public enum MembershipStatus { Invited, Active, Suspended, Removed }
public enum RoleName { Owner, Admin, Manager, Accountant, InventoryStaff, HRStaff, Employee }
public enum MovementType { Purchase, Sale, Adjustment, TransferIn, TransferOut, Return }
public enum PurchaseStatus { Draft, Received, Cancelled }
public enum SaleStatus { Draft, Completed, Cancelled }
public enum EmploymentStatus { Active, OnLeave, Resigned, Terminated }
public enum AttendanceStatus { Present, Absent, HalfDay, Leave, Holiday }
public enum LeaveStatus { Requested, Approved, Rejected, Cancelled }
public enum PayrollStatus { Draft, Calculated, Finalized, Cancelled }
public enum PaymentStatus { Pending, Completed, Failed, Reversed }
public enum PaymentMethod { Cash, BankTransfer, Card, Upi, Other }
public enum PayeeType { Supplier, Employee, Other }
public enum ExpenseStatus { Draft, Submitted, Approved, Rejected, Paid }
