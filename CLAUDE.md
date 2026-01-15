# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build and Run Commands

```bash
# Build the project
dotnet build

# Run the application
dotnet run --project SIM600.Simulation
# HTTP:  http://localhost:5050
# HTTPS: https://localhost:7219

# Restore dependencies
dotnet restore

# Entity Framework migrations
dotnet ef migrations add <MigrationName> --project SIM600.Simulation
dotnet ef database update --project SIM600.Simulation
```

## Architecture

ASP.NET Core 10.0 web application using a **hybrid MVC + Razor Pages** pattern:
- **MVC Controllers** (`Controllers/`) - Main application pages
- **Razor Pages** (`Areas/Identity/`) - Authentication flows (login, register, 2FA, account management)

**Data Access:** Entity Framework Core 10.0 with SQLite (`app.db`). Migrations in `Data/Migrations/`.

**Authentication:** ASP.NET Identity with:
- Email confirmation required (`RequireConfirmedAccount = true`)
- Two-factor authentication via authenticator apps (TOTP)
- QR code generation for 2FA setup (`wwwroot/js/qr.js` using qrcodejs)

**Email:** SendGrid configured in `appsettings.Development.json`. Use User Secrets for production keys.

## Key Files

- `Program.cs` - DI configuration, middleware pipeline
- `Data/ApplicationDbContext.cs` - EF Core context (extends IdentityDbContext)
- `Views/Shared/_Layout.cshtml` - Master layout with navbar
- `Areas/Identity/Pages/Account/Manage/EnableAuthenticator.cshtml` - 2FA setup with QR code

## Testing

No test projects currently exist. When adding tests, use xUnit.
