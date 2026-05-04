# Research: Insurance Policy Status Monitor

**Feature**: Insurance Policy Status Monitor  
**Branch**: 001-policy-status-monitor  
**Date**: May 4, 2026

## Research Objectives

This document captures research findings to resolve all NEEDS CLARIFICATION items from the technical context, enabling confident implementation decisions for the ASP.NET Core + SQL Server solution.

## 1. PDF Generation Library Selection

### Decision: QuestPDF

**Rationale:** QuestPDF provides the optimal balance of licensing (Community License free for < $1M revenue companies), modern .NET 8.0 support, excellent performance, and developer-friendly fluent API suitable for professional insurance documentation.

### Alternatives Considered

**iText7 for .NET**
- ❌ **Rejected** - AGPL licensing requires source disclosure OR $3,500+/developer commercial license
- While feature-rich and battle-tested, licensing costs and complexity outweigh benefits
- Overkill for basic policy status change notices

**PdfSharpCore**
- ❌ **Rejected** - MIT license perfect but maintenance concerns and poor Unicode support
- Manual layout calculations error-prone and time-consuming
- Irregular updates raise long-term maintenance risk

### Key Characteristics

**Performance:**
- Generates 100-200 PDFs per second
- ~80MB baseline memory + ~2MB per concurrent operation
- Easily handles 5000+ PDFs/day requirement with significant headroom

**Licensing:**
- Community License: FREE for companies < $1M annual revenue
- Professional License: $1,299/year if needed (still cost-effective vs development time saved)
- No AGPL contamination or source disclosure requirements

**Integration:**
- Clean abstraction through `IPdfGenerator` interface
- Factory pattern for instance creation
- Full Unicode support for special characters in policy data
- Professional output suitable for compliance documentation

**Dependencies:**
- Minimal: QuestPDF + SkiaSharp (2 packages total)
- Meets "minimal external dependencies" specification requirement

## 2. Email/Notification Service Selection

### Decision: MailKit

**Rationale:** MailKit is the industry-standard .NET email library with excellent SMTP support for internal mail servers, modern async patterns, and clean interfaces enabling proper abstraction and test isolation.

### Alternatives Considered

**SendGrid SDK**
- ❌ **Rejected** - Requires cloud SaaS dependency conflicting with "minimal external dependencies"
- No native SMTP support (API-only)
- Data sovereignty concerns for insurance domain
- Ongoing service costs

**System.Net.Mail**
- ❌ **Rejected** - Microsoft explicitly recommends against for new development
- Obsolete, poor async support, sealed classes difficult to abstract
- Maintenance mode with no new features

### Key Characteristics

**SMTP Support:**
- Full-featured SMTP client for internal Exchange/mail servers
- Multiple authentication methods: NTLM, OAuth2, Basic
- TLS/SSL support with connection pooling

**Performance:**
- Async/await throughout, meets <30 second notification requirement
- Efficient connection management
- Detailed SMTP exception handling for retry logic

**Integration:**
- Abstraction through `IEmailClient` → `INotificationService` layers
- Factory pattern for service creation
- Enables future SMS/push notification channels

**Resilience Strategy:**
- Integrate with Polly for retry policies
- Exponential backoff: 30s → 2min → 5min (per spec)
- Circuit breaker for SMTP server failures
- Jitter to prevent thundering herd

**Dependencies:**
- MailKit + MimeKit (2 NuGet packages)
- Minimal overhead, industry standard

**Testability:**
- Mock `IEmailClient` for unit tests (95%+ coverage support)
- Integration tests with in-memory SMTP server (smtp4dev, Papercut)
- Clean interface design enables complete test isolation

## 3. SQL Server Trigger Architecture

### Decision: CLR Trigger + .NET Background Service

**Rationale:** Hybrid approach leveraging SQL Server CLR triggers for immediate change detection with .NET background service for processing enables <5 second detection while keeping business logic in testable C# code.

### Architecture Components

**Database Layer:**
- SQL DML trigger on Policy status column
- Inserts change event to `PolicyStatusChangeEvents` table
- Lightweight trigger keeps database operations fast
- CLR assembly for calling .NET code (optional optimization)

**.NET Processing Service:**
- ASP.NET Core Worker Service polls or listens for change events
- Alternative: Service Broker for push notifications from SQL Server
- Processes events: PDF generation, notification, file transfer
- Retry and error handling in application layer

**Why Not Pure SQL:**
- Cannot easily generate PDFs in T-SQL
- Email libraries require .NET
- Business logic testability (95%+ coverage requires C# unit tests)
- Factory/Repository patterns require OOP

**Why Not Pure Application Polling:**
- Triggers ensure immediate detection (<5 seconds)
- No missed changes during app downtime
- Database guarantees transactional consistency

## 4. Testing Strategy Clarifications

### Unit Test Approach (95%+ Coverage)

**Mocking Strategy:**
- All external dependencies behind interfaces
- `IPdfGenerator` mocked with in-memory test implementation
- `IEmailClient` mocked to verify notification content
- `IFileTransferService` mocked for file operations
- `IRepository<T>` mocked for data access

**Test Organization:**
- Domain.Tests: Business logic (100% coverage target)
- Infrastructure.Tests: Factory implementations, service wrappers
- Data.Tests: Repository patterns, EF Core queries
- Integration.Tests: End-to-end workflows with real database

**Test Data Builders:**
- PolicyBuilder for test policy entities
- StatusChangeEventBuilder for event scenarios
- TestEmailClient for notification verification

## 5. Architecture Decisions

### Layered Architecture Implementation

**Presentation Layer:**
- Worker Service: Background processing loop
- Minimal logic: receive trigger events, dispatch to business layer

**Business Logic Layer:**
- `PolicyStatusService`: Core workflow orchestration
- `StatusChangeProcessor`: Process individual changes
- Domain entities: Policy, StatusChangeEvent, etc.
- Independent of infrastructure (database, PDF, email)

**Data Access Layer:**
- Repository pattern: `IPolicyRepository`, `IStatusChangeEventRepository`
- Entity Framework Core for database operations
- Abstraction enables test isolation and future database switching

**Infrastructure Layer:**
- `PdfGeneratorFactory`: Creates `IPdfGenerator` instances
- `NotificationServiceFactory`: Creates `INotificationService` instances
- Concrete implementations: `QuestPdfGenerator`, `MailKitEmailClient`
- File I/O services: `FileTransferService`

### Design Patterns Application

**Factory Pattern:**
- `PdfGeneratorFactory`: Returns `IPdfGenerator` (currently QuestPDF, swappable)
- `NotificationServiceFactory`: Returns `INotificationService` (currently MailKit, swappable)

**Repository Pattern:**
- `IPolicyRepository`, `IStatusChangeEventRepository` for all data access
- EF Core implementation, mockable for tests

**Why Minimal Patterns:**
- Specification requested "minimal" approach
- Additional patterns (Strategy, Observer, etc.) add complexity without clear benefit
- YAGNI principle: implement patterns when problems arise, not preemptively

## 6. Technology Stack Summary

### Finalized Stack

**Language/Framework:**
- C# 12 / .NET 8.0
- ASP.NET Core 8.0 Worker Service

**Database:**
- SQL Server 2019+
- Entity Framework Core 8.0
- DML Triggers for change detection

**PDF Generation:**
- QuestPDF (Community or Professional License)

**Email/Notifications:**
- MailKit + MimeKit
- Polly for resilience

**Testing:**
- xUnit
- Moq or NSubstitute (mocking)
- FluentAssertions
- Integration: Testcontainers for SQL Server

**Additional Libraries:**
- Serilog (structured logging)
- Microsoft.Extensions.Configuration
- Microsoft.Extensions.DependencyInjection

### Dependency Minimization

**Total NuGet Packages (excluding test):**
- QuestPDF: 2 packages
- MailKit: 2 packages
- Polly: 1 package
- EF Core: 3-4 packages
- Serilog: 2-3 packages
- **Total: ~12-15 packages** (reasonable for enterprise .NET application)

All dependencies justified by specification requirements and industry best practices.

## 7. Configuration & Deployment

### Configuration Requirements

**SMTP Settings:**
- Host: Internal mail server (e.g., smtp.company.local)
- Port: 587 (STARTTLS recommended)
- Authentication: Windows Auth (NTLM) or credentials
- From address: noreply@company.com

**File Transfer:**
- Destination path: UNC path or local folder
- Permissions: Write access for service account

**Database:**
- Connection string with appropriate permissions
- Trigger installation SQL scripts

**PDF Settings:**
- Template configuration
- Output directory (staging location)

### Deployment Model

**Windows Server:**
- Install as Windows Service
- Run under service account with SQL and file access
- Monitor with Windows Event Log + Serilog sinks

**Linux (optional):**
- Systemd service
- SQL Server on Linux support
- Same codebase, cross-platform .NET

## Research Validation

All NEEDS CLARIFICATION items resolved:
- ✅ PDF Library: QuestPDF selected with licensing clarity
- ✅ Email/Notification: MailKit selected with SMTP architecture defined
- ✅ Architecture patterns confirmed with rationale
- ✅ Testing strategy detailed for 95%+ coverage
- ✅ Technology stack finalized

Ready to proceed to Phase 1: Design & Contracts.
