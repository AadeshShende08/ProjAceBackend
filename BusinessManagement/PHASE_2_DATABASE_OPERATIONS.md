# Phase 2 database operations

The API uses `ConnectionStrings:DefaultConnection` and gives environment variables and .NET User Secrets precedence over `appsettings.json`.

## Configure a development connection

Do not put the password in source control. From the `BusinessManagement` directory, use one of these options:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=...;Port=5432;Database=postgres;Username=...;Password=...;SSL Mode=Require;Trust Server Certificate=false" --project BusinessManagement.Api\BusinessManagement.Api.csproj
```

or set the process environment variable:

```powershell
$env:ConnectionStrings__DefaultConnection = "Host=...;Port=5432;Database=postgres;Username=...;Password=...;SSL Mode=Require;Trust Server Certificate=false"
```

Use a restricted development role and a separate database or Supabase project where possible. The runtime connection requires encrypted transport.

## Review and apply migrations

From the `BusinessManagement` directory:

```powershell
dotnet ef migrations list --project BusinessManagement.Infrastructure\BusinessManagement.Infrastructure.csproj --startup-project BusinessManagement.Api\BusinessManagement.Api.csproj
dotnet ef database update --project BusinessManagement.Infrastructure\BusinessManagement.Infrastructure.csproj --startup-project BusinessManagement.Api\BusinessManagement.Api.csproj
```

Review the generated SQL or migration files and confirm the target database before applying. Do not run `database update` automatically during API startup or against production.

## Rollback and recovery

1. Back up the development database before applying a migration.
2. If a migration has not been applied, remove it only with:

```powershell
dotnet ef migrations remove --project BusinessManagement.Infrastructure\BusinessManagement.Infrastructure.csproj --startup-project BusinessManagement.Api\BusinessManagement.Api.csproj
```

3. If a migration is already applied, create a corrective migration. Do not edit an applied migration or silently delete inventory/financial history.
4. For a controlled rollback, restore the backup or explicitly update to a reviewed previous migration after confirming data-loss implications:

```powershell
dotnet ef database update <ReviewedPreviousMigration> --project BusinessManagement.Infrastructure\BusinessManagement.Infrastructure.csproj --startup-project BusinessManagement.Api\BusinessManagement.Api.csproj
```

5. Verify the migration history, application startup, and integrity checks after recovery.

The initial migration was generated locally. Applying it and verifying PostgreSQL connectivity require a valid development connection supplied outside source control.
