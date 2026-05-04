# Implementation Plan: Insurance Policy Status Monitor

**Branch**: `001-policy-status-monitor` | **Date**: May 4, 2026 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/001-policy-status-monitor/spec.md`

## Summary

An automated system that monitors SQL Server database for insurance policy status changes using database triggers, generates PDF documentation, sends notifications to agents, and transfers files to designated locations. The solution leverages ASP.NET Core for the processing engine with minimal external dependencies, using abstraction layers (Factory and Repository patterns) for PDF generation and notification services to enable future library switching.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0  
**Primary Framework**: ASP.NET Core 8.0 (Background Service for processing)  
**Database**: SQL Server 2019+ (triggers, stored procedures, CLR assemblies)  
**PDF Library**: QuestPDF (Community License, swappable via Factory pattern)  
**Email/Notification**: MailKit + MimeKit (SMTP client for internal mail servers)  
**Resilience**: Polly (retry policies, circuit breaker, exponential backoff)  
**Testing**: xUnit, Moq (for mocking), FluentAssertions  
**Target Platform**: Windows Server 2019+ / Linux (with SQL Server support)  
**Project Type**: Background processing service (ASP.NET Core Worker Service)  
**Performance Goals**: Process 5000 policy changes/day, <5 sec change detection, <30 sec notification delivery  
**Constraints**: 99.9% uptime during business hours, 95%+ unit test coverage, minimal external dependencies  
**Scale/Scope**: Single database instance, up to 1000 concurrent policy changes, 7-year audit retention

### Technology Stack Summary

**Core Dependencies** (~12-15 NuGet packages total):
- QuestPDF + SkiaSharp (2 packages) - PDF generation
- MailKit + MimeKit (2 packages) - Email delivery
- Polly (1 package) - Resilience policies
- Entity Framework Core (3-4 packages) - Data access
- Serilog (2-3 packages) - Structured logging

**All dependencies justified by specification requirements and industry best practices.**

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

**Status**: ✅ PASSED (No project constitution defined - no gates to evaluate)

*Note: Constitution file exists but is not yet populated with project-specific principles. All architectural decisions will follow industry best practices and specification requirements.*

## Project Structure

### Documentation (this feature)

```text
specs/[###-feature]/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
src/
├── PolicyMonitor.Domain/              # Business Logic Layer
│   ├── Entities/                      # Policy, StatusChangeEvent, etc.
│   ├── Services/                      # Core business logic
│   └── Interfaces/                    # Service contracts
│
├── PolicyMonitor.Infrastructure/      # Infrastructure Layer
│   ├── Pdf/                          # PDF generation (Factory pattern)
│   ├── Notifications/                # Email/notification services (Factory pattern)
│   ├── FileTransfer/                 # File operations
│   └── Factories/                    # Concrete factory implementations
│
├── PolicyMonitor.Data/                # Data Access Layer
│   ├── Repositories/                 # Repository pattern implementations
│   ├── Context/                      # EF Core DbContext
│   └── Entities/                     # EF entities (if different from domain)
│
├── PolicyMonitor.Presentation/        # Presentation Layer
│   ├── Workers/                      # Background service workers
│   ├── TriggerHandlers/              # SQL CLR trigger handlers
│   └── Program.cs                    # Application entry point
│
└── PolicyMonitor.Shared/              # Cross-cutting concerns
    ├── Logging/
    ├── Configuration/
    └── Extensions/

database/
├── triggers/                          # SQL trigger scripts
├── stored-procedures/                 # SQL stored procedures
├── clr/                              # CLR assembly code
└── migrations/                        # Database migration scripts

tests/
├── PolicyMonitor.Domain.Tests/        # Unit tests (business logic)
├── PolicyMonitor.Infrastructure.Tests/ # Unit tests (infrastructure)
├── PolicyMonitor.Data.Tests/          # Unit tests (data access)
├── PolicyMonitor.Integration.Tests/   # Integration tests
└── PolicyMonitor.TestHelpers/         # Test utilities, mocks, builders
```

**Structure Decision**: Standard 4-layer ASP.NET Core architecture following the specification requirements. Domain layer contains business logic isolated from infrastructure. Factory pattern for PDF/notification services enables library switching. Repository pattern abstracts data access. SQL Server components (triggers, CLR) isolated in database folder for maintainability.

## Complexity Tracking

**No constitution violations** - No project constitution defined yet. All architectural decisions follow industry best practices and align with specification requirements.

---

## Phase Completion Status

### Phase 0: Research & Outline ✅ COMPLETE

**Status**: All NEEDS CLARIFICATION items resolved

**Artifacts Generated**:
- [research.md](research.md) - Technology stack decisions and rationale

**Key Decisions**:
1. **PDF Library**: QuestPDF selected over iText7 (AGPL licensing issues) and PdfSharpCore (maintenance concerns)
2. **Email Service**: MailKit selected over SendGrid SDK (SaaS dependency) and System.Net.Mail (obsolete)
3. **Architecture**: Hybrid SQL Server triggers + .NET Worker Service for <5 second detection with testable business logic
4. **Testing Strategy**: Interface-based mocking for all dependencies, 95%+ unit test coverage target

**Research Summary**: All technical unknowns resolved with clear rationale documented. Stack minimizes external dependencies while enabling swappability through Factory pattern.

---

### Phase 1: Design & Contracts ✅ COMPLETE

**Status**: Data model, contracts, and quickstart guide created

**Artifacts Generated**:
- [data-model.md](data-model.md) - Complete entity definitions, relationships, validation rules, state transitions
- [contracts/service-contracts.md](contracts/service-contracts.md) - 11 interface contracts for Factory and Repository patterns
- [quickstart.md](quickstart.md) - Developer onboarding guide with 5-minute setup

**Design Summary**:
- 7 core entities with full relationships and constraints
- 11 service contracts enabling 95%+ test coverage
- 4-layer architecture mapped to project structure
- SQL Server trigger-based change detection
- Factory pattern for PDF/notification swappability
- Repository pattern for data access abstraction

**Constitution Re-Check**: ✅ PASSED (No constitution defined - no violations)

---

### Phase 2: Task Generation - PENDING

**Next Command**: /speckit.tasks

This command will generate 	asks.md with dependency-ordered implementation tasks based on the design artifacts created in Phases 0 and 1.

---

## Implementation Readiness

✅ **Technology Stack**: Finalized and justified  
✅ **Architecture**: 4-layer design with clear separation of concerns  
✅ **Data Model**: Complete with validation rules and relationships  
✅ **Service Contracts**: All interfaces defined for Factory/Repository patterns  
✅ **Developer Guide**: Quickstart created for team onboarding  
✅ **Agent Context**: Updated to reference this plan

**Ready for**: Task generation (/speckit.tasks) and implementation (/speckit.implement)

---

## Summary

This implementation plan provides:
- Complete technical context with ASP.NET Core + SQL Server stack
- Research-backed decisions for PDF generation (QuestPDF) and email delivery (MailKit)
- Comprehensive data model with 7 entities and full relationship mapping
- 11 service contracts implementing Factory and Repository patterns
- Standard 4-layer architecture for testability and maintainability
- Developer quickstart guide for rapid team onboarding

**Estimated Effort**: 3-4 weeks for 1-2 developers
**Complexity**: Medium (standard enterprise patterns, well-defined requirements)
**Risk**: Low (proven technologies, clear architecture, comprehensive design)
