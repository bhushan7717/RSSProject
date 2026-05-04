# Quickstart Guide: Insurance Policy Status Monitor

**Feature**: Insurance Policy Status Monitor  
**Audience**: Developers joining the project  
**Last Updated**: May 4, 2026

## What This System Does

The **Insurance Policy Status Monitor** automatically detects when an insurance policy status changes in SQL Server, then:
1. **Detects** the change within 5 seconds using database triggers
2. **Generates** a professional PDF document of the status change notice
3. **Notifies** the assigned insurance agent via email
4. **Transfers** the PDF to a designated file location for archival

**Business Value**: Saves agents 15+ minutes per policy change by automating manual documentation and notifications.

---

## Architecture at a Glance

```
SQL Server                 ASP.NET Core Worker Service
┌─────────────┐           ┌──────────────────────────────────┐
│   Policy    │           │  Presentation Layer              │
│   Table     │  Trigger  │  └─ Background Worker            │
│             ├──────────>│                                  │
│  (Status    │  Insert   │  Business Logic Layer            │
│   changes)  │  Event    │  └─ StatusChangeProcessor        │
└─────────────┘           │                                  │
                          │  Data Access Layer               │
                          │  └─ Repositories (EF Core)       │
                          │                                  │
                          │  Infrastructure Layer            │
                          │  ├─ PDF Generator (QuestPDF)     │
                          │  ├─ Email Service (MailKit)      │
                          │  └─ File Transfer                │
                          └──────────────────────────────────┘
```

**4-Layer Architecture**:
- **Presentation**: Worker service that polls for events
- **Business Logic**: Core workflow orchestration (testable, no dependencies)
- **Data Access**: Repository pattern for database operations
- **Infrastructure**: PDF generation, email, file I/O (swappable via Factory pattern)

---

## Quick Setup (5 Minutes)

### Prerequisites
- .NET 8.0 SDK
- SQL Server 2019+ (local or remote)
- Visual Studio 2022 or VS Code
- SMTP server access (for email notifications)

### Step 1: Clone & Restore

```powershell
git clone <repository-url>
cd RSSProject
dotnet restore
```

### Step 2: Configure Database

**Update Connection String** in `src/PolicyMonitor.Presentation/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "PolicyDatabase": "Server=localhost;Database=InsuranceDB;Integrated Security=true;TrustServerCertificate=true"
  }
}
```

**Run Database Migration**:

```powershell
cd src/PolicyMonitor.Data
dotnet ef database update
```

This creates:
- Policy tables
- StatusChangeEvent table
- GeneratedDocument, AgentNotification tables
- Database trigger for status change detection

### Step 3: Configure SMTP

**Update SMTP Settings** in `appsettings.Development.json`:

```json
{
  "SmtpSettings": {
    "Host": "smtp.company.local",  # Your SMTP server
    "Port": 587,
    "EnableSsl": true,
    "Username": "",  # Use User Secrets for credentials
    "Password": "",  # Use User Secrets for credentials
    "FromEmail": "noreply@company.com",
    "FromName": "Policy Monitor System"
  }
}
```

**Set Credentials with User Secrets** (recommended):

```powershell
cd src/PolicyMonitor.Presentation
dotnet user-secrets set "SmtpSettings:Username" "your-username"
dotnet user-secrets set "SmtpSettings:Password" "your-password"
```

### Step 4: Configure PDF & File Transfer

**Update Settings**:

```json
{
  "PdfSettings": {
    "GeneratorType": "QuestPDF",  # Default
    "OutputDirectory": "C:\\Temp\\PolicyPdfs",  # Staging location
    "TimeoutSeconds": 30
  },
  "FileTransferSettings": {
    "DestinationPath": "\\\\server\\share\\policies",  # UNC or local
    "RetryIntervalMinutes": 5,
    "MaxRetries": 24
  }
}
```

### Step 5: Run the Application

```powershell
cd src/PolicyMonitor.Presentation
dotnet run
```

**Expected Output**:
```
info: PolicyMonitor.Presentation.Workers.StatusChangeWorker[0]
      Status change monitoring started
info: PolicyMonitor.Presentation.Workers.StatusChangeWorker[0]
      Polling for pending events every 5 seconds...
```

### Step 6: Test the System

**Insert Test Policy**:

```sql
-- In SQL Server Management Studio or Azure Data Studio
USE InsuranceDB;

INSERT INTO Policies (PolicyNumber, PolicyholderName, PolicyType, 
                      CoverageAmount, EffectiveDate, ExpirationDate, Status)
VALUES ('POL-12345678', 'John Doe', 'Auto', 50000.00, 
        '2026-01-01', '2027-01-01', 'Pending');

-- Assign test agent
INSERT INTO Agents (EmployeeId, FirstName, LastName, Email, PreferredNotificationMethod)
VALUES ('EMP001', 'Jane', 'Smith', 'jane.smith@company.com', 'Email');

INSERT INTO PolicyAgents (PolicyNumber, AgentId, IsPrimaryAgent)
VALUES ('POL-12345678', 1, 1);

-- Trigger status change (this will fire the trigger)
UPDATE Policies 
SET Status = 'Approved' 
WHERE PolicyNumber = 'POL-12345678';
```

**What Should Happen**:
1. Trigger inserts event into `StatusChangeEvents` table
2. Worker service picks up event within 5 seconds
3. PDF generated at `C:\Temp\PolicyPdfs\POL-12345678_Approved_YYYYMMDD_HHMMSS.pdf`
4. Email sent to jane.smith@company.com
5. PDF transferred to configured destination
6. Event status updated to "Completed"

**Check Results**:

```sql
-- View processing status
SELECT * FROM StatusChangeEvents 
WHERE PolicyNumber = 'POL-12345678' 
ORDER BY ChangeTimestamp DESC;

-- View generated documents
SELECT * FROM GeneratedDocuments 
WHERE PolicyNumber = 'POL-12345678';

-- View notifications
SELECT * FROM AgentNotifications 
WHERE PolicyNumber = 'POL-12345678';
```

---

## Project Structure

```
src/
├── PolicyMonitor.Domain/              # Business Logic (NO dependencies)
│   ├── Entities/                      # Policy, StatusChangeEvent
│   ├── Services/                      # StatusChangeProcessor
│   └── Interfaces/                    # Service contracts
│
├── PolicyMonitor.Infrastructure/      # External integrations (SWAPPABLE)
│   ├── Pdf/
│   │   ├── IPdfGenerator.cs           # Interface
│   │   └── QuestPdfGenerator.cs       # QuestPDF implementation
│   ├── Notifications/
│   │   ├── INotificationService.cs
│   │   └── EmailNotificationService.cs # MailKit implementation
│   ├── FileTransfer/
│   │   └── FileTransferService.cs
│   └── Factories/                     # Factory pattern implementations
│       ├── PdfGeneratorFactory.cs
│       └── NotificationServiceFactory.cs
│
├── PolicyMonitor.Data/                # Data Access (Repository pattern)
│   ├── Context/
│   │   └── PolicyDbContext.cs         # EF Core DbContext
│   ├── Entities/                      # EF entity classes
│   ├── Repositories/                  # Repository implementations
│   │   ├── PolicyRepository.cs
│   │   ├── StatusChangeEventRepository.cs
│   │   └── GeneratedDocumentRepository.cs
│   └── Migrations/                    # EF Core migrations
│
├── PolicyMonitor.Presentation/        # Worker Service (entry point)
│   ├── Workers/
│   │   └── StatusChangeWorker.cs      # Background service
│   ├── Program.cs                     # DI configuration
│   └── appsettings.json               # Configuration
│
└── PolicyMonitor.Shared/              # Cross-cutting concerns
    ├── Logging/
    ├── Configuration/
    └── Extensions/

tests/
├── PolicyMonitor.Domain.Tests/        # Business logic unit tests (95%+ coverage)
├── PolicyMonitor.Infrastructure.Tests/ # Infrastructure unit tests
├── PolicyMonitor.Data.Tests/          # Repository unit tests
└── PolicyMonitor.Integration.Tests/   # End-to-end integration tests

database/
├── triggers/                          # SQL trigger scripts
│   └── trg_PolicyStatusChange.sql
├── stored-procedures/
└── migrations/                        # Raw SQL migration scripts
```

---

## Key Design Patterns

### Factory Pattern (Library Swappability)

**PDF Generator Factory**:
```csharp
// Configured in Program.cs
services.AddSingleton<IPdfGeneratorFactory, PdfGeneratorFactory>();

// Used in business logic
var pdfGenerator = _pdfGeneratorFactory.CreatePdfGenerator();
var result = await pdfGenerator.GeneratePolicyStatusChangePdfAsync(data, path);
```

**Why**: Switch from QuestPDF to iText7 by changing configuration, not code.

### Repository Pattern (Data Access Abstraction)

```csharp
// Interface in Domain, implementation in Data
public interface IPolicyRepository
{
    Task<Policy?> GetByIdAsync(string policyNumber);
    Task UpdatePolicyStatusAsync(string policyNumber, string newStatus);
}

// Business logic depends on interface (testable)
public class StatusChangeProcessor
{
    private readonly IPolicyRepository _policyRepo;
    
    public StatusChangeProcessor(IPolicyRepository policyRepo)
    {
        _policyRepo = policyRepo;  // Injected, mockable
    }
}
```

**Why**: Enables 95%+ test coverage by mocking data access.

---

## Running Tests

### Unit Tests (Fast - No Database)

```powershell
# Run all unit tests
dotnet test --filter "FullyQualifiedName~.Domain.Tests|.Infrastructure.Tests|.Data.Tests"

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

**Coverage Target**: 95%+ for Domain and Infrastructure layers

### Integration Tests (Slower - Uses Testcontainers)

```powershell
# Requires Docker Desktop running
dotnet test --filter "FullyQualifiedName~.Integration.Tests"
```

Tests full workflow with real SQL Server container.

### Test Example

```csharp
[Fact]
public async Task ProcessStatusChange_GeneratesPdf_WhenEventValid()
{
    // Arrange - Mock dependencies
    var mockPdfGenerator = new Mock<IPdfGenerator>();
    mockPdfGenerator.Setup(x => x.GeneratePolicyStatusChangePdfAsync(
        It.IsAny<PolicyStatusChangeData>(), 
        It.IsAny<string>(), 
        It.IsAny<CancellationToken>()))
        .ReturnsAsync(new PdfGenerationResult { Success = true });
    
    var processor = new StatusChangeProcessor(mockPdfGenerator.Object, ...);
    
    // Act
    var result = await processor.ProcessStatusChangeAsync(eventId);
    
    // Assert
    Assert.True(result.PdfGenerated);
    mockPdfGenerator.Verify(x => x.GeneratePolicyStatusChangePdfAsync(...), Times.Once);
}
```

---

## Common Development Tasks

### Add a New PDF Library

1. Create implementation: `src/PolicyMonitor.Infrastructure/Pdf/NewPdfGenerator.cs`
2. Implement `IPdfGenerator` interface
3. Update `PdfGeneratorFactory` to support new type
4. Add configuration option in `appsettings.json`
5. Write unit tests for new generator

**No changes to business logic required** - that's the power of abstraction!

### Add SMS Notifications

1. Create `SmsNotificationService : INotificationService`
2. Update `NotificationServiceFactory`
3. Add SMS provider configuration
4. Update `AgentNotification` to support SMS delivery method

**Business logic remains unchanged.**

### Debug a Failed Status Change

```sql
-- Find failed events
SELECT * FROM StatusChangeEvents 
WHERE ProcessingStatus = 'Failed' 
ORDER BY ChangeTimestamp DESC;

-- View error details
SELECT EventId, PolicyNumber, ErrorMessage, RetryCount
FROM StatusChangeEvents 
WHERE ProcessingStatus = 'Failed';

-- Manually retry
-- Set ProcessingStatus = 'Pending' and system will retry
UPDATE StatusChangeEvents 
SET ProcessingStatus = 'Pending', RetryCount = 0
WHERE EventId = '<guid>';
```

### View Application Logs

**Console** (Development):
```
info: PolicyMonitor.Domain.Services.StatusChangeProcessor[100]
      Processing event {EventId} for policy {PolicyNumber}
info: PolicyMonitor.Infrastructure.Pdf.QuestPdfGenerator[200]
      Generated PDF at C:\Temp\PolicyPdfs\POL-12345678_Approved_20260504_143022.pdf
```

**Serilog Sinks** (Production):
- File: `logs/policy-monitor-{Date}.txt`
- SQL: `Logs` table in database
- Application Insights (if configured)

---

## Configuration Reference

### Minimal Production Configuration

```json
{
  "ConnectionStrings": {
    "PolicyDatabase": "Server=prod-sql;Database=InsuranceDB;..."
  },
  "SmtpSettings": {
    "Host": "smtp.company.com",
    "Port": 587,
    "EnableSsl": true,
    "FromEmail": "noreply@company.com"
  },
  "PdfSettings": {
    "GeneratorType": "QuestPDF",
    "OutputDirectory": "C:\\PolicyMonitor\\Staging"
  },
  "FileTransferSettings": {
    "DestinationPath": "\\\\fileserver\\policies",
    "RetryIntervalMinutes": 5,
    "MaxRetries": 24
  },
  "WorkerSettings": {
    "PollingIntervalSeconds": 5,
    "MaxConcurrentProcessing": 10
  }
}
```

**Secrets** (Azure Key Vault, User Secrets, Environment Variables):
- SMTP Username/Password
- Database passwords
- QuestPDF license key (if Professional license)

---

## Troubleshooting

### Problem: Worker service not detecting changes

**Check**:
1. Trigger installed? Run `database/triggers/trg_PolicyStatusChange.sql`
2. Connection string correct? Test with `dotnet ef database update`
3. Worker running? Check Process Manager for `PolicyMonitor.Presentation.exe`

### Problem: PDF generation fails

**Check**:
1. Output directory exists and writable? `C:\Temp\PolicyPdfs`
2. QuestPDF license valid? Check logs for license errors
3. Unicode issues? Verify policy data doesn't have unsupported characters

### Problem: Email not sending

**Check**:
1. SMTP settings correct? Test connection with Telnet: `telnet smtp.company.com 587`
2. Credentials valid? Verify username/password in User Secrets
3. Firewall blocking? Check port 587/465/25 is open
4. TLS/SSL settings? Try `EnableSsl = true` and `EnableSsl = false`

### Problem: Tests failing

**Check**:
1. Docker running? (for integration tests with Testcontainers)
2. Test database accessible?
3. Mocks configured correctly?

---

## Performance Tuning

**Expected Performance**:
- **Change Detection**: <5 seconds from SQL update to event detection
- **PDF Generation**: ~200ms per document
- **Email Delivery**: <10 seconds to SMTP server
- **File Transfer**: <5 seconds for typical file sizes
- **Total**: <30 seconds end-to-end for typical case

**Bottlenecks**:
- SQL trigger execution (optimize trigger code)
- SMTP server latency (consider local SMTP relay)
- File transfer over network (use local destination during staging)

**Scaling**:
- Current: 5000 changes/day = ~0.06 changes/second (well within capacity)
- Max: System can handle ~10-20 concurrent changes with current config
- Scale up: Increase `MaxConcurrentProcessing` in WorkerSettings

---

## Next Steps

1. **Read**: [data-model.md](data-model.md) for entity relationships
2. **Review**: [contracts/service-contracts.md](contracts/service-contracts.md) for interfaces
3. **Study**: [research.md](research.md) for technology decisions
4. **Plan**: [plan.md](plan.md) for full implementation details
5. **Tasks**: [tasks.md](tasks.md) for development task breakdown

**Ready to implement?** Start with Task 1 in [tasks.md](tasks.md) (generated by `/speckit.tasks` command).

---

## Support & Resources

- **Repository**: [GitHub URL]
- **Documentation**: `specs/001-policy-status-monitor/`
- **Team Contact**: [Team Slack/Email]
- **QuestPDF Docs**: https://www.questpdf.com/documentation
- **MailKit Docs**: https://github.com/jstedfast/MailKit

**Questions?** Check existing documentation first, then ask the team! 🚀
