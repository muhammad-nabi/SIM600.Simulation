# SIM600.Simulation

ASP.NET Core 10.0 web application with secure authentication, featuring password-based login, magic links (passwordless), and two-factor authentication.

## Features

- **Hybrid MVC + Razor Pages** architecture
- **ASP.NET Identity** authentication with email confirmation required
- **Two-Factor Authentication (2FA)** via TOTP authenticator apps with QR code setup
- **Magic Link Login** - passwordless authentication via email
- **Rate Limiting** for magic link requests (3 requests per 15 minutes)
- **Azure Communication Services** email integration
- **Automated Deployment** via GitHub Actions with rollback support

## Technology Stack

- .NET 10.0 / ASP.NET Core
- Entity Framework Core 10.0
- SQLite database
- ASP.NET Identity
- Azure Communication Services (Email)
- Bootstrap 5

## Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
- Azure Communication Services resource (for email functionality)

## Getting Started

### 1. Clone the repository

```bash
git clone <repository-url>
cd SIM600.Simulation
```

### 2. Configure Azure Email Settings

Create or update `appsettings.Development.json` with your Azure Communication Services credentials:

```json
{
  "AzureEmailSettings": {
    "ConnectionString": "endpoint=https://your-acs-resource.communication.azure.com/;accesskey=...",
    "SenderAddress": "DoNotReply@your-domain.azurecomm.net",
    "SenderDisplayName": "SIM600 Simulation"
  }
}
```

For production, use [User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) or environment variables.

### 3. Build and Run

```bash
# Restore dependencies
dotnet restore SIM600.Simulation/SIM600.Simulation.csproj

# Build the project
dotnet build SIM600.Simulation/SIM600.Simulation.csproj

# Run the application
dotnet run --project SIM600.Simulation/SIM600.Simulation.csproj
```

The application will be available at:
- HTTP: http://localhost:5050
- HTTPS: https://localhost:7219

## Project Structure

```
SIM600.Simulation/
├── .github/workflows/     # GitHub Actions deployment workflow
├── SIM600.Simulation/     # Main application project
│   ├── Areas/Identity/    # Razor Pages for authentication flows
│   ├── Constants/         # Application constants
│   ├── Controllers/       # MVC controllers
│   ├── Data/              # EF Core context and migrations
│   ├── Models/            # View models
│   ├── Services/          # Application services (email sender)
│   ├── Views/             # MVC views
│   ├── wwwroot/           # Static files (CSS, JS)
│   ├── Program.cs         # Application entry point and DI configuration
│   └── app.db             # SQLite database
└── SIM600.sln             # Solution file
```

## Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "DataSource=app.db;Cache=Shared"
  },
  "AzureEmailSettings": {
    "ConnectionString": "",
    "SenderAddress": "",
    "SenderDisplayName": "SIM600 Simulation"
  }
}
```

## Authentication Flows

### Registration
1. User registers with email and password
2. Confirmation email sent via Azure Communication Services
3. User clicks confirmation link to activate account

### Password Login
1. User enters email and password
2. If 2FA is enabled, user is prompted for authenticator code
3. User is signed in upon successful authentication

### Magic Link Login (Passwordless)
1. User requests magic link by entering their email
2. Email with one-time login link is sent (valid for 15 minutes)
3. User clicks link to sign in automatically
4. Rate limited to 3 requests per 15-minute window

### Two-Factor Authentication Setup
1. User navigates to Account > Two-factor authentication
2. Scans QR code with authenticator app (Google Authenticator, Microsoft Authenticator, etc.)
3. Enters verification code to confirm setup
4. Recovery codes provided for backup access

## Entity Framework Migrations

```bash
# Add a new migration
dotnet ef migrations add <MigrationName> --project SIM600.Simulation

# Update the database
dotnet ef database update --project SIM600.Simulation
```

## Deployment

The project includes a GitHub Actions workflow (`.github/workflows/deploy.yml`) that:

1. Triggers on push to the `master` branch
2. Builds and publishes the application
3. Deploys to an Azure VM via self-hosted runner
4. Preserves production configuration and database during deployment
5. Automatically rolls back on deployment failure

### Self-Hosted Runner Setup

The deployment requires a self-hosted GitHub Actions runner on the target VM. See [GitHub documentation](https://docs.github.com/en/actions/hosting-your-own-runners) for setup instructions.

## License

[Add license information here]
