# Tasks: Insurance Policy Status Monitor

**Input**: Design documents from `/specs/001-policy-status-monitor/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: Not explicitly requested in specification - focusing on implementation tasks only

**Organization**: Tasks grouped by user story to enable independent implementation and testing

## Format: `- [ ] [ID] [P?] [Story?] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2, US3, US4)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [ ] T001 Create solution structure with 5 projects: PolicyMonitor.Domain, PolicyMonitor.Infrastructure, PolicyMonitor.Data, PolicyMonitor.Presentation, PolicyMonitor.Shared
- [ ] T002 Initialize PolicyMonitor.Domain class library (.NET 8.0) in src/PolicyMonitor.Domain/
- [ ] T003 [P] Initialize PolicyMonitor.Infrastructure class library (.NET 8.0) in src/PolicyMonitor.Infrastructure/
- [ ] T004 [P] Initialize PolicyMonitor.Data class library (.NET 8.0) in src/PolicyMonitor.Data/
- [ ] T005 [P] Initialize PolicyMonitor.Presentation Worker Service (.NET 8.0) in src/PolicyMonitor.Presentation/
- [ ] T006 [P] Initialize PolicyMonitor.Shared class library (.NET 8.0) in src/PolicyMonitor.Shared/
- [ ] T007 Add NuGet packages to PolicyMonitor.Infrastructure: QuestPDF, SkiaSharp
- [ ] T008 [P] Add NuGet packages to PolicyMonitor.Infrastructure: MailKit, MimeKit
- [ ] T009 [P] Add NuGet packages to PolicyMonitor.Infrastructure: Polly
- [ ] T010 [P] Add NuGet packages to PolicyMonitor.Data: Microsoft.EntityFrameworkCore.SqlServer, Microsoft.EntityFrameworkCore.Tools
- [ ] T011 [P] Add NuGet packages to PolicyMonitor.Shared: Serilog, Serilog.Sinks.Console, Serilog.Sinks.File
- [ ] T012 Create appsettings.json configuration structure in src/PolicyMonitor.Presentation/appsettings.json
- [ ] T013 [P] Create appsettings.Development.json in src/PolicyMonitor.Presentation/appsettings.Development.json
- [ ] T014 [P] Create .editorconfig with C# coding standards in repository root
- [ ] T015 Create database/ folder structure: triggers/, stored-procedures/, migrations/ in database/

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [ ] T016 Create base IRepository<TEntity, TKey> interface in src/PolicyMonitor.Data/Repositories/IRepository.cs
- [ ] T017 Create base Repository<TEntity, TKey> implementation in src/PolicyMonitor.Data/Repositories/Repository.cs
- [ ] T018 Create PolicyDbContext with connection string configuration in src/PolicyMonitor.Data/Context/PolicyDbContext.cs
- [ ] T019 [P] Create Serilog configuration helper in src/PolicyMonitor.Shared/Logging/LoggingConfiguration.cs
- [ ] T020 [P] Create configuration extensions for app settings in src/PolicyMonitor.Shared/Configuration/ConfigurationExtensions.cs
- [ ] T021 Setup Polly retry policies for PDF generation in src/PolicyMonitor.Shared/Resilience/PdfRetryPolicy.cs
- [ ] T022 [P] Setup Polly retry policies for email delivery in src/PolicyMonitor.Shared/Resilience/EmailRetryPolicy.cs
- [ ] T023 [P] Setup Polly retry policies for file transfer in src/PolicyMonitor.Shared/Resilience/FileTransferRetryPolicy.cs
- [ ] T024 Create base domain entity class with common properties in src/PolicyMonitor.Domain/Entities/BaseEntity.cs
- [ ] T025 Configure dependency injection container structure in src/PolicyMonitor.Presentation/Program.cs
- [ ] T026 Create custom exception types: PdfGenerationException, NotificationException, FileTransferException in src/PolicyMonitor.Domain/Exceptions/

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Automatic Policy Status Change Detection (Priority: P1) 🎯 MVP

**Goal**: Detect policy status changes within 5 seconds using SQL Server triggers and capture change events for processing

**Independent Test**: Update a policy status in database and verify StatusChangeEvent is created within 5 seconds with correct previous/new status values

### Data Layer for User Story 1

- [ ] T027 [P] [US1] Create Policy entity in src/PolicyMonitor.Domain/Entities/Policy.cs
- [ ] T028 [P] [US1] Create PolicyStatus enumeration in src/PolicyMonitor.Domain/Enums/PolicyStatus.cs
- [ ] T029 [P] [US1] Create StatusChangeEvent entity in src/PolicyMonitor.Domain/Entities/StatusChangeEvent.cs
- [ ] T030 [P] [US1] Create Agent entity in src/PolicyMonitor.Domain/Entities/Agent.cs
- [ ] T031 [US1] Configure Policy entity mapping in PolicyDbContext in src/PolicyMonitor.Data/Context/PolicyDbContext.cs
- [ ] T032 [P] [US1] Configure StatusChangeEvent entity mapping in PolicyDbContext in src/PolicyMonitor.Data/Context/PolicyDbContext.cs
- [ ] T033 [P] [US1] Configure Agent entity mapping in PolicyDbContext in src/PolicyMonitor.Data/Context/PolicyDbContext.cs
- [ ] T034 [US1] Create EF Core migration for Policy, StatusChangeEvent, Agent tables in src/PolicyMonitor.Data/Migrations/

### Repository Layer for User Story 1

- [ ] T035 [P] [US1] Create IPolicyRepository interface in src/PolicyMonitor.Data/Repositories/IPolicyRepository.cs
- [ ] T036 [P] [US1] Create IStatusChangeEventRepository interface in src/PolicyMonitor.Data/Repositories/IStatusChangeEventRepository.cs
- [ ] T037 [US1] Implement PolicyRepository with status query methods in src/PolicyMonitor.Data/Repositories/PolicyRepository.cs
- [ ] T038 [US1] Implement StatusChangeEventRepository with pending events query in src/PolicyMonitor.Data/Repositories/StatusChangeEventRepository.cs

### Database Triggers for User Story 1

- [ ] T039 [US1] Create SQL trigger trg_PolicyStatusChange on Policy table in database/triggers/trg_PolicyStatusChange.sql
- [ ] T040 [US1] Create stored procedure sp_InsertStatusChangeEvent in database/stored-procedures/sp_InsertStatusChangeEvent.sql
- [ ] T041 [US1] Create database migration script for trigger installation in database/migrations/001_InstallStatusChangeTrigger.sql

### Background Processing for User Story 1

- [ ] T042 [US1] Create StatusChangeWorker background service in src/PolicyMonitor.Presentation/Workers/StatusChangeWorker.cs
- [ ] T043 [US1] Implement event polling logic with 5-second interval in src/PolicyMonitor.Presentation/Workers/StatusChangeWorker.cs
- [ ] T044 [US1] Add worker registration in Program.cs dependency injection in src/PolicyMonitor.Presentation/Program.cs
- [ ] T045 [US1] Configure WorkerSettings section in appsettings.json with polling interval in src/PolicyMonitor.Presentation/appsettings.json

**Checkpoint**: User Story 1 complete - status changes are detected within 5 seconds and events captured

---

## Phase 4: User Story 2 - Automated Policy Document Generation (Priority: P1) 🎯 MVP

**Goal**: Generate professional PDF documents containing policy information and status change details for each detected change

**Independent Test**: Trigger a status change, verify PDF is generated at staging location with correct filename format and all required policy data

### Contracts for User Story 2

- [ ] T046 [P] [US2] Create IPdfGenerator interface in src/PolicyMonitor.Infrastructure/Pdf/IPdfGenerator.cs
- [ ] T047 [P] [US2] Create PolicyStatusChangeData DTO in src/PolicyMonitor.Infrastructure/Pdf/PolicyStatusChangeData.cs
- [ ] T048 [P] [US2] Create PdfGenerationResult class in src/PolicyMonitor.Infrastructure/Pdf/PdfGenerationResult.cs
- [ ] T049 [P] [US2] Create IPdfGeneratorFactory interface in src/PolicyMonitor.Infrastructure/Factories/IPdfGeneratorFactory.cs

### PDF Implementation for User Story 2

- [ ] T050 [US2] Implement QuestPdfGenerator implementing IPdfGenerator in src/PolicyMonitor.Infrastructure/Pdf/QuestPdfGenerator.cs
- [ ] T051 [US2] Create PDF document template with policy header, status section, change details in src/PolicyMonitor.Infrastructure/Pdf/Templates/PolicyStatusChangeTemplate.cs
- [ ] T052 [US2] Implement PdfGeneratorFactory with QuestPDF creation logic in src/PolicyMonitor.Infrastructure/Factories/PdfGeneratorFactory.cs
- [ ] T053 [US2] Configure PdfSettings section in appsettings.json with output directory and timeout in src/PolicyMonitor.Presentation/appsettings.json

### Data Layer for User Story 2

- [ ] T054 [P] [US2] Create GeneratedDocument entity in src/PolicyMonitor.Domain/Entities/GeneratedDocument.cs
- [ ] T055 [US2] Configure GeneratedDocument entity mapping in PolicyDbContext in src/PolicyMonitor.Data/Context/PolicyDbContext.cs
- [ ] T056 [US2] Create EF Core migration for GeneratedDocument table in src/PolicyMonitor.Data/Migrations/
- [ ] T057 [P] [US2] Create IGeneratedDocumentRepository interface in src/PolicyMonitor.Data/Repositories/IGeneratedDocumentRepository.cs
- [ ] T058 [US2] Implement GeneratedDocumentRepository in src/PolicyMonitor.Data/Repositories/GeneratedDocumentRepository.cs

### Business Logic for User Story 2

- [ ] T059 [P] [US2] Create IStatusChangeProcessor interface in src/PolicyMonitor.Domain/Services/IStatusChangeProcessor.cs
- [ ] T060 [US2] Implement StatusChangeProcessor with PDF generation orchestration in src/PolicyMonitor.Domain/Services/StatusChangeProcessor.cs
- [ ] T061 [US2] Add PDF generation error handling and retry logic using Polly in src/PolicyMonitor.Domain/Services/StatusChangeProcessor.cs
- [ ] T062 [US2] Update StatusChangeWorker to call StatusChangeProcessor in src/PolicyMonitor.Presentation/Workers/StatusChangeWorker.cs
- [ ] T063 [US2] Register PDF services in dependency injection in src/PolicyMonitor.Presentation/Program.cs

**Checkpoint**: User Story 2 complete - PDFs generated automatically for all status changes

---

## Phase 5: User Story 3 - Agent Notification Delivery (Priority: P2)

**Goal**: Send email notifications to assigned insurance agents containing policy details and status change information

**Independent Test**: Trigger a status change, verify assigned agent receives email within 30 seconds with correct policy information and PDF reference

### Contracts for User Story 3

- [ ] T064 [P] [US3] Create INotificationService interface in src/PolicyMonitor.Infrastructure/Notifications/INotificationService.cs
- [ ] T065 [P] [US3] Create NotificationMethod enum in src/PolicyMonitor.Infrastructure/Notifications/NotificationMethod.cs
- [ ] T066 [P] [US3] Create PolicyNotificationData DTO in src/PolicyMonitor.Infrastructure/Notifications/PolicyNotificationData.cs
- [ ] T067 [P] [US3] Create NotificationResult class in src/PolicyMonitor.Infrastructure/Notifications/NotificationResult.cs
- [ ] T068 [P] [US3] Create INotificationServiceFactory interface in src/PolicyMonitor.Infrastructure/Factories/INotificationServiceFactory.cs

### Email Implementation for User Story 3

- [ ] T069 [US3] Implement EmailNotificationService using MailKit in src/PolicyMonitor.Infrastructure/Notifications/EmailNotificationService.cs
- [ ] T070 [US3] Create email template with professional formatting in src/PolicyMonitor.Infrastructure/Notifications/Templates/PolicyStatusChangeEmailTemplate.cs
- [ ] T071 [US3] Implement NotificationServiceFactory with email service creation in src/PolicyMonitor.Infrastructure/Factories/NotificationServiceFactory.cs
- [ ] T072 [US3] Configure SmtpSettings section in appsettings.json with host, port, credentials in src/PolicyMonitor.Presentation/appsettings.json

### Data Layer for User Story 3

- [ ] T073 [P] [US3] Create AgentNotification entity in src/PolicyMonitor.Domain/Entities/AgentNotification.cs
- [ ] T074 [P] [US3] Create NotificationStatus enum in src/PolicyMonitor.Domain/Enums/NotificationStatus.cs
- [ ] T075 [US3] Configure AgentNotification entity mapping in PolicyDbContext in src/PolicyMonitor.Data/Context/PolicyDbContext.cs
- [ ] T076 [US3] Create EF Core migration for AgentNotification table in src/PolicyMonitor.Data/Migrations/
- [ ] T077 [P] [US3] Create IAgentNotificationRepository interface in src/PolicyMonitor.Data/Repositories/IAgentNotificationRepository.cs
- [ ] T078 [US3] Implement AgentNotificationRepository with retry query methods in src/PolicyMonitor.Data/Repositories/AgentNotificationRepository.cs

### Business Logic for User Story 3

- [ ] T079 [US3] Add notification delivery to StatusChangeProcessor workflow in src/PolicyMonitor.Domain/Services/StatusChangeProcessor.cs
- [ ] T080 [US3] Implement notification retry logic with exponential backoff (30s, 2min, 5min) in src/PolicyMonitor.Domain/Services/StatusChangeProcessor.cs
- [ ] T081 [US3] Add PolicyAgent junction table mapping in PolicyDbContext in src/PolicyMonitor.Data/Context/PolicyDbContext.cs
- [ ] T082 [US3] Implement multi-agent notification logic in src/PolicyMonitor.Domain/Services/StatusChangeProcessor.cs
- [ ] T083 [US3] Register notification services in dependency injection in src/PolicyMonitor.Presentation/Program.cs

**Checkpoint**: User Story 3 complete - agents receive email notifications for all status changes

---

## Phase 6: User Story 4 - Automated File Transfer (Priority: P3)

**Goal**: Transfer generated PDF files to designated storage location for centralized access and retention

**Independent Test**: Generate a PDF, verify it transfers to destination path with correct naming and verify file integrity

### Contracts for User Story 4

- [ ] T084 [P] [US4] Create IFileTransferService interface in src/PolicyMonitor.Infrastructure/FileTransfer/IFileTransferService.cs
- [ ] T085 [P] [US4] Create FileTransferResult class in src/PolicyMonitor.Infrastructure/FileTransfer/FileTransferResult.cs

### File Transfer Implementation for User Story 4

- [ ] T086 [US4] Implement FileTransferService with local and UNC path support in src/PolicyMonitor.Infrastructure/FileTransfer/FileTransferService.cs
- [ ] T087 [US4] Implement filename conflict handling with sequence numbers (_001, _002) in src/PolicyMonitor.Infrastructure/FileTransfer/FileTransferService.cs
- [ ] T088 [US4] Implement file verification logic (existence and size check) in src/PolicyMonitor.Infrastructure/FileTransfer/FileTransferService.cs
- [ ] T089 [US4] Configure FileTransferSettings section in appsettings.json with destination path in src/PolicyMonitor.Presentation/appsettings.json

### Data Layer for User Story 4

- [ ] T090 [P] [US4] Create FileTransferRecord entity in src/PolicyMonitor.Domain/Entities/FileTransferRecord.cs
- [ ] T091 [P] [US4] Create TransferStatus enum in src/PolicyMonitor.Domain/Enums/TransferStatus.cs
- [ ] T092 [US4] Configure FileTransferRecord entity mapping in PolicyDbContext in src/PolicyMonitor.Data/Context/PolicyDbContext.cs
- [ ] T093 [US4] Create EF Core migration for FileTransferRecord table in src/PolicyMonitor.Data/Migrations/
- [ ] T094 [P] [US4] Create IFileTransferRepository interface in src/PolicyMonitor.Data/Repositories/IFileTransferRepository.cs
- [ ] T095 [US4] Implement FileTransferRepository in src/PolicyMonitor.Data/Repositories/FileTransferRepository.cs

### Business Logic for User Story 4

- [ ] T096 [US4] Add file transfer to StatusChangeProcessor workflow in src/PolicyMonitor.Domain/Services/StatusChangeProcessor.cs
- [ ] T097 [US4] Implement file transfer retry logic (every 5 minutes for 2 hours) in src/PolicyMonitor.Domain/Services/StatusChangeProcessor.cs
- [ ] T098 [US4] Add file transfer queue processing for failed transfers in src/PolicyMonitor.Presentation/Workers/StatusChangeWorker.cs
- [ ] T099 [US4] Register file transfer services in dependency injection in src/PolicyMonitor.Presentation/Program.cs

**Checkpoint**: User Story 4 complete - PDFs automatically transferred to designated location

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [ ] T100 [P] Create AuditLog entity for 7-year compliance retention in src/PolicyMonitor.Domain/Entities/AuditLog.cs
- [ ] T101 [P] Implement audit logging across all operations in src/PolicyMonitor.Domain/Services/AuditService.cs
- [ ] T102 Add comprehensive logging for all status change operations in src/PolicyMonitor.Domain/Services/StatusChangeProcessor.cs
- [ ] T103 [P] Add performance metrics logging (success rates, average times) in src/PolicyMonitor.Shared/Logging/MetricsLogger.cs
- [ ] T104 [P] Create README.md with setup instructions in repository root
- [ ] T105 [P] Update quickstart.md validation scenarios in specs/001-policy-status-monitor/quickstart.md
- [ ] T106 Add graceful shutdown handling for background worker in src/PolicyMonitor.Presentation/Workers/StatusChangeWorker.cs
- [ ] T107 [P] Add database connection resilience with retry policies in src/PolicyMonitor.Data/Context/PolicyDbContext.cs
- [ ] T108 Implement manual reprocess feature for failed events in src/PolicyMonitor.Domain/Services/StatusChangeProcessor.cs
- [ ] T109 [P] Add health check endpoints for monitoring in src/PolicyMonitor.Presentation/HealthChecks/
- [ ] T110 Code review and refactoring pass across all layers

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Story 1 (Phase 3)**: Depends on Foundational completion - No dependencies on other stories
- **User Story 2 (Phase 4)**: Depends on Foundational completion - Integrates with US1 but independently testable
- **User Story 3 (Phase 5)**: Depends on Foundational completion - Integrates with US1 and US2
- **User Story 4 (Phase 6)**: Depends on Foundational completion - Integrates with US2
- **Polish (Phase 7)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Foundation only - NO dependencies on other stories
- **User Story 2 (P1)**: Foundation only - Uses US1 events but independently testable
- **User Story 3 (P2)**: Foundation only - Uses US1 events and US2 PDFs but independently testable
- **User Story 4 (P3)**: Foundation only - Uses US2 PDFs but independently testable

### Within Each User Story

User Story 1:
- Data layer entities in parallel → Entity configurations → Migration
- Repository interfaces in parallel → Repository implementations
- Trigger scripts can be developed in parallel with repositories
- Background worker depends on repositories

User Story 2:
- Contract interfaces in parallel → Implementations
- Entity and repository in parallel with PDF implementation
- Business logic integration last

User Story 3:
- Contract interfaces in parallel → Email implementation
- Entity and repository in parallel with notification service
- Business logic integration last

User Story 4:
- Contract interfaces → Implementation
- Entity and repository in parallel with file transfer service
- Business logic integration last

### Parallel Opportunities

**Setup Phase:**
- T002-T006: All project initializations in parallel (different folders)
- T007-T011: All NuGet package additions in parallel (different projects)
- T013-T014: Configuration files in parallel

**Foundational Phase:**
- T019-T020: Shared utilities in parallel
- T021-T023: Polly policies in parallel (different files)

**User Story 1:**
- T027-T030: All entities in parallel (different files)
- T031-T033: Entity configurations in parallel (different DbSet registrations)
- T035-T036: Repository interfaces in parallel
- T037-T038: Repository implementations in parallel

**User Story 2:**
- T046-T049: All contract interfaces in parallel
- T054, T057: Entity and repository interface in parallel

**User Story 3:**
- T064-T068: All contract interfaces in parallel
- T073-T074, T077: Entity, enum, and repository interface in parallel

**User Story 4:**
- T084-T085: Contract interfaces in parallel
- T090-T091, T094: Entity, enum, and repository interface in parallel

**Polish Phase:**
- T100-T101, T103-T105, T107, T109: All independent improvements in parallel

---

## Parallel Example: User Story 1

```bash
# Launch all entities together:
Task T027: "Create Policy entity in src/PolicyMonitor.Domain/Entities/Policy.cs"
Task T028: "Create PolicyStatus enumeration in src/PolicyMonitor.Domain/Enums/PolicyStatus.cs"
Task T029: "Create StatusChangeEvent entity in src/PolicyMonitor.Domain/Entities/StatusChangeEvent.cs"
Task T030: "Create Agent entity in src/PolicyMonitor.Domain/Entities/Agent.cs"

# Launch repository interfaces together:
Task T035: "Create IPolicyRepository interface"
Task T036: "Create IStatusChangeEventRepository interface"
```

---

## Implementation Strategy

### MVP First (User Stories 1 + 2 Only)

1. Complete Phase 1: Setup (15 tasks)
2. Complete Phase 2: Foundational (11 tasks) ← CRITICAL BLOCKER
3. Complete Phase 3: User Story 1 (19 tasks) → Change detection working
4. Complete Phase 4: User Story 2 (18 tasks) → PDFs generating
5. **STOP and VALIDATE**: Test end-to-end change detection + PDF generation
6. Deploy/demo if ready (core value delivered)

**MVP delivers**: Automated status change detection and documentation generation - eliminates manual PDF creation for agents.

### Incremental Delivery

1. **Foundation** (Phases 1-2): Project structure + infrastructure ready
2. **MVP** (Phases 3-4): Change detection + PDF generation → Deploy
3. **Enhanced** (Phase 5): Add email notifications → Deploy
4. **Complete** (Phase 6): Add file transfer → Deploy
5. **Production** (Phase 7): Add audit, monitoring, polish → Deploy

Each increment adds value without breaking previous functionality.

### Parallel Team Strategy

With 2-3 developers:

1. **All together**: Complete Phases 1-2 (Setup + Foundation)
2. **Split after foundation**:
   - Developer A: User Story 1 (change detection)
   - Developer B: User Story 2 (PDF generation)
   - Developer C: Foundation polish + prepare US3
3. **Integration**: Merge US1 + US2, test together
4. **Continue parallel**: US3 and US4 in parallel

---

## Task Summary

- **Total Tasks**: 110
- **Phase 1 (Setup)**: 15 tasks
- **Phase 2 (Foundational)**: 11 tasks (BLOCKS all stories)
- **Phase 3 (User Story 1 - P1)**: 19 tasks
- **Phase 4 (User Story 2 - P1)**: 18 tasks
- **Phase 5 (User Story 3 - P2)**: 20 tasks
- **Phase 6 (User Story 4 - P3)**: 16 tasks
- **Phase 7 (Polish)**: 11 tasks

**Parallel Tasks**: 47 tasks marked [P] can run in parallel within their phase
**MVP Scope**: Phases 1-4 = 63 tasks (57% of total) delivers core value

**Suggested MVP**: User Stories 1 + 2 (change detection + PDF generation)
**Full Feature**: All phases = 110 tasks

---

## Notes

- All tasks include specific file paths for clarity
- [P] tasks can run in parallel (different files, no shared dependencies)
- [US#] labels map tasks to user stories for traceability
- Each user story is independently testable and valuable
- Tests not included as not explicitly requested in specification
- Commit after completing each logical group or checkpoint
- Validate at each checkpoint before proceeding to next phase
