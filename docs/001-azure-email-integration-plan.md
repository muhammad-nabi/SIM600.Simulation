# Plan: Azure Communication Services Email Integration

## Summary
Replace unused SendGrid configuration with Azure Communication Services Email to enable account activation emails and other Identity email flows.

## Current State
- `RequireConfirmedAccount = true` but NO `IEmailSender` implementation exists
- SendGrid settings in `appsettings.Development.json` are unused (and exposed)
- Services folder exists but is empty
- 6 Identity pages inject `IEmailSender`: Register, ForgotPassword, ExternalLogin, Email management (2), ResendEmailConfirmation

---

## Implementation Steps

### 1. Add NuGet Package
**File:** `SIM600.Simulation/SIM600.Simulation.csproj`

Add:
```xml
<PackageReference Include="Azure.Communication.Email" Version="1.0.1" />
```

### 2. Create Configuration Options Class
**File:** `SIM600.Simulation/Services/AzureEmailOptions.cs` (NEW)

```csharp
namespace SIM600.Simulation.Services;

public class AzureEmailOptions
{
    public const string SectionName = "AzureEmailSettings";
    public string ConnectionString { get; set; } = string.Empty;
    public string SenderAddress { get; set; } = string.Empty;
    public string SenderDisplayName { get; set; } = "SIM600 Simulation";
}
```

### 3. Create Email Sender Service
**File:** `SIM600.Simulation/Services/AzureEmailSender.cs` (NEW)

Implements `IEmailSender` from `Microsoft.AspNetCore.Identity.UI.Services`:
- Initialize `EmailClient` from connection string
- `SendEmailAsync` sends via Azure Communication Services
- Proper logging (ILogger<AzureEmailSender>)
- Graceful handling when not configured (logs warning, doesn't throw)
- Throws `RequestFailedException` on Azure errors (Identity pages handle gracefully)

### 4. Update Configuration Files

**File:** `SIM600.Simulation/appsettings.Development.json`

Replace SendGrid section with:
```json
{
  "AzureEmailSettings": {
    "ConnectionString": "",
    "SenderAddress": "",
    "SenderDisplayName": "SIM600 Simulation"
  }
}
```
(Remove the exposed SendGrid API key)

### 5. Register Services in DI
**File:** `SIM600.Simulation/Program.cs`

Add after line 15 (`AddControllersWithViews`):
```csharp
// Configure Azure Email Settings
builder.Services.Configure<AzureEmailOptions>(
    builder.Configuration.GetSection(AzureEmailOptions.SectionName));

// Register IEmailSender implementation
builder.Services.AddTransient<IEmailSender, AzureEmailSender>();
```

Add using statements:
```csharp
using Microsoft.AspNetCore.Identity.UI.Services;
using SIM600.Simulation.Services;
```

---

## Files to Modify/Create

| File | Action |
|------|--------|
| `SIM600.Simulation.csproj` | Add Azure.Communication.Email package |
| `Services/AzureEmailOptions.cs` | CREATE - Configuration class |
| `Services/AzureEmailSender.cs` | CREATE - IEmailSender implementation |
| `appsettings.Development.json` | Replace SendGrid with AzureEmailSettings |
| `Program.cs` | Add service registration |

---

## Error Handling Strategy

| Scenario | Behavior |
|----------|----------|
| Connection string empty | Log warning, skip sending (no exception) |
| Azure API error | Log error, throw (Identity handles gracefully) |
| Network failure | Log error, throw |

User flows continue even if email fails - user can retry via "Resend confirmation email".

---

## Verification

1. **Build:** `dotnet build` - should succeed with no errors
2. **Run:** `dotnet run --project SIM600.Simulation` - should start without exceptions
3. **Test without Azure config:**
   - Register new user
   - Check logs for "Email client not configured" warning
   - Flow should complete (user created, unconfirmed)
4. **Test with Azure config:**
   - Set connection string and sender address
   - Register new user
   - Verify confirmation email received
   - Click confirmation link
   - Test forgot password flow

---

## Azure Prerequisites (User Action Required)

Before emails will actually send:
1. Create Azure Communication Services resource
2. Enable Email Communication Services
3. Configure verified domain
4. Get connection string from Keys section
5. Get sender address from Domains section

---

## Implementation Validation Report

| Step | Plan | Implementation | Status |
|------|------|----------------|--------|
| 1. NuGet Package | `Azure.Communication.Email 1.0.1` | csproj line 15 | ✅ MATCH |
| 2. AzureEmailOptions.cs | 3 properties + SectionName | Created with XML docs | ✅ MATCH |
| 3. AzureEmailSender.cs | IEmailSender, logging, graceful degradation | 97 lines, all requirements | ✅ MATCH |
| 4. appsettings.Development.json | Replace SendGrid → AzureEmailSettings | SendGrid removed, Azure added | ✅ MATCH |
| 5. Program.cs | Using statements + DI registration | Lines 2, 5, 19-22 | ✅ MATCH |

### Error Handling Verification

| Scenario | Expected | Actual | Status |
|----------|----------|--------|--------|
| Empty connection string | Log warning, no throw | Lines 42-48: returns silently | ✅ |
| Azure API error | Log + throw | Lines 82-89: RequestFailedException | ✅ |
| Network/other failure | Log + throw | Lines 91-95: generic Exception | ✅ |

### Build Verification
- `dotnet build` → **Succeeded** (0 warnings, 0 errors)

**Conclusion:** Implementation matches plan 100%. All 5 steps completed correctly.
