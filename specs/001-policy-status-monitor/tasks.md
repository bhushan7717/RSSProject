# Tasks: Insurance Policy Status Monitor

**Input**: Design documents from `/specs/001-policy-status-monitor/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: ✅ **REQUIRED** - SC-011 mandates 95%+ unit test coverage across all modules. Test tasks included following TDD principles.

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

## Phase 1.5: Test Infrastructure Setup (SC-011 Requirement)

**Purpose**: Initialize test projects and frameworks to achieve 95%+ unit test coverage

**⚠️ MANDATORY**: SC-011 requires 95%+ unit test coverage across all modules

- [ ] T016-TEST Initialize PolicyMonitor.Domain.Tests xUnit project (.NET 8.0) in tests/PolicyMonitor.Domain.Tests/
- [ ] T017-TEST [P] Initialize PolicyMonitor.Infrastructure.Tests xUnit project (.NET 8.0) in tests/PolicyMonitor.Infrastructure.Tests/
- [ ] T018-TEST [P] Initialize PolicyMonitor.Data.Tests xUnit project (.NET 8.0) in tests/PolicyMonitor.Data.Tests/
- [ ] T019-TEST [P] Initialize PolicyMonitor.Integration.Tests xUnit project (.NET 8.0) in tests/PolicyMonitor.Integration.Tests/
- [ ] T020-TEST [P] Initialize PolicyMonitor.TestHelpers class library (.NET 8.0) in tests/PolicyMonitor.TestHelpers/
- [ ] T021-TEST Add NuGet packages to all test projects: xUnit, xUnit.runner.visualstudio, coverlet.collector
- [ ] T022-TEST [P] Add NuGet packages to test projects: Moq (mocking framework)
- [ ] T023-TEST [P] Add NuGet packages to test projects: FluentAssertions (assertion library)
- [ ] T024-TEST [P] Add NuGet packages to Integration.Tests: Microsoft.EntityFrameworkCore.InMemory, Testcontainers
- [ ] T025-TEST [P] Create test builders in TestHelpers: PolicyBuilder, StatusChangeEventBuilder in tests/PolicyMonitor.TestHelpers/Builders/
- [ ] T026-TEST [P] Create mock factories in TestHelpers: MockPdfGenerator, MockEmailClient in tests/PolicyMonitor.TestHelpers/Mocks/
- [ ] T027-TEST [P] Create test data fixtures for common scenarios in tests/PolicyMonitor.TestHelpers/Fixtures/
- [ ] T028-TEST Configure code coverage reporting with coverlet in repository root
- [ ] T029-TEST [P] Create GitHub Actions workflow or script for running tests and coverage in .github/workflows/test.yml

**Checkpoint**: Test infrastructure ready - TDD workflow can begin

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [ ] T030 Create base IRepository<TEntity, TKey> interface in src/PolicyMonitor.Data/Repositories/IRepository.cs
- [ ] T031 Create base Repository<TEntity, TKey> implementation in src/PolicyMonitor.Data/Repositories/Repository.cs
- [ ] T032 Create PolicyDbContext with connection string configuration in src/PolicyMonitor.Data/Context/PolicyDbContext.cs
- [ ] T033 [P] Create Serilog configuration helper in src/PolicyMonitor.Shared/Logging/LoggingConfiguration.cs
- [ ] T034 [P] Create configuration extensions for app settings in src/PolicyMonitor.Shared/Configuration/ConfigurationExtensions.cs
- [ ] T035 Setup Polly retry policies for PDF generation in src/PolicyMonitor.Shared/Resilience/PdfRetryPolicy.cs
- [ ] T036 [P] Setup Polly retry policies for email delivery in src/PolicyMonitor.Shared/Resilience/EmailRetryPolicy.cs
- [ ] T037 [P] Setup Polly retry policies for file transfer in src/PolicyMonitor.Shared/Resilience/FileTransferRetryPolicy.cs
- [ ] T038 Create base domain entity class with common properties in src/PolicyMonitor.Domain/Entities/BaseEntity.cs
- [ ] T039 Configure dependency injection container structure in src/PolicyMonitor.Presentation/Program.cs
- [ ] T040 Create custom exception types: PdfGenerationException, NotificationException, FileTransferException in src/PolicyMonitor.Domain/Exceptions/

### Foundational Tests (TDD - Write First)

- [ ] T041-TEST [P] Unit test for Repository<TEntity, TKey> base implementation in tests/PolicyMonitor.Data.Tests/Repositories/RepositoryTests.cs
- [ ] T042-TEST [P] Unit test for Polly retry policies (PDF, Email, FileTransfer) in tests/PolicyMonitor.Shared.Tests/Resilience/
- [ ] T043-TEST [P] Unit test for custom exception types in tests/PolicyMonitor.Domain.Tests/Exceptions/ExceptionTests.cs

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Automatic Policy Status Change Detection (Priority: P1) 🎯 MVP

**Goal**: Detect policy status changes within 5 seconds using SQL Server triggers and capture change events for processing

**Independent Test**: Update a policy status in database and verify StatusChangeEvent is created within 5 seconds with correct previous/new status values

### Unit Tests for User Story 1 (TDD - Write FIRST, Ensure FAIL) ⚠️

- [ ] T044-TEST [P] [US1] Unit test Policy entity validation rules in tests/PolicyMonitor.Domain.Tests/Entities/PolicyTests.cs
- [ ] T045-TEST [P] [US1] Unit test PolicyStatus enum transitions in tests/PolicyMonitor.Domain.Tests/Enums/PolicyStatusTests.cs
- [ ] T046-TEST [P] [US1] Unit test StatusChangeEvent entity in tests/PolicyMonitor.Domain.Tests/Entities/StatusChangeEventTests.cs
- [ ] T047-TEST [P] [US1] Unit test Agent entity validation in tests/PolicyMonitor.Domain.Tests/Entities/AgentTests.cs
- [ ] T048-TEST [P] [US1] Mock IPolicyRepository for service tests in tests/PolicyMonitor.TestHelpers/Mocks/MockPolicyRepository.cs
- [ ] T049-TEST [P] [US1] Mock IStatusChangeEventRepository for service tests in tests/PolicyMonitor.TestHelpers/Mocks/MockStatusChangeEventRepository.cs
- [ ] T050-TEST [US1] Unit test PolicyRepository query methods in tests/PolicyMonitor.Data.Tests/Repositories/PolicyRepositoryTests.cs
- [ ] T051-TEST [US1] Unit test StatusChangeEventRepository pending events query in tests/PolicyMonitor.Data.Tests/Repositories/StatusChangeEventRepositoryTests.cs
- [ ] T052-TEST [US1] Unit test StatusChangeWorker event polling logic in tests/PolicyMonitor.Presentation.Tests/Workers/StatusChangeWorkerTests.cs

**Checkpoint**: Tests written and FAILING - ready for implementation

### Data Layer for User Story 1

- [ ] T053 [P] [US1] Create Policy entity in src/PolicyMonitor.Domain/Entities/Policy.cs
- [ ] T054 [P] [US1] Create PolicyStatus enumeration in src/PolicyMonitor.Domain/Enums/PolicyStatus.cs
- [ ] T055 [P] [US1] Create StatusChangeEvent entity in src/PolicyMonitor.Domain/Entities/StatusChangeEvent.cs
- [ ] T056 [P] [US1] Create Agent entity in src/PolicyMonitor.Domain/Entities/Agent.cs
- [ ] T057 [US1] Configure Policy entity mapping in PolicyDbContext in src/PolicyMonitor.Data/Context/PolicyDbContext.cs
- [ ] T058 [P] [US1] Configure StatusChangeEvent entity mapping in PolicyDbContext in src/PolicyMonitor.Data/Context/PolicyDbContext.cs
- [ ] T059 [P] [US1] Configure Agent entity mapping in PolicyDbContext in src/PolicyMonitor.Data/Context/PolicyDbContext.cs
- [ ] T060 [US1] Create EF Core migration for Policy, StatusChangeEvent, Agent tables in src/PolicyMonitor.Data/Migrations/

### Repository Layer for User Story 1

- [ ] T061 [P] [US1] Create IPolicyRepository interface in src/PolicyMonitor.Data/Repositories/IPolicyRepository.cs
- [ ] T062 [P] [US1] Create IStatusChangeEventRepository interface in src/PolicyMonitor.Data/Repositories/IStatusChangeEventRepository.cs
- [ ] T063 [US1] Implement PolicyRepository with status query methods in src/PolicyMonitor.Data/Repositories/PolicyRepository.cs
- [ ] T064 [US1] Implement StatusChangeEventRepository with pending events query in src/PolicyMonitor.Data/Repositories/StatusChangeEventRepository.cs

### Database Triggers for User Story 1

- [ ] T065 [US1] Create SQL trigger trg_PolicyStatusChange on Policy table in database/triggers/trg_PolicyStatusChange.sql
- [ ] T066 [US1] Create stored procedure sp_InsertStatusChangeEvent in database/stored-procedures/sp_InsertStatusChangeEvent.sql
- [ ] T067 [US1] Create database migration script for trigger installation in database/migrations/001_InstallStatusChangeTrigger.sql

### Background Processing for User Story 1

- [ ] T068 [US1] Create StatusChangeWorker background service in src/PolicyMonitor.Presentation/Workers/StatusChangeWorker.cs
- [ ] T069 [US1] Implement event polling logic with 5-second interval in src/PolicyMonitor.Presentation/Workers/StatusChangeWorker.cs
- [ ] T070 [US1] Add worker registration in Program.cs dependency injection in src/PolicyMonitor.Presentation/Program.cs
- [ ] T071 [US1] Configure WorkerSettings section in appsettings.json with polling interval in src/PolicyMonitor.Presentation/appsettings.json

### Integration Tests for User Story 1

- [ ] T072-TEST [US1] Integration test: Policy status update triggers event creation in tests/PolicyMonitor.Integration.Tests/UserStory1IntegrationTests.cs
- [ ] T073-TEST [US1] Integration test: Multiple simultaneous status changes in tests/PolicyMonitor.Integration.Tests/UserStory1IntegrationTests.cs
- [ ] T074-TEST [US1] Integration test: Rapid sequential status changes in tests/PolicyMonitor.Integration.Tests/UserStory1IntegrationTests.cs
- [ ] T075-TEST [US1] Integration test: Event detection within 5-second SLA in tests/PolicyMonitor.Integration.Tests/UserStory1IntegrationTests.cs

**Checkpoint**: User Story 1 complete - status changes detected within 5 seconds, all tests PASSING ✅

---

## Phase 4: User Story 2 - Automated Policy Document Generation (Priority: P1) 🎯 MVP

**Goal**: Generate professional PDF documents containing policy information and status change details for each detected change

**Independent Test**: Trigger a status change, verify PDF is generated at staging location with correct filename format and all required policy data

### Unit Tests for User Story 2 (TDD - Write FIRST, Ensure FAIL) ⚠️

- [ ] T076-TEST [P] [US2] Unit test IPdfGenerator interface mock implementation in tests/PolicyMonitor.TestHelpers/Mocks/MockPdfGenerator.cs
- [ ] T077-TEST [P] [US2] Unit test PolicyStatusChangeData DTO validation in tests/PolicyMonitor.Infrastructure.Tests/Pdf/PolicyStatusChangeDataTests.cs
- [ ] T078-TEST [P] [US2] Unit test PdfGenerationResult class in tests/PolicyMonitor.Infrastructure.Tests/Pdf/PdfGenerationResultTests.cs
- [ ] T079-TEST [US2] Unit test QuestPdfGenerator PDF generation logic in tests/PolicyMonitor.Infrastructure.Tests/Pdf/QuestPdfGeneratorTests.cs
- [ ] T080-TEST [US2] Unit test QuestPdfGenerator special character handling in tests/PolicyMonitor.Infrastructure.Tests/Pdf/QuestPdfGeneratorTests.cs
- [ ] T081-TEST [US2] Unit test QuestPdfGenerator error handling and retries in tests/PolicyMonitor.Infrastructure.Tests/Pdf/QuestPdfGeneratorTests.cs
- [ ] T082-TEST [US2] Unit test PdfGeneratorFactory creation logic in tests/PolicyMonitor.Infrastructure.Tests/Factories/PdfGeneratorFactoryTests.cs
- [ ] T083-TEST [US2] Unit test GeneratedDocument entity in tests/PolicyMonitor.Domain.Tests/Entities/GeneratedDocumentTests.cs
- [ ] T084-TEST [US2] Unit test GeneratedDocumentRepository in tests/PolicyMonitor.Data.Tests/Repositories/GeneratedDocumentRepositoryTests.cs
- [ ] T085-TEST [US2] Unit test StatusChangeProcessor PDF generation orchestration in tests/PolicyMonitor.Domain.Tests/Services/StatusChangeProcessorTests.cs
- [ ] T086-TEST [US2] Unit test StatusChangeProcessor PDF retry logic with Polly in tests/PolicyMonitor.Domain.Tests/Services/StatusChangeProcessorTests.cs

**Checkpoint**: Tests written and FAILING - ready for implementation

### Contracts for User Story 2

- [ ] T087 [P] [US2] Create IPdfGenerator interface in src/PolicyMonitor.Infrastructure/Pdf/IPdfGenerator.cs
- [ ] T088 [P] [US2] Create PolicyStatusChangeData DTO in src/PolicyMonitor.Infrastructure/Pdf/PolicyStatusChangeData.cs
- [ ] T089 [P] [US2] Create PdfGenerationResult class in src/PolicyMonitor.Infrastructure/Pdf/PdfGenerationResult.cs
- [ ] T090 [P] [US2] Create IPdfGeneratorFactory interface in src/PolicyMonitor.Infrastructure/Factories/IPdfGeneratorFactory.cs

### PDF Implementation for User Story 2

- [ ] T091 [US2] Implement QuestPdfGenerator implementing IPdfGenerator in src/PolicyMonitor.Infrastructure/Pdf/QuestPdfGenerator.cs
- [ ] T092 [US2] Create PDF document template with policy header, status section, change details in src/PolicyMonitor.Infrastructure/Pdf/Templates/PolicyStatusChangeTemplate.cs
- [ ] T093 [US2] Implement PdfGeneratorFactory with QuestPDF creation logic in src/PolicyMonitor.Infrastructure/Factories/PdfGeneratorFactory.cs
- [ ] T094 [US2] Configure PdfSettings section in appsettings.json with output directory and timeout in src/PolicyMonitor.Presentation/appsettings.json

### Data Layer for User Story 2

- [ ] T095 [P] [US2] Create GeneratedDocument entity in src/PolicyMonitor.Domain/Entities/GeneratedDocument.cs
- [ ] T096 [US2] Configure GeneratedDocument entity mapping in PolicyDbContext in src/PolicyMonitor.Data/Context/PolicyDbContext.cs
- [ ] T097 [US2] Create EF Core migration for GeneratedDocument table in src/PolicyMonitor.Data/Migrations/
- [ ] T098 [P] [US2] Create IGeneratedDocumentRepository interface in src/PolicyMonitor.Data/Repositories/IGeneratedDocumentRepository.cs
- [ ] T099 [US2] Implement GeneratedDocumentRepository in src/PolicyMonitor.Data/Repositories/GeneratedDocumentRepository.cs

### Business Logic for User Story 2

- [ ] T100 [P] [US2] Create IStatusChangeProcessor interface in src/PolicyMonitor.Domain/Services/IStatusChangeProcessor.cs
- [ ] T101 [US2] Implement StatusChangeProcessor with PDF generation orchestration in src/PolicyMonitor.Domain/Services/StatusChangeProcessor.cs
- [ ] T102 [US2] Add PDF generation error handling and retry logic using Polly in src/PolicyMonitor.Domain/Services/StatusChangeProcessor.cs
- [ ] T103 [US2] Update StatusChangeWorker to call StatusChangeProcessor in src/PolicyMonitor.Presentation/Workers/StatusChangeWorker.cs
- [ ] T104 [US2] Register PDF services in dependency injection in src/PolicyMonitor.Presentation/Program.cs

### Integration Tests for User Story 2

- [ ] T105-TEST [US2] Integration test: End-to-end PDF generation from status change in tests/PolicyMonitor.Integration.Tests/UserStory2IntegrationTests.cs
- [ ] T106-TEST [US2] Integration test: PDF filename format validation in tests/PolicyMonitor.Integration.Tests/UserStory2IntegrationTests.cs
- [ ] T107-TEST [US2] Integration test: PDF content validation (all required fields) in tests/PolicyMonitor.Integration.Tests/UserStory2IntegrationTests.cs
- [ ] T108-TEST [US2] Integration test: Concurrent PDF generation for multiple policies in tests/PolicyMonitor.Integration.Tests/UserStory2IntegrationTests.cs
- [ ] T109-TEST [US2] Integration test: PDF generation retry on failure in tests/PolicyMonitor.Integration.Tests/UserStory2IntegrationTests.cs

**Checkpoint**: User Story 2 complete - PDFs generated automatically for all status changes, all tests PASSING ✅

---

## Phase 5: User Story 3 - Agent Notification Delivery (Priority: P2)

**Goal**: Send email notifications to assigned insurance agents containing policy details and status change information

**Independent Test**: Trigger a status change, verify assigned agent receives email within 30 seconds with correct policy information and PDF reference

### Unit Tests for User Story 3 (TDD - Write FIRST, Ensure FAIL) ⚠️

- [ ] T110-TEST [P] [US3] Unit test INotificationService interface mock in tests/PolicyMonitor.TestHelpers/Mocks/MockNotificationService.cs
- [ ] T111-TEST [P] [US3] Unit test PolicyNotificationData DTO validation in tests/PolicyMonitor.Infrastructure.Tests/Notifications/PolicyNotificationDataTests.cs
- [ ] T112-TEST [P] [US3] Unit test NotificationResult class in tests/PolicyMonitor.Infrastructure.Tests/Notifications/NotificationResultTests.cs
- [ ] T113-TEST [US3] Unit test EmailNotificationService email delivery logic in tests/PolicyMonitor.Infrastructure.Tests/Notifications/EmailNotificationServiceTests.cs
- [ ] T114-TEST [US3] Unit test EmailNotificationService SMTP connectivity in tests/PolicyMonitor.Infrastructure.Tests/Notifications/EmailNotificationServiceTests.cs
- [ ] T115-TEST [US3] Unit test EmailNotificationService template rendering in tests/PolicyMonitor.Infrastructure.Tests/Notifications/EmailNotificationServiceTests.cs
- [ ] T116-TEST [US3] Unit test NotificationServiceFactory creation logic in tests/PolicyMonitor.Infrastructure.Tests/Factories/NotificationServiceFactoryTests.cs
- [ ] T117-TEST [US3] Unit test AgentNotification entity in tests/PolicyMonitor.Domain.Tests/Entities/AgentNotificationTests.cs
- [ ] T118-TEST [US3] Unit test AgentNotificationRepository retry queries in tests/PolicyMonitor.Data.Tests/Repositories/AgentNotificationRepositoryTests.cs
- [ ] T119-TEST [US3] Unit test StatusChangeProcessor notification delivery in tests/PolicyMonitor.Domain.Tests/Services/StatusChangeProcessorTests.cs
- [ ] T120-TEST [US3] Unit test StatusChangeProcessor notification retry logic with exponential backoff in tests/PolicyMonitor.Domain.Tests/Services/StatusChangeProcessorTests.cs
- [ ] T121-TEST [US3] Unit test StatusChangeProcessor multi-agent notification in tests/PolicyMonitor.Domain.Tests/Services/StatusChangeProcessorTests.cs

**Checkpoint**: Tests written and FAILING - ready for implementation

### Contracts for User Story 3

- [ ] T122 [P] [US3] Create INotificationService interface in src/PolicyMonitor.Infrastructure/Notifications/INotificationService.cs
- [ ] T123 [P] [US3] Create NotificationMethod enum in src/PolicyMonitor.Infrastructure/Notifications/NotificationMethod.cs
- [ ] T124 [P] [US3] Create PolicyNotificationData DTO in src/PolicyMonitor.Infrastructure/Notifications/PolicyNotificationData.cs
- [ ] T125 [P] [US3] Create NotificationResult class in src/PolicyMonitor.Infrastructure/Notifications/NotificationResult.cs
- [ ] T126 [P] [US3] Create INotificationServiceFactory interface in src/PolicyMonitor.Infrastructure/Factories/INotificationServiceFactory.cs

### Email Implementation for User Story 3

- [ ] T127 [US3] Implement EmailNotificationService using MailKit in src/PolicyMonitor.Infrastructure/Notifications/EmailNotificationService.cs
- [ ] T128 [US3] Create email template with professional formatting in src/PolicyMonitor.Infrastructure/Notifications/Templates/PolicyStatusChangeEmailTemplate.cs
- [ ] T129 [US3] Implement NotificationServiceFactory with email service creation in src/PolicyMonitor.Infrastructure/Factories/NotificationServiceFactory.cs
- [ ] T130 [US3] Configure SmtpSettings section in appsettings.json with host, port, credentials in src/PolicyMonitor.Presentation/appsettings.json

### Data Layer for User Story 3

- [ ] T131 [P] [US3] Create AgentNotification entity in src/PolicyMonitor.Domain/Entities/AgentNotification.cs
- [ ] T132 [P] [US3] Create NotificationStatus enum in src/PolicyMonitor.Domain/Enums/NotificationStatus.cs
- [ ] T133 [US3] Configure AgentNotification entity mapping in PolicyDbContext in src/PolicyMonitor.Data/Context/PolicyDbContext.cs
- [ ] T134 [US3] Create EF Core migration for AgentNotification table in src/PolicyMonitor.Data/Migrations/
- [ ] T135 [P] [US3] Create IAgentNotificationRepository interface in src/PolicyMonitor.Data/Repositories/IAgentNotificationRepository.cs
- [ ] T136 [US3] Implement AgentNotificationRepository with retry query methods in src/PolicyMonitor.Data/Repositories/AgentNotificationRepository.cs

### Business Logic for User Story 3

- [ ] T137 [US3] Add notification delivery to StatusChangeProcessor workflow in src/PolicyMonitor.Domain/Services/StatusChangeProcessor.cs
- [ ] T138 [US3] Implement notification retry logic with exponential backoff (30s, 2min, 5min) in src/PolicyMonitor.Domain/Services/StatusChangeProcessor.cs
- [ ] T139 [US3] Add PolicyAgent junction table mapping in PolicyDbContext in src/PolicyMonitor.Data/Context/PolicyDbContext.cs
- [ ] T140 [US3] Implement multi-agent notification logic in src/PolicyMonitor.Domain/Services/StatusChangeProcessor.cs
- [ ] T141 [US3] Register notification services in dependency injection in src/PolicyMonitor.Presentation/Program.cs

### Integration Tests for User Story 3

- [ ] T142-TEST [US3] Integration test: End-to-end notification delivery from status change in tests/PolicyMonitor.Integration.Tests/UserStory3IntegrationTests.cs
- [ ] T143-TEST [US3] Integration test: Notification delivery within 30-second SLA in tests/PolicyMonitor.Integration.Tests/UserStory3IntegrationTests.cs
- [ ] T144-TEST [US3] Integration test: Notification retry with exponential backoff in tests/PolicyMonitor.Integration.Tests/UserStory3IntegrationTests.cs
- [ ] T145-TEST [US3] Integration test: Multi-agent notification for single policy in tests/PolicyMonitor.Integration.Tests/UserStory3IntegrationTests.cs

**Checkpoint**: User Story 3 complete - agents receive email notifications for all status changes, all tests PASSING ✅

---

## Phase 6: User Story 4 - Automated File Transfer (Priority: P3)

**Goal**: Transfer generated PDF files to designated storage location for centralized access and retention

**Independent Test**: Generate a PDF, verify it transfers to destination path with correct naming and verify file integrity

### Unit Tests for User Story 4 (TDD - Write FIRST, Ensure FAIL) ⚠️

- [ ] T146-TEST [P] [US4] Unit test IFileTransferService interface mock in tests/PolicyMonitor.TestHelpers/Mocks/MockFileTransferService.cs
- [ ] T147-TEST [P] [US4] Unit test FileTransferResult class in tests/PolicyMonitor.Infrastructure.Tests/FileTransfer/FileTransferResultTests.cs
- [ ] T148-TEST [US4] Unit test FileTransferService local path transfers in tests/PolicyMonitor.Infrastructure.Tests/FileTransfer/FileTransferServiceTests.cs
- [ ] T149-TEST [US4] Unit test FileTransferService UNC path support in tests/PolicyMonitor.Infrastructure.Tests/FileTransfer/FileTransferServiceTests.cs
- [ ] T150-TEST [US4] Unit test FileTransferService filename conflict resolution in tests/PolicyMonitor.Infrastructure.Tests/FileTransfer/FileTransferServiceTests.cs
- [ ] T151-TEST [US4] Unit test FileTransferService file verification logic in tests/PolicyMonitor.Infrastructure.Tests/FileTransfer/FileTransferServiceTests.cs
- [ ] T152-TEST [US4] Unit test FileTransferRecord entity in tests/PolicyMonitor.Domain.Tests/Entities/FileTransferRecordTests.cs
- [ ] T153-TEST [US4] Unit test FileTransferRepository in tests/PolicyMonitor.Data.Tests/Repositories/FileTransferRepositoryTests.cs
- [ ] T154-TEST [US4] Unit test StatusChangeProcessor file transfer integration in tests/PolicyMonitor.Domain.Tests/Services/StatusChangeProcessorTests.cs
- [ ] T155-TEST [US4] Unit test StatusChangeProcessor file transfer retry logic in tests/PolicyMonitor.Domain.Tests/Services/StatusChangeProcessorTests.cs

**Checkpoint**: Tests written and FAILING - ready for implementation

### Contracts for User Story 4

- [ ] T156 [P] [US4] Create IFileTransferService interface in src/PolicyMonitor.Infrastructure/FileTransfer/IFileTransferService.cs
- [ ] T157 [P] [US4] Create FileTransferResult class in src/PolicyMonitor.Infrastructure/FileTransfer/FileTransferResult.cs

### File Transfer Implementation for User Story 4

- [ ] T158 [US4] Implement FileTransferService with local and UNC path support in src/PolicyMonitor.Infrastructure/FileTransfer/FileTransferService.cs
- [ ] T159 [US4] Implement filename conflict handling with sequence numbers (_001, _002) in src/PolicyMonitor.Infrastructure/FileTransfer/FileTransferService.cs
- [ ] T160 [US4] Implement file verification logic (existence and size check) in src/PolicyMonitor.Infrastructure/FileTransfer/FileTransferService.cs
- [ ] T161 [US4] Configure FileTransferSettings section in appsettings.json with destination path in src/PolicyMonitor.Presentation/appsettings.json

### Data Layer for User Story 4

- [ ] T162 [P] [US4] Create FileTransferRecord entity in src/PolicyMonitor.Domain/Entities/FileTransferRecord.cs
- [ ] T163 [P] [US4] Create TransferStatus enum in src/PolicyMonitor.Domain/Enums/TransferStatus.cs
- [ ] T164 [US4] Configure FileTransferRecord entity mapping in PolicyDbContext in src/PolicyMonitor.Data/Context/PolicyDbContext.cs
- [ ] T165 [US4] Create EF Core migration for FileTransferRecord table in src/PolicyMonitor.Data/Migrations/
- [ ] T166 [P] [US4] Create IFileTransferRepository interface in src/PolicyMonitor.Data/Repositories/IFileTransferRepository.cs
- [ ] T167 [US4] Implement FileTransferRepository in src/PolicyMonitor.Data/Repositories/FileTransferRepository.cs

### Business Logic for User Story 4

- [ ] T168 [US4] Add file transfer to StatusChangeProcessor workflow in src/PolicyMonitor.Domain/Services/StatusChangeProcessor.cs
- [ ] T169 [US4] Implement file transfer retry logic (every 5 minutes for 2 hours) in src/PolicyMonitor.Domain/Services/StatusChangeProcessor.cs
- [ ] T170 [US4] Add file transfer queue processing for failed transfers in src/PolicyMonitor.Presentation/Workers/StatusChangeWorker.cs
- [ ] T171 [US4] Register file transfer services in dependency injection in src/PolicyMonitor.Presentation/Program.cs

### Integration Tests for User Story 4

- [ ] T172-TEST [US4] Integration test: End-to-end file transfer from PDF generation in tests/PolicyMonitor.Integration.Tests/UserStory4IntegrationTests.cs
- [ ] T173-TEST [US4] Integration test: File transfer within 10-second SLA in tests/PolicyMonitor.Integration.Tests/UserStory4IntegrationTests.cs
- [ ] T174-TEST [US4] Integration test: Filename conflict resolution with sequence numbers in tests/PolicyMonitor.Integration.Tests/UserStory4IntegrationTests.cs
- [ ] T175-TEST [US4] Integration test: File verification after transfer in tests/PolicyMonitor.Integration.Tests/UserStory4IntegrationTests.cs
- [ ] T176-TEST [US4] Integration test: File transfer retry on destination unavailable in tests/PolicyMonitor.Integration.Tests/UserStory4IntegrationTests.cs

**Checkpoint**: User Story 4 complete - PDFs automatically transferred to designated location, all tests PASSING ✅

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [ ] T177 [P] Create AuditLog entity for 7-year compliance retention in src/PolicyMonitor.Domain/Entities/AuditLog.cs
- [ ] T178 [P] Implement audit logging across all operations in src/PolicyMonitor.Domain/Services/AuditService.cs
- [ ] T179 Add comprehensive logging for all status change operations in src/PolicyMonitor.Domain/Services/StatusChangeProcessor.cs
- [ ] T180 [P] Add performance metrics logging (success rates, average times) in src/PolicyMonitor.Shared/Logging/MetricsLogger.cs
- [ ] T181 [P] Create README.md with setup instructions in repository root
- [ ] T182 [P] Update quickstart.md validation scenarios in specs/001-policy-status-monitor/quickstart.md
- [ ] T183 Add graceful shutdown handling for background worker in src/PolicyMonitor.Presentation/Workers/StatusChangeWorker.cs
- [ ] T184 [P] Add database connection resilience with retry policies in src/PolicyMonitor.Data/Context/PolicyDbContext.cs
- [ ] T185 Implement manual reprocess feature for failed events (FR-035) in src/PolicyMonitor.Domain/Services/StatusChangeProcessor.cs
- [ ] T186 [P] Add health check endpoints for monitoring in src/PolicyMonitor.Presentation/HealthChecks/
- [ ] T187 Add maintenance window configuration and automatic worker pause/resume (Edge Case #8) in src/PolicyMonitor.Presentation/Workers/StatusChangeWorker.cs
- [ ] T188 Code review and refactoring pass across all layers

### Test Coverage Validation (SC-011 Requirement)

- [ ] T189-TEST Run code coverage analysis across all test projects in CI/CD pipeline
- [ ] T190-TEST Verify 95%+ coverage threshold met for Domain layer in coverage report
- [ ] T191-TEST Verify 95%+ coverage threshold met for Infrastructure layer in coverage report
- [ ] T192-TEST Verify 95%+ coverage threshold met for Data layer in coverage report
- [ ] T193-TEST Configure coverage gates in CI/CD to fail builds below 95% threshold
- [ ] T194-TEST Generate and publish coverage badge/report in README.md

**Checkpoint**: All user stories complete, 95%+ test coverage achieved, production-ready ✅

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

**Test Infrastructure Phase:**
- T016-TEST to T020-TEST: All test projects in parallel
- T022-TEST to T024-TEST: NuGet packages to test projects in parallel
- T025-TEST to T027-TEST: Test builders, mocks, fixtures in parallel

**Foundational Phase:**
- T033-T034: Shared utilities in parallel
- T035-T037: Polly policies in parallel (different files)
- T041-TEST to T043-TEST: Foundation unit tests in parallel

**User Story 1:**
- T044-TEST to T047-TEST: All entity unit tests in parallel (TDD - write FIRST)
- T048-TEST, T049-TEST: Repository mocks in parallel
- T053-T056: All entities in parallel (after tests written)
- T057-T059: Entity configurations in parallel
- T061-T062: Repository interfaces in parallel
- T063-T064: Repository implementations in parallel
- T072-TEST to T075-TEST: All integration tests in parallel

**User Story 2:**
- T076-TEST to T078-TEST: Contract DTOs unit tests in parallel
- T079-TEST to T081-TEST: PDF generator tests in parallel
- T087-T090: Contract interfaces in parallel
- T095, T098: Entity and repository interface in parallel
- T105-TEST to T109-TEST: All integration tests in parallel

**User Story 3:**
- T110-TEST to T112-TEST: Notification contract tests in parallel
- T113-TEST to T115-TEST: Email service tests in parallel
- T122-T126: All contract interfaces in parallel
- T131-T132, T135: Entity, enum, repository interface in parallel
- T142-TEST to T145-TEST: All integration tests in parallel

**User Story 4:**
- T146-TEST, T147-TEST: File transfer contract tests in parallel
- T148-TEST to T151-TEST: File transfer service tests in parallel
- T156-T157: Contract interfaces in parallel
- T162-T163, T166: Entity, enum, repository interface in parallel
- T172-TEST to T176-TEST: All integration tests in parallel

**Polish Phase:**
- T177-T178, T180-T182, T184, T186: All independent improvements in parallel
- T189-TEST to T194-TEST: All coverage validation tasks in parallel

---

## Parallel Example: User Story 1 (TDD Workflow)

```bash
# STEP 1: Write all unit tests FIRST (ensure they FAIL):
Task T044-TEST: "Unit test Policy entity validation rules"
Task T045-TEST: "Unit test PolicyStatus enum transitions"
Task T046-TEST: "Unit test StatusChangeEvent entity"
Task T047-TEST: "Unit test Agent entity validation"

# STEP 2: Launch all entities together:
Task T053: "Create Policy entity in src/PolicyMonitor.Domain/Entities/Policy.cs"
Task T054: "Create PolicyStatus enumeration in src/PolicyMonitor.Domain/Enums/PolicyStatus.cs"
Task T055: "Create StatusChangeEvent entity in src/PolicyMonitor.Domain/Entities/StatusChangeEvent.cs"
Task T056: "Create Agent entity in src/PolicyMonitor.Domain/Entities/Agent.cs"

# STEP 3: Run tests - they should now PASS ✅

# STEP 4: Launch repository tests and implementations in parallel:
Task T050-TEST: "Unit test PolicyRepository query methods"
Task T051-TEST: "Unit test StatusChangeEventRepository"
Task T063: "Implement PolicyRepository"
Task T064: "Implement StatusChangeEventRepository"
```

---

## Implementation Strategy

### TDD Workflow (Required by SC-011)

For each user story:
1. **Write unit tests FIRST** → Ensure they FAIL (red)
2. **Implement code** → Make tests PASS (green)
3. **Refactor** → Improve code quality while keeping tests green
4. **Integration tests** → Verify end-to-end functionality
5. **Coverage validation** → Ensure 95%+ threshold met

### MVP First (User Stories 1 + 2 Only)

1. Complete Phase 1: Setup (15 tasks)
2. Complete Phase 1.5: Test Infrastructure (14 tasks) ← Enables TDD workflow
3. Complete Phase 2: Foundational (14 tasks: 11 impl + 3 test) ← CRITICAL BLOCKER
4. Complete Phase 3: User Story 1 (32 tasks: 9 unit + 19 impl + 4 integration) → Change detection working with tests
5. Complete Phase 4: User Story 2 (34 tasks: 11 unit + 18 impl + 5 integration) → PDFs generating with tests
6. **STOP and VALIDATE**: Test end-to-end, verify coverage ≥95%
7. Deploy/demo if ready (core value delivered)

**MVP delivers**: Automated status change detection and documentation generation with 95%+ test coverage - production-ready system.

### Incremental Delivery (Test-First)

1. **Foundation** (Phases 1 + 1.5 + 2): 43 tasks → Project structure + test infrastructure + foundation ready
2. **MVP** (Phases 3-4): +66 tasks = 109 total → Change detection + PDF generation with 95%+ coverage → Deploy
3. **Enhanced** (Phase 5): +36 tasks = 145 total → Add email notifications with tests → Deploy
4. **Complete** (Phase 6): +31 tasks = 176 total → Add file transfer with tests → Deploy
5. **Production** (Phase 7): +18 tasks = 194 total → Add audit, monitoring, coverage validation → Deploy

Each increment adds value without breaking previous functionality, with tests ensuring quality.

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

- **Total Tasks**: 194 (110 implementation + 84 test)
- **Phase 1 (Setup)**: 15 tasks
- **Phase 1.5 (Test Infrastructure)**: 14 tasks (test frameworks, builders, mocks)
- **Phase 2 (Foundational)**: 14 tasks (11 impl + 3 test) - BLOCKS all stories
- **Phase 3 (User Story 1 - P1)**: 32 tasks (9 unit + 19 impl + 4 integration)
- **Phase 4 (User Story 2 - P1)**: 34 tasks (11 unit + 18 impl + 5 integration)
- **Phase 5 (User Story 3 - P2)**: 36 tasks (12 unit + 20 impl + 4 integration)
- **Phase 6 (User Story 4 - P3)**: 31 tasks (10 unit + 16 impl + 5 integration)
- **Phase 7 (Polish)**: 18 tasks (12 impl + 6 coverage validation)

**Test Tasks**: 84 total (43% of all tasks)
- Unit tests: 45 tasks
- Integration tests: 18 tasks  
- Test infrastructure: 14 tasks
- Coverage validation: 6 tasks
- Foundation tests: 3 tasks

**Parallel Tasks**: ~65 tasks marked [P] can run in parallel within their phase
**MVP Scope**: Phases 1-4 = 109 tasks (56% of total) delivers core value with 95%+ coverage

**Suggested MVP**: User Stories 1 + 2 (change detection + PDF generation) with complete test coverage
**Full Feature**: All phases = 194 tasks

**Test Coverage Commitment**: SC-011 requires 95%+ coverage - **84 test tasks (43%)** ensure this target is met

---

## Notes

- All tasks include specific file paths for clarity
- [P] tasks can run in parallel (different files, no shared dependencies)
- [US#] labels map tasks to user stories for traceability
- Each user story is independently testable and valuable
- **Tests ARE INCLUDED** - SC-011 mandates 95%+ coverage (84 test tasks)
- **TDD workflow enforced**: Write tests FIRST, ensure they FAIL, then implement
- Commit after completing each logical group or checkpoint
- Validate at each checkpoint before proceeding to next phase
- Run coverage reports regularly to ensure 95%+ threshold maintained
