# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build and Run Commands

```bash
# Build the project
dotnet build

# Run the application (launches at http://localhost:5050 or https://localhost:7219)
dotnet run --project SIM600.Simulation

# Restore dependencies
dotnet restore

# Entity Framework migrations
dotnet ef migrations add <MigrationName> --project SIM600.Simulation
dotnet ef database update --project SIM600.Simulation
```

## Architecture

This is an ASP.NET Core 9.0 MVC web application with ASP.NET Identity for authentication.

**Key Components:**
- **Data/ApplicationDbContext.cs** - Entity Framework Core context extending IdentityDbContext, uses SQLite (app.db)
- **Controllers/** - MVC controllers (currently HomeController)
- **Views/** - Razor views with shared _Layout.cshtml master page
- **Areas/Identity/** - Scaffolded ASP.NET Identity pages for authentication
- **Program.cs** - Application configuration and middleware pipeline

**Data Access:** Entity Framework Core 9.0 with SQLite provider. Migrations are in `Data/Migrations/`.

**Authentication:** ASP.NET Identity with confirmed account requirement enabled.

## Project Structure

```
SIM600.Simulation/
├── Controllers/      # MVC controllers
├── Models/           # View models and domain models
├── Data/             # DbContext and EF migrations
├── Views/            # Razor views organized by controller
├── Areas/Identity/   # Identity UI scaffolding
├── wwwroot/          # Static assets (Bootstrap, jQuery, site.css/js)
└── Program.cs        # App entry point and configuration
```

## Testing

No test projects currently exist. When adding tests, use xUnit with the standard .NET test patterns.
