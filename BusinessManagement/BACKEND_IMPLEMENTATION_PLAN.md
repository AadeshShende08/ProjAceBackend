# Business Management SaaS — Backend Implementation Specification

**Project:** Business Management SaaS
**Document:** Backend Architecture, Implementation Tasks, Database Design and Testing Requirements
**Backend:** ASP.NET Core Web API
**Language:** C#
**Database:** Supabase PostgreSQL
**ORM:** Entity Framework Core
**Frontend:** Existing React + TypeScript + Vite application
**Target customers:** Small businesses, retailers, wholesalers, service businesses and small-scale manufacturing industries
**Expected users:** 51–200 users per business
**Initial hosting strategy:** Free-tier-first, with a production-ready upgrade path

---

# 1. Instructions for GitHub Copilot

Act as a senior .NET architect and backend engineer responsible for implementing a secure, maintainable, production-oriented multi-tenant SaaS application.

Before writing code:

1. Inspect the existing repository, project structure, installed .NET SDK, target framework, existing frontend, configuration files and current Git changes.
2. Preserve the existing React frontend and its configuration. Do not overwrite, recreate or unnecessarily restructure it.
3. Identify existing backend code and reuse working components where appropriate.
4. Create an implementation checklist and execute it in logical phases.
5. Implement real functionality, not placeholder endpoints, fake database operations or hardcoded production data.
6. Build and test after every meaningful phase.
7. Fix compilation errors and failing tests before proceeding.
8. Never silently delete existing code, configuration, migrations or user-created files.
9. Never commit passwords, access tokens, connection strings containing credentials, or other secrets.
10. Do not claim that a task or test passed unless it was actually executed successfully.

Use the following as the authoritative functional and technical specification.

When a requirement is ambiguous, choose a secure, maintainable default, document the decision and continue where safe. Do not introduce unnecessary dependencies or microservices.

# 2. Product Requirements

Build the backend for a multi-tenant business management platform that allows businesses to manage:

- Business profiles and settings.
- Users, memberships, roles and permissions.
- Products, categories and warehouses.
- Inventory balances and stock movements.
- Suppliers, purchases and purchase receipts.
- Customers and sales.
- Employees and departments.
- Attendance, leave requests and approvals.
- Salary structures and payroll periods.
- Salary calculations and salary records.
- Supplier payments, employee payouts and expenses.
- Dashboard summaries, reports and exports.
- Audit logs and operational history.

The initial MVP must support all these core modules, but implementation should proceed in dependency order.

Advanced accounting, GST filing, bank transfers, biometric attendance, manufacturing resource planning, subscriptions and payment gateway integrations are outside the initial scope unless explicitly requested later.

The backend must expose a versioned REST API consumed by the existing React application.

# 3. Technology and Architecture

Use the following stack:

- ASP.NET Core Web API with the installed, supported .NET LTS version.
- C# with nullable reference types enabled.
- Entity Framework Core.
- Npgsql PostgreSQL provider compatible with the chosen EF Core version.
- Supabase-hosted PostgreSQL for development and initial deployment.
- ASP.NET Core authentication and authorization.
- OpenAPI documentation.
- Built-in dependency injection and configuration.
- Structured logging.
- xUnit for automated tests.
- A compatible PostgreSQL test environment for integration tests.

Choose compatible package versions. Do not mix incompatible EF Core, .NET and Npgsql major versions.

## 3.1 Architecture

Use a modular monolith. Do not create microservices.

Recommended structure:

```text
backend/
├── BusinessManager.sln
├── src/
│   ├── BusinessManager.API/
│   │   ├── Controllers/
│   │   ├── Middleware/
│   │   ├── Authorization/
│   │   ├── Contracts/
│   │   ├── Extensions/
│   │   ├── Program.cs
│   │   └── appsettings.json
│   │
│   ├── BusinessManager.Application/
│   │   ├── Common/
│   │   ├── Businesses/
│   │   ├── Inventory/
│   │   ├── Purchasing/
│   │   ├── Sales/
│   │   ├── Employees/
│   │   ├── Attendance/
│   │   ├── Payroll/
│   │   ├── Payments/
│   │   └── Reports/
│   │
│   ├── BusinessManager.Domain/
│   │   ├── Common/
│   │   ├── Entities/
│   │   ├── Enums/
│   │   ├── ValueObjects/
│   │   └── Exceptions/
│   │
│   └── BusinessManager.Infrastructure/
│       ├── Persistence/
│       │   ├── ApplicationDbContext.cs
│       │   ├── Configurations/
│       │   ├── Migrations/
│       │   └── Seed/
│       ├── Identity/
│       ├── Services/
│       └── Repositories/
│
└── tests/
    ├── BusinessManager.UnitTests/
    ├── BusinessManager.IntegrationTests/
    └── BusinessManager.TenantSecurityTests/
```

Adapt this layout if a suitable solution already exists. Avoid unnecessary projects or abstractions.

## 3.2 Dependency rules

- Domain must not depend on API, Application or Infrastructure.
- Application contains use cases, validation and interfaces.
- Infrastructure implements persistence and external-service interfaces.
- API contains HTTP concerns, authentication configuration and dependency registration.
- Controllers should remain thin.
- Business logic must not be embedded directly in controllers.
- Do not expose EF Core entities directly as API responses.
- Use request and response DTOs.

Use repository abstractions only where they provide meaningful business value. Do not create repetitive generic repositories solely to wrap every `DbSet`.

# 4. Configuration and Supabase PostgreSQL

The application must connect to the existing Supabase PostgreSQL instance through a backend-only connection string.

Use the Npgsql EF Core provider.

Required configuration:

```text
ConnectionStrings:DefaultConnection
```

Load secrets from environment variables or .NET User Secrets during development.

The environment variable convention should be:

```text
ConnectionStrings__DefaultConnection
```

Requirements:

- Never place real credentials in source control.
- Commit only a safe `.env.example` or configuration example containing placeholders.
- Never expose the database password, service-role key or privileged database credentials to React.
- Require encrypted database connections.
- Prefer the Supabase session pooler when the deployment environment requires IPv4 connectivity.
- Use the exact hostname, username and port supplied by Supabase.
- Do not assume the pooler username or port without reading the supplied connection details.
- Use separate databases or isolated projects for development, testing and production whenever possible.
- Use a restricted database role for runtime access where the hosting and database configuration support it.
- Do not use a privileged database account unnecessarily for the running application.
- Configure connection pooling and timeouts appropriately.
- Do not expose database ports publicly.

Create health endpoints that report application health without disclosing connection strings, SQL errors or infrastructure secrets.

## 4.1 Database migrations

- Generate migrations with EF Core.
- Commit migration files to source control.
- Never rely on `EnsureCreated()` for the production relational schema.
- Do not run automatic schema changes on every application startup in production.
- Apply migrations through a controlled deployment step.
- Verify migrations against a clean test database.
- Document how to back up the database before schema changes.
- Test both applying a new migration and deploying the application against an already migrated database.

Supabase free-tier limitations must be documented. Do not assume free hosting provides automatic backups, guaranteed uptime or sufficient capacity for business-critical payroll records.

# 5. Database Design

Use PostgreSQL UUID primary keys for business entities. Use appropriate foreign keys, unique constraints, indexes and transactions.

Configure decimal precision explicitly. Use UTC timestamps for recorded instants and store business time-zone settings for date-based operations.

## 5.1 Identity and business ownership

Implement the following logical tables:

### Businesses

Fields:

- Id
- Name
- BusinessType
- CurrencyCode
- TimeZone
- IsActive
- CreatedAtUtc
- UpdatedAtUtc

### Users

Fields:

- Id
- Email
- NormalizedEmail
- DisplayName
- AuthenticationProviderId or appropriate Identity fields
- IsActive
- CreatedAtUtc
- UpdatedAtUtc

Use ASP.NET Core Identity or a properly configured external identity provider. Do not implement custom password hashing.

### BusinessUsers

Fields:

- BusinessId
- UserId
- RoleId or membership-role representation
- Status
- JoinedAtUtc
- CreatedAtUtc

Add a unique constraint on `(BusinessId, UserId)` unless a documented membership-history design requires a different structure.

### Roles and permissions

Implement roles and permission mappings appropriate for:

- Owner
- Admin
- Manager
- Accountant
- InventoryStaff
- HRStaff
- Employee

Enforce permissions in the API. Frontend visibility is not authorization.

### AuditLogs

Fields:

- Id
- BusinessId
- ActorUserId
- Action
- EntityType
- EntityId
- OccurredAtUtc
- CorrelationId
- Sanitized metadata where appropriate

Never store passwords, tokens, complete payment credentials or other secrets in audit metadata.

## 5.2 Inventory

Implement:

- Categories
- Products
- Warehouses
- StockBalances
- StockMovements
- Suppliers
- Purchases
- PurchaseItems
- Customers
- Sales
- SaleItems

Products must include:

- Id
- BusinessId
- CategoryId
- SKU
- Name
- Unit
- CostPrice
- SellingPrice
- ReorderLevel
- IsActive
- CreatedAtUtc
- UpdatedAtUtc

Add a unique index on `(BusinessId, SKU)`.

StockBalances must identify the business, warehouse and product. Add a unique constraint on `(BusinessId, WarehouseId, ProductId)`.

StockMovements must record:

- BusinessId
- WarehouseId
- ProductId
- MovementType
- QuantityDelta
- UnitCost where applicable
- ReferenceType
- ReferenceId
- Reason
- CreatedByUserId
- OccurredAtUtc

Treat stock movements as the inventory history. Do not silently rewrite historical movements to correct mistakes.

## 5.3 Employees and attendance

Implement:

- Departments
- Employees
- AttendanceRecords
- LeaveRequests

Employee fields should include:

- Id
- BusinessId
- EmployeeCode
- FirstName
- LastName
- DepartmentId
- JoiningDate
- EmploymentStatus
- CreatedAtUtc
- UpdatedAtUtc

Add a unique index on `(BusinessId, EmployeeCode)`.

Attendance should include the employee, work date, attendance status, check-in, check-out and approval information where applicable.

Prevent duplicate employee attendance for the same business and work date unless multiple shifts or sessions are explicitly supported.

## 5.4 Payroll

Implement:

- SalaryStructures
- PayPeriods
- SalaryRecords
- SalaryRecordItems

Salary structures must be effective-dated.

Salary records must contain:

- EmployeeId
- PayPeriodId
- GrossAmount
- DeductionAmount
- NetAmount
- Status
- CalculatedAtUtc
- FinalizedAtUtc where applicable

Use decimal calculations. Do not use floating-point types for money.

Keep calculation rules separate from payment execution. Historical finalized payroll must not change silently when an employee's current salary structure changes.

Payroll rules and statutory deductions must be configurable and reviewed for applicable Indian legal requirements before real-world use.

## 5.5 Payments and expenses

Implement:

- Payments
- PaymentAllocations
- Expenses
- ExpenseAttachments

Payments should include:

- Id
- BusinessId
- PayeeType
- PayeeId
- Amount
- PaymentDate
- PaymentMethod
- ReferenceNumber
- Status
- CreatedByUserId
- CreatedAtUtc

Support partial payments through payment allocations.

Keep payment history immutable or use controlled reversal and correction records. Do not silently overwrite completed financial transactions.

## 5.6 Database conventions

- Use `numeric(18,2)` for ordinary currency amounts, subject to the selected currency requirements.
- Use `numeric(18,3)` or another justified precision for fractional inventory quantities.
- Use enums or constrained values for stable status fields.
- Use foreign keys and database constraints for essential invariants.
- Index foreign keys and frequently filtered columns.
- Add indexes for common business-scoped date, status and search queries.
- Use optimistic concurrency or appropriate locking for stock updates.
- Avoid hard-deleting records referenced by financial or inventory history.
- Use explicit EF Core entity configurations rather than relying entirely on conventions.

# 6. Mandatory Multi-Tenant Security

The application must isolate every business's data.

A user can belong to multiple businesses, but each request must operate only within a business to which that user has authorized access.

## 6.1 Tenant resolution

Implement:

- ICurrentUser
- ICurrentBusiness
- Business membership validation
- Permission-based authorization
- Tenant-aware data access

Resolve the active business from a validated request context, and verify membership for every request. Do not trust an arbitrary BusinessId sent by the client.

Every business-owned query, update, delete, export and file operation must enforce tenant ownership.

Use EF Core global query filters as a defence-in-depth mechanism where appropriate, but do not rely on them alone.

For update and delete operations, explicitly verify that the target record belongs to the authorized business.

Validate both parent and child ownership. For example, a purchase belonging to Business A must not reference a product belonging to Business B.

Where appropriate, enforce tenant consistency through composite foreign keys and database constraints.

Consider PostgreSQL row-level security as an additional layer only after defining and testing its policies, connection behaviour and interaction with connection pooling.

## 6.2 Required security tests

Create at least two businesses and users in automated tests.

Verify that:

- User A cannot read Business B's products.
- User A cannot update or delete Business B's products.
- User A cannot access Business B's employee records.
- User A cannot access Business B's payroll or payments.
- User A cannot export Business B's reports.
- User A cannot submit a forged BusinessId to bypass tenant isolation.
- A valid user cannot use an ID from another tenant as a nested resource.
- Background jobs and scheduled operations also enforce business ownership.
- Anonymous requests cannot access protected endpoints.

Every tenant isolation failure must fail closed.

# 7. Authentication and Authorization

Use ASP.NET Core Identity or a documented external identity provider.

Choose and document an authentication approach compatible with the existing React deployment.

If using bearer tokens:

- Use a properly configured identity/token implementation.
- Validate issuer, audience, signing keys and expiry.
- Keep access tokens short-lived.
- If refresh tokens are implemented, rotate them and store only appropriately protected or hashed representations.
- Support revocation and logout.
- Never place long-lived credentials in browser local storage without a documented security assessment.

If using cookies:

- Configure Secure, HttpOnly and appropriate SameSite attributes.
- Implement CSRF protection for state-changing requests where required.
- Configure CORS and credentials explicitly.

Implement authorization policies for business roles and sensitive operations.

Never authorize a request solely because the client sends a role or business ID.

Protect registration, login, invitation and password-reset workflows against abuse. Use rate limiting and generic responses where appropriate.

# 8. API Endpoints

Use the prefix `/api/v1`.

Use DTOs, request validation, consistent error responses and pagination.

## 8.1 Authentication and business management

Implement:

- POST `/api/v1/auth/register`
- POST `/api/v1/auth/login`
- POST `/api/v1/auth/logout`
- GET `/api/v1/me`
- GET `/api/v1/businesses`
- POST `/api/v1/businesses`
- GET `/api/v1/businesses/{businessId}`
- PATCH `/api/v1/businesses/{businessId}`
- GET `/api/v1/businesses/{businessId}/members`
- POST `/api/v1/businesses/{businessId}/invitations`
- PATCH `/api/v1/businesses/{businessId}/members/{userId}`

Implement appropriate email verification, invitation acceptance, role assignment and account recovery according to the chosen identity solution.

## 8.2 Inventory and purchasing

Implement:

- GET `/api/v1/products`
- POST `/api/v1/products`
- GET `/api/v1/products/{id}`
- PUT `/api/v1/products/{id}`
- DELETE `/api/v1/products/{id}` — deactivate rather than erase historical records
- GET `/api/v1/categories`
- POST `/api/v1/categories`
- GET `/api/v1/warehouses`
- POST `/api/v1/warehouses`
- GET `/api/v1/stock/balances`
- GET `/api/v1/stock/movements`
- POST `/api/v1/stock/adjustments`
- GET `/api/v1/suppliers`
- POST `/api/v1/suppliers`
- GET `/api/v1/purchases`
- POST `/api/v1/purchases`
- POST `/api/v1/purchases/{id}/receive`
- GET `/api/v1/sales`
- POST `/api/v1/sales`
- POST `/api/v1/sales/{id}/complete`

Purchase receipt and sale completion must update stock balances and stock movements atomically.

Prevent duplicate completion and duplicate receipt operations.

## 8.3 Employees and attendance

Implement:

- GET `/api/v1/employees`
- POST `/api/v1/employees`
- GET `/api/v1/employees/{id}`
- PUT `/api/v1/employees/{id}`
- GET `/api/v1/attendance`
- POST `/api/v1/attendance`
- PUT `/api/v1/attendance/{id}`
- GET `/api/v1/leave-requests`
- POST `/api/v1/leave-requests`
- POST `/api/v1/leave-requests/{id}/approve`
- POST `/api/v1/leave-requests/{id}/reject`

Restrict employee self-service access to the records permitted by their role.

## 8.4 Payroll and payments

Implement:

- GET `/api/v1/pay-periods`
- POST `/api/v1/pay-periods`
- POST `/api/v1/pay-periods/{id}/calculate`
- POST `/api/v1/pay-periods/{id}/finalize`
- GET `/api/v1/salary-records`
- GET `/api/v1/salary-records/{id}`
- GET `/api/v1/payments`
- POST `/api/v1/payments`
- GET `/api/v1/expenses`
- POST `/api/v1/expenses`
- PUT `/api/v1/expenses/{id}`
- POST `/api/v1/expenses/{id}/approve`

Payroll finalization and payment recording must have separate workflows.

Use idempotency controls for financial operations that may be retried. A repeated request must not accidentally create a duplicate payout.

## 8.5 Reports

Implement:

- GET `/api/v1/reports/dashboard`
- GET `/api/v1/reports/inventory`
- GET `/api/v1/reports/payroll`
- GET `/api/v1/reports/payments`
- GET `/api/v1/reports/expenses`
- GET `/api/v1/audit-logs`

All reports must enforce tenant and permission checks.

## 8.6 API response conventions

Use standard HTTP status codes and a consistent Problem Details format for errors.

For paginated endpoints, support:

- `page`
- `pageSize`
- `search`
- `sortBy`
- `sortDirection`
- Relevant filters such as status, category, date range and department

Enforce a maximum page size.

Return pagination metadata, such as:

```json
{
  "items": [],
  "page": 1,
  "pageSize": 25,
  "totalRecords": 0,
  "totalPages": 0
}
```

Validate sort fields against an allowlist. Do not concatenate arbitrary client-supplied strings into SQL.

Avoid exposing stack traces, database errors, internal paths or secrets in API responses.

# 9. Business Logic and Transaction Requirements

## 9.1 Inventory

- Receiving goods increases stock.
- Completing a sale reduces stock.
- Adjustments require an explicit reason.
- Corrections use reversal or adjustment movements.
- Stock movements and balance updates occur within the same transaction.
- Concurrent updates must not lose stock changes.
- Prevent negative stock by default unless a business setting explicitly permits it.
- If negative stock is enabled, apply the rule consistently under concurrent requests.
- Validate available stock before completing a sale.
- Repeated purchase receipt or sale completion must not create duplicate movements.

## 9.2 Payroll

- Calculate gross pay, deductions and net pay using decimal arithmetic.
- Validate pay period boundaries.
- Prevent duplicate payroll records for an employee and pay period.
- Require appropriate authorization for finalization.
- Do not change finalized records silently.
- Record corrections explicitly.
- Keep statutory deduction rules configurable and versioned where applicable.
- Do not treat a calculated salary as proof that a payment occurred.

## 9.3 Payments

- Validate that the payee and referenced records belong to the same business.
- Reject zero, negative or invalid payment amounts.
- Support partial allocations when applicable.
- Validate allocations against outstanding balances.
- Make payment recording idempotent.
- Record the actor and timestamps for sensitive changes.
- Use controlled reversals for erroneous completed payments.

## 9.4 Transaction boundaries

Use database transactions for workflows that update related records.

Use concurrency tokens, optimistic concurrency or row-level locking where required.

Do not hold database transactions open while waiting for an external API or user interaction.

# 10. Validation, Errors and Logging

Implement request validation using FluentValidation or an equivalent consistent validation approach.

Validate:

- Required fields and string lengths.
- Email formats.
- Valid dates and date ranges.
- Positive monetary values and quantities.
- Currency and time-zone formats.
- Foreign key ownership.
- SKU and employee-code uniqueness.
- Valid status transitions.
- Pagination limits.
- Business membership and role permissions.

Use structured logging with correlation IDs.

Log operational failures and security-relevant events without logging passwords, access tokens, database credentials, payroll secrets or unnecessary personal information.

Use Problem Details for consistent error responses.

Handle expected exceptions centrally. Do not wrap every operation in broad catch blocks that hide failures.

# 11. Automated Testing Strategy

Create unit, integration and tenant-security tests.

Use xUnit and compatible test libraries. Use a real PostgreSQL test environment for integration tests involving PostgreSQL-specific behaviour, constraints, transactions or row-level security.

Do not rely exclusively on EF Core's InMemory provider for relational database testing.

## 11.1 Unit tests

Test:

- Product validation.
- SKU and employee-code validation.
- Stock quantity calculations.
- Sale completion rules.
- Purchase receipt rules.
- Negative-stock restrictions.
- Payroll calculation arithmetic.
- Salary finalization rules.
- Payment allocation and partial payment rules.
- Permission policies.
- Invalid status transitions.
- Date-range validation.

## 11.2 Integration tests

Test:

- API startup and dependency injection.
- Database connectivity.
- Applying migrations to a clean database.
- Business and product creation.
- Product list filtering and pagination.
- Foreign key and unique constraints.
- Stock movement transactions.
- Purchase receipt updates.
- Sale completion updates.
- Duplicate operation rejection.
- Payroll calculation and finalization.
- Payment recording and allocation.
- Consistent error responses.
- Authentication and authorization.

## 11.3 Tenant-security tests

Test all important CRUD and reporting operations with at least two businesses.

Include attempts to:

- Read another business's records.
- Modify or delete another business's records.
- Submit forged tenant identifiers.
- Associate a child record with a foreign tenant's parent.
- Access payroll and payment information without permission.
- Export cross-tenant data.
- Bypass authorization through nested endpoints.
- Use inactive memberships.
- Access protected endpoints without authentication.

Use 404 for inaccessible tenant-owned resources where appropriate to avoid leaking their existence; use 403 when the resource is visible but the action is forbidden. Apply the policy consistently.

## 11.4 Concurrency tests

Simulate concurrent stock receipts and sales.

Verify that:

- Stock balances reconcile with committed movements.
- Concurrent changes are not silently lost.
- Duplicate idempotency keys do not create duplicate financial records.
- Transactions roll back completely when a required step fails.

## 11.5 Security tests

Test:

- Authentication expiry and invalid tokens.
- Role escalation attempts.
- Mass assignment of protected fields.
- SQL injection resistance.
- Rate limiting on sensitive endpoints.
- CORS restrictions.
- Cookie and CSRF settings if cookie authentication is used.
- Secret leakage in logs and API responses.
- File-upload validation if attachments are implemented.

# 12. Performance and Scalability

The application should support 51–200 users per business without introducing unnecessary distributed infrastructure.

Requirements:

- Use asynchronous database operations.
- Use `AsNoTracking()` for read-only queries where appropriate.
- Add indexes based on real query patterns.
- Paginate list endpoints.
- Avoid loading complete tables into application memory.
- Avoid N+1 query patterns.
- Project directly into DTOs for reports and list endpoints.
- Use database transactions for critical workflows.
- Configure connection pooling and reasonable command timeouts.
- Use cancellation tokens for long-running requests.
- Avoid premature caching of sensitive tenant-specific data.
- Ensure cached data, if introduced later, is tenant-scoped.
- Add query and endpoint performance measurements.
- Test representative list and dashboard queries using realistic data volumes.

Do not promise a specific throughput or latency without measuring it against the actual deployment configuration.

# 13. Production Configuration

Implement:

- HTTPS-only production access.
- Strict CORS allowlisting for the actual React frontend origins.
- Authentication and role-based authorization.
- Request validation.
- Rate limiting.
- Safe exception handling.
- Health checks.
- Structured logs.
- Correlation IDs.
- Database migration procedures.
- Secret management.
- Backup and restoration documentation.
- Dependency vulnerability checks.
- Automated build and test workflows.

Do not expose detailed health-check output publicly.

Use separate readiness and liveness checks where appropriate. Database readiness checks must not disclose connection details.

Configure file uploads through controlled object storage if expense attachments are implemented. Validate content type, size and permissions. Do not store uploaded files on ephemeral application-server disks.

# 14. Deployment and Free-Tier Requirements

Initial deployment should be cost-conscious but must not compromise data isolation or security.

Expected components:

- React frontend hosted on a suitable static hosting plan.
- ASP.NET Core API on a compatible hosting service.
- Supabase PostgreSQL.
- GitHub for source control and automated checks.

Requirements:

- Verify the current terms and limits of each selected hosting plan.
- Store secrets in the hosting platform's secret settings.
- Never expose privileged Supabase credentials to the frontend.
- Verify database network connectivity and SSL.
- Configure production CORS using the deployed frontend origin.
- Apply migrations through a controlled release step.
- Document database backup and restore procedures.
- Test restoring a backup before storing real business-critical records.
- Configure alerts for deployment failures and resource limits where available.
- Document free-tier pause behaviour, database capacity and lack of automatic backups where applicable.
- Provide a documented upgrade path to paid hosting.

Do not claim that the production system has reliable backups merely because it uses Supabase. Verify the actual backup configuration and recovery procedure.

# 15. Documentation Deliverables

Generate and maintain:

1. `README.md` — setup, architecture and run instructions.
2. `docs/architecture.md` — project boundaries and data flow.
3. `docs/database-schema.md` — tables, relationships, constraints and indexes.
4. `docs/api.md` — endpoint contracts and authorization requirements.
5. `docs/security.md` — authentication, authorization and tenant isolation.
6. `docs/testing.md` — test commands and required environment.
7. `docs/deployment.md` — build, migration and deployment instructions.
8. `docs/backup-restore.md` — backup verification and recovery steps.
9. `.env.example` — placeholder configuration only.
10. OpenAPI documentation for the API.

Document any implementation decisions that differ from this specification.

# 16. Required Implementation Tasks

Complete the following checklist in order.

## Phase 1 — Repository and foundation

- [ ] Inspect the repository and current SDK.
- [ ] Preserve the existing frontend.
- [ ] Create or adapt the .NET solution.
- [ ] Configure project references and dependency injection.
- [ ] Configure nullable reference types and analyzers.
- [ ] Add compatible EF Core and Npgsql packages.
- [ ] Configure development and production settings.
- [ ] Configure safe logging and exception handling.
- [ ] Create initial unit-test and integration-test projects.
- [ ] Verify the solution builds.

**Acceptance:** The solution builds successfully and the API starts without secrets being committed.

## Phase 2 — PostgreSQL and migrations

- [ ] Configure the Supabase PostgreSQL connection.
- [ ] Verify SSL and network connectivity.
- [ ] Create the initial entities.
- [ ] Configure relationships and indexes.
- [ ] Create the initial EF Core migration.
- [ ] Apply migrations to the development database.
- [ ] Test migrations against a clean PostgreSQL database.
- [ ] Document migration deployment and rollback/recovery procedures.
- [ ] Verify that credentials are absent from source control.

**Acceptance:** Tables and constraints are created correctly and integration tests can use PostgreSQL.

## Phase 3 — Authentication and multi-tenancy

- [ ] Implement authentication.
- [ ] Implement user and business membership management.
- [ ] Implement roles and permission policies.
- [ ] Implement current-user and current-business services.
- [ ] Add tenant-aware data access.
- [ ] Add membership validation.
- [ ] Add cross-tenant integration tests.
- [ ] Test inactive memberships and role escalation.
- [ ] Add audit logging for sensitive operations.

**Acceptance:** Cross-tenant access is rejected for every implemented business-owned resource.

## Phase 4 — Inventory and purchasing

- [ ] Implement categories and products.
- [ ] Implement warehouses and stock balances.
- [ ] Implement stock movements.
- [ ] Implement suppliers and purchase drafts.
- [ ] Implement purchase receipt workflows.
- [ ] Implement customers and sales.
- [ ] Implement sale completion.
- [ ] Implement stock adjustments.
- [ ] Add concurrency and idempotency controls.
- [ ] Add inventory integration tests.
- [ ] Add paginated API endpoints.

**Acceptance:** Stock balances reconcile with committed movements and duplicate operations cannot corrupt inventory.

## Phase 5 — Employees and attendance

- [ ] Implement departments and employees.
- [ ] Implement attendance.
- [ ] Implement leave requests.
- [ ] Implement approvals.
- [ ] Add employee-specific permissions.
- [ ] Add uniqueness and validation constraints.
- [ ] Add integration and authorization tests.

**Acceptance:** Employee records and attendance are correctly validated and isolated by business.

## Phase 6 — Payroll and finance

- [ ] Implement salary structures and effective dates.
- [ ] Implement pay periods.
- [ ] Implement salary calculations.
- [ ] Implement payroll finalization.
- [ ] Implement salary records and breakdowns.
- [ ] Implement payments and allocations.
- [ ] Implement expenses and approvals.
- [ ] Add idempotency and transaction boundaries.
- [ ] Add financial audit logging.
- [ ] Add payroll and payment tests.
- [ ] Document statutory-compliance assumptions.

**Acceptance:** Payroll calculations are reproducible, finalized records are controlled, and payment records reconcile with allocations.

## Phase 7 — Reports and frontend integration

- [ ] Implement dashboard summaries.
- [ ] Implement inventory reports.
- [ ] Implement payroll, payment and expense reports.
- [ ] Implement pagination and filters consistently.
- [ ] Implement CSV exports with authorization checks.
- [ ] Publish accurate OpenAPI contracts.
- [ ] Verify CORS for the actual React origin.
- [ ] Test API error handling and frontend integration.
- [ ] Document endpoint usage.

**Acceptance:** The existing React application can use the API without bypassing backend validation or tenant security.

## Phase 8 — Production readiness

- [ ] Run all unit tests.
- [ ] Run PostgreSQL integration tests.
- [ ] Run tenant-security tests.
- [ ] Run concurrency tests.
- [ ] Run security and dependency checks.
- [ ] Verify HTTPS, CORS and rate limits.
- [ ] Verify secret handling.
- [ ] Verify migration deployment.
- [ ] Test backup restoration.
- [ ] Configure logs and health checks.
- [ ] Document deployment and incident recovery.
- [ ] Produce a final implementation report.

**Acceptance:** All required tests pass, unresolved security issues are documented, and deployment and recovery instructions are reproducible.

# 17. Commands for Local Verification

Document and verify commands equivalent to:

```bash
dotnet restore
dotnet build
dotnet test
dotnet ef migrations list
dotnet ef database update
```

Use the correct project and startup-project arguments when running EF Core CLI commands from the repository root.

Provide commands for:

- Starting the API.
- Running unit tests.
- Running integration tests.
- Applying migrations to a development database.
- Checking migration status.
- Building a release configuration.
- Running the application with environment-specific settings.

Never apply development migrations to production without confirming the target database and reviewing the migration.

# 18. Final Definition of Done

The backend is not complete merely because the application compiles or Swagger opens.

It is complete for the initial release only when:

1. The API starts successfully.
2. The application connects to the intended PostgreSQL database.
3. Database migrations can be applied reproducibly.
4. Authentication and authorization work.
5. Every implemented business-owned resource is tenant-isolated.
6. Inventory transactions are consistent under concurrent operations.
7. Employee and attendance workflows pass their tests.
8. Payroll calculations and finalization rules are tested.
9. Payments and allocations cannot be duplicated through retries.
10. API validation, pagination and error responses are consistent.
11. Integration and security tests pass.
12. Secrets are not committed or exposed.
13. Deployment, backup and restoration procedures are documented.
14. The existing React frontend remains intact and can integrate with the API.
15. All known limitations and unimplemented requirements are explicitly reported.

# 19. Final Instructions to GitHub Copilot

Start by inspecting the existing repository and reporting the current state.

Then implement the backend in phases, following the task checklist.

Do not generate the entire system as one giant, untested code dump. Build and test each phase, preserve existing working code, and keep the implementation consistent with this specification.

If a required secret, external service or infrastructure capability is unavailable, document it and provide a safe configuration path rather than inventing credentials or claiming that the integration works.

At the end of each phase, report:

- Files created or changed.
- Database changes and migrations.
- API endpoints implemented.
- Tests executed and their actual results.
- Security considerations addressed.
- Known limitations.
- The next recommended task.

At the end of the implementation, provide a concise production-readiness report distinguishing verified functionality from anything still requiring manual configuration, security review, compliance review or deployment validation.
