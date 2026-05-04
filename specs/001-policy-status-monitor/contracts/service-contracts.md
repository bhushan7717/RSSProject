# Service Contracts: Insurance Policy Status Monitor

**Feature**: Insurance Policy Status Monitor  
**Purpose**: Define public interfaces for all services implementing Factory and Repository patterns  
**Version**: 1.0  
**Date**: May 4, 2026

## Overview

This document specifies the service contracts (interfaces) for the Insurance Policy Status Monitor system. These contracts enable:
- **Abstraction**: Business logic decoupled from infrastructure implementations
- **Testability**: All dependencies mockable for 95%+ test coverage
- **Swappability**: Factory pattern enables switching PDF/notification libraries
- **Dependency Inversion**: Core domain depends on abstractions, not concrete implementations

## Contract Organization

```
Infrastructure Contracts (Factory Pattern Outputs)
├── IPdfGenerator              - PDF generation abstraction
├── INotificationService       - Notification delivery abstraction
└── IFileTransferService       - File operations abstraction

Data Access Contracts (Repository Pattern)
├── IPolicyRepository          - Policy data access
├── IStatusChangeEventRepository - Event tracking
├── IGeneratedDocumentRepository - Document management
├── IAgentNotificationRepository - Notification tracking
└── IFileTransferRepository    - Transfer records

Factory Contracts
├── IPdfGeneratorFactory       - Creates PDF generators
└── INotificationServiceFactory - Creates notification services

Business Logic Contracts
├── IStatusChangeProcessor     - Core workflow orchestration
└── IPolicyStatusService       - Business operations
```

---

## 1. Infrastructure Contracts

### 1.1 IPdfGenerator

**Purpose**: Abstract PDF generation to enable library switching (currently QuestPDF).

**Location**: `PolicyMonitor.Infrastructure/Pdf/IPdfGenerator.cs`

```csharp
namespace PolicyMonitor.Infrastructure.Pdf
{
    /// <summary>
    /// Defines contract for generating PDF documents for policy status changes.
    /// Implementations: QuestPdfGenerator (v1), potential future: iText7Generator, PdfSharpGenerator
    /// </summary>
    public interface IPdfGenerator
    {
        /// <summary>
        /// Generates a PDF document for a policy status change notification.
        /// </summary>
        /// <param name="policyData">Policy and status change information</param>
        /// <param name="outputPath">Full path where PDF should be saved</param>
        /// <param name="cancellationToken">Cancellation token for async operation</param>
        /// <returns>PdfGenerationResult containing success status and metadata</returns>
        /// <exception cref="PdfGenerationException">Thrown when generation fails</exception>
        Task<PdfGenerationResult> GeneratePolicyStatusChangePdfAsync(
            PolicyStatusChangeData policyData,
            string outputPath,
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Validates that the generator can create PDFs (licenses, dependencies available).
        /// </summary>
        /// <returns>True if generator is operational, false otherwise</returns>
        Task<bool> ValidateAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets the supported PDF format version (e.g., "PDF/A-1b", "PDF 1.7").
        /// </summary>
        string SupportedPdfVersion { get; }
        
        /// <summary>
        /// Gets the generator implementation name for logging/diagnostics.
        /// </summary>
        string GeneratorName { get; }
    }
    
    /// <summary>
    /// Data transfer object containing policy and status change information for PDF generation.
    /// </summary>
    public class PolicyStatusChangeData
    {
        public string PolicyNumber { get; init; } = string.Empty;
        public string PolicyholderName { get; init; } = string.Empty;
        public string PolicyType { get; init; } = string.Empty;
        public decimal CoverageAmount { get; init; }
        public DateTime EffectiveDate { get; init; }
        public DateTime ExpirationDate { get; init; }
        public string PreviousStatus { get; init; } = string.Empty;
        public string NewStatus { get; init; } = string.Empty;
        public DateTime ChangeTimestamp { get; init; }
        public string ChangedByUser { get; init; } = string.Empty;
        public string AgentName { get; init; } = string.Empty;
        public string? AdditionalNotes { get; init; }
    }
    
    /// <summary>
    /// Result of PDF generation operation.
    /// </summary>
    public class PdfGenerationResult
    {
        public bool Success { get; init; }
        public string FilePath { get; init; } = string.Empty;
        public long FileSizeBytes { get; init; }
        public TimeSpan GenerationDuration { get; init; }
        public string? ErrorMessage { get; init; }
        public Exception? Exception { get; init; }
    }
    
    /// <summary>
    /// Exception thrown when PDF generation fails.
    /// </summary>
    public class PdfGenerationException : Exception
    {
        public string? PolicyNumber { get; init; }
        public PdfGenerationException(string message) : base(message) { }
        public PdfGenerationException(string message, Exception inner) : base(message, inner) { }
    }
}
```

**Contract Guarantees:**
- ✅ Thread-safe concurrent generation
- ✅ Async/await throughout
- ✅ Timeout: Max 30 seconds per PDF (enforced by caller)
- ✅ Unicode support for special characters
- ✅ Professional formatting suitable for compliance
- ✅ Generates file atomically (no partial files on failure)

**Test Requirements:**
- Mock this interface for business logic unit tests
- Create in-memory test implementation for integration tests
- Real implementation tested in infrastructure layer tests

---

### 1.2 INotificationService

**Purpose**: Abstract notification delivery to enable service switching (currently MailKit).

**Location**: `PolicyMonitor.Infrastructure/Notifications/INotificationService.cs`

```csharp
namespace PolicyMonitor.Infrastructure.Notifications
{
    /// <summary>
    /// Defines contract for sending notifications to insurance agents.
    /// Implementations: EmailNotificationService (MailKit - v1), future: SmsNotificationService, PushNotificationService
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Sends a policy status change notification to an agent.
        /// </summary>
        /// <param name="notification">Notification details</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>NotificationResult with delivery status</returns>
        Task<NotificationResult> SendNotificationAsync(
            PolicyNotificationData notification,
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Tests connectivity to the notification service (SMTP server, API endpoint, etc.).
        /// </summary>
        /// <returns>True if service is reachable and operational</returns>
        Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets the notification method type (Email, SMS, Push, etc.).
        /// </summary>
        NotificationMethod Method { get; }
        
        /// <summary>
        /// Gets the service implementation name for logging.
        /// </summary>
        string ServiceName { get; }
    }
    
    /// <summary>
    /// Notification method types.
    /// </summary>
    public enum NotificationMethod
    {
        Email,
        SMS,      // Future
        Push      // Future
    }
    
    /// <summary>
    /// Data transfer object for policy status change notifications.
    /// </summary>
    public class PolicyNotificationData
    {
        public string PolicyNumber { get; init; } = string.Empty;
        public string PolicyholderName { get; init; } = string.Empty;
        public string PreviousStatus { get; init; } = string.Empty;
        public string NewStatus { get; init; } = string.Empty;
        public DateTime ChangeTimestamp { get; init; }
        public string AgentName { get; init; } = string.Empty;
        public string AgentEmail { get; init; } = string.Empty;
        public string? PdfFilePath { get; init; }
        public string? AdditionalMessage { get; init; }
    }
    
    /// <summary>
    /// Result of notification delivery operation.
    /// </summary>
    public class NotificationResult
    {
        public bool Success { get; init; }
        public NotificationStatus Status { get; init; }
        public DateTime SentTimestamp { get; init; }
        public TimeSpan DeliveryDuration { get; init; }
        public string? ErrorMessage { get; init; }
        public Exception? Exception { get; init; }
        public string? MessageId { get; init; }  // For tracking (email message ID, SMS ID, etc.)
    }
    
    /// <summary>
    /// Notification delivery status.
    /// </summary>
    public enum NotificationStatus
    {
        Pending,
        Sent,
        Delivered,
        Failed,
        Queued
    }
}
```

**Contract Guarantees:**
- ✅ Async delivery
- ✅ Timeout: Max 30 seconds per notification
- ✅ Idempotent (safe to retry)
- ✅ Returns delivery status for retry logic
- ✅ Properly formatted professional emails

**Test Requirements:**
- Mock for business logic tests
- In-memory test service for integration tests
- Real SMTP tests in infrastructure layer

---

### 1.3 IFileTransferService

**Purpose**: Abstract file transfer operations for testability and flexibility.

**Location**: `PolicyMonitor.Infrastructure/FileTransfer/IFileTransferService.cs`

```csharp
namespace PolicyMonitor.Infrastructure.FileTransfer
{
    /// <summary>
    /// Defines contract for transferring generated PDF files to destination locations.
    /// </summary>
    public interface IFileTransferService
    {
        /// <summary>
        /// Transfers a file from source to destination.
        /// </summary>
        /// <param name="sourcePath">Source file path</param>
        /// <param name="destinationPath">Destination directory or full path</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>FileTransferResult with transfer status and final path</returns>
        Task<FileTransferResult> TransferFileAsync(
            string sourcePath,
            string destinationPath,
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Verifies file exists at destination with correct size.
        /// </summary>
        Task<bool> VerifyFileAsync(string filePath, long expectedSize, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Tests write access to destination path.
        /// </summary>
        Task<bool> TestDestinationAccessAsync(string destinationPath, CancellationToken cancellationToken = default);
    }
    
    public class FileTransferResult
    {
        public bool Success { get; init; }
        public string FinalPath { get; init; } = string.Empty;
        public long FileSizeBytes { get; init; }
        public TimeSpan TransferDuration { get; init; }
        public string? ErrorMessage { get; init; }
        public Exception? Exception { get; init; }
    }
}
```

---

## 2. Data Access Contracts (Repository Pattern)

### 2.1 IRepository<T> (Generic Base)

**Purpose**: Common repository operations for all entities.

**Location**: `PolicyMonitor.Data/Repositories/IRepository.cs`

```csharp
namespace PolicyMonitor.Data.Repositories
{
    /// <summary>
    /// Generic repository pattern interface for data access operations.
    /// </summary>
    public interface IRepository<TEntity, TKey> where TEntity : class
    {
        /// <summary>
        /// Gets entity by primary key.
        /// </summary>
        Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets all entities (use with caution - prefer filtered queries).
        /// </summary>
        Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Adds a new entity.
        /// </summary>
        Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Updates an existing entity.
        /// </summary>
        Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Deletes an entity (rarely used in insurance domain - prefer soft delete).
        /// </summary>
        Task DeleteAsync(TKey id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Saves changes to database (if using Unit of Work pattern).
        /// </summary>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
```

---

### 2.2 IPolicyRepository

**Purpose**: Policy-specific data access operations.

**Location**: `PolicyMonitor.Data/Repositories/IPolicyRepository.cs`

```csharp
namespace PolicyMonitor.Data.Repositories
{
    public interface IPolicyRepository : IRepository<Policy, string>
    {
        /// <summary>
        /// Gets policies by status.
        /// </summary>
        Task<IEnumerable<Policy>> GetPoliciesByStatusAsync(
            string status,
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets policies assigned to specific agent.
        /// </summary>
        Task<IEnumerable<Policy>> GetPoliciesByAgentAsync(
            int agentId,
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Updates policy status (creates audit trail).
        /// </summary>
        Task UpdatePolicyStatusAsync(
            string policyNumber,
            string newStatus,
            string userId,
            CancellationToken cancellationToken = default);
    }
}
```

---

### 2.3 IStatusChangeEventRepository

**Location**: `PolicyMonitor.Data/Repositories/IStatusChangeEventRepository.cs`

```csharp
namespace PolicyMonitor.Data.Repositories
{
    public interface IStatusChangeEventRepository : IRepository<StatusChangeEvent, Guid>
    {
        /// <summary>
        /// Gets pending events for processing.
        /// </summary>
        Task<IEnumerable<StatusChangeEvent>> GetPendingEventsAsync(
            int maxCount,
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Updates event processing status.
        /// </summary>
        Task UpdateProcessingStatusAsync(
            Guid eventId,
            string status,
            string? errorMessage = null,
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets events for specific policy.
        /// </summary>
        Task<IEnumerable<StatusChangeEvent>> GetEventsByPolicyAsync(
            string policyNumber,
            CancellationToken cancellationToken = default);
    }
}
```

---

### 2.4 IGeneratedDocumentRepository

```csharp
namespace PolicyMonitor.Data.Repositories
{
    public interface IGeneratedDocumentRepository : IRepository<GeneratedDocument, Guid>
    {
        /// <summary>
        /// Gets document for specific event.
        /// </summary>
        Task<GeneratedDocument?> GetByEventIdAsync(
            Guid eventId,
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Updates document generation status.
        /// </summary>
        Task UpdateGenerationStatusAsync(
            Guid documentId,
            string status,
            string? filePath = null,
            long? fileSize = null,
            string? errorMessage = null,
            CancellationToken cancellationToken = default);
    }
}
```

---

### 2.5 IAgentNotificationRepository

```csharp
namespace PolicyMonitor.Data.Repositories
{
    public interface IAgentNotificationRepository : IRepository<AgentNotification, Guid>
    {
        /// <summary>
        /// Gets notifications ready for retry (NextRetryTime <= now).
        /// </summary>
        Task<IEnumerable<AgentNotification>> GetNotificationsForRetryAsync(
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Updates notification delivery status.
        /// </summary>
        Task UpdateDeliveryStatusAsync(
            Guid notificationId,
            string status,
            DateTime? deliveredTimestamp = null,
            string? errorMessage = null,
            CancellationToken cancellationToken = default);
    }
}
```

---

## 3. Factory Contracts

### 3.1 IPdfGeneratorFactory

**Purpose**: Create PDF generator instances based on configuration.

**Location**: `PolicyMonitor.Infrastructure/Factories/IPdfGeneratorFactory.cs`

```csharp
namespace PolicyMonitor.Infrastructure.Factories
{
    /// <summary>
    /// Factory for creating IPdfGenerator instances.
    /// Enables switching PDF libraries via configuration.
    /// </summary>
    public interface IPdfGeneratorFactory
    {
        /// <summary>
        /// Creates PDF generator based on configuration.
        /// </summary>
        /// <returns>Configured IPdfGenerator implementation</returns>
        IPdfGenerator CreatePdfGenerator();
        
        /// <summary>
        /// Gets available generator types.
        /// </summary>
        IEnumerable<string> GetAvailableGenerators();
    }
}
```

---

### 3.2 INotificationServiceFactory

**Location**: `PolicyMonitor.Infrastructure/Factories/INotificationServiceFactory.cs`

```csharp
namespace PolicyMonitor.Infrastructure.Factories
{
    /// <summary>
    /// Factory for creating INotificationService instances.
    /// Enables switching notification methods via configuration.
    /// </summary>
    public interface INotificationServiceFactory
    {
        /// <summary>
        /// Creates notification service for specified method.
        /// </summary>
        INotificationService CreateNotificationService(NotificationMethod method);
        
        /// <summary>
        /// Creates default notification service (Email for v1).
        /// </summary>
        INotificationService CreateDefaultNotificationService();
    }
}
```

---

## 4. Business Logic Contracts

### 4.1 IStatusChangeProcessor

**Purpose**: Core workflow orchestration for processing status changes.

**Location**: `PolicyMonitor.Domain/Services/IStatusChangeProcessor.cs`

```csharp
namespace PolicyMonitor.Domain.Services
{
    /// <summary>
    /// Core business logic for processing policy status changes.
    /// Orchestrates PDF generation, notification, and file transfer.
    /// </summary>
    public interface IStatusChangeProcessor
    {
        /// <summary>
        /// Processes a status change event through complete workflow.
        /// </summary>
        Task<ProcessingResult> ProcessStatusChangeAsync(
            Guid eventId,
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Retries failed processing operations.
        /// </summary>
        Task<ProcessingResult> RetryFailedOperationAsync(
            Guid eventId,
            CancellationToken cancellationToken = default);
    }
    
    public class ProcessingResult
    {
        public bool Success { get; init; }
        public bool PdfGenerated { get; init; }
        public bool NotificationSent { get; init; }
        public bool FileTransferred { get; init; }
        public List<string> Errors { get; init; } = new();
    }
}
```

---

## Contract Versioning

**Version Strategy:**
- Semantic versioning for breaking changes
- New optional methods don't break compatibility
- Major version bump for breaking changes
- Document all changes in contract file headers

**Current Version**: 1.0 (Initial release)

---

## Testing Contracts

All contracts MUST have:
- ✅ Mock implementations for unit tests
- ✅ In-memory implementations for integration tests
- ✅ Interface documentation with examples
- ✅ Exception specifications
- ✅ Async/await patterns throughout
- ✅ Cancellation token support

---

## Summary

**Total Contracts**: 11 interfaces
- Infrastructure: 3 (PDF, Notification, FileTransfer)
- Repositories: 5 (Generic + 4 specific)
- Factories: 2 (PDF, Notification)
- Business Logic: 1 (StatusChangeProcessor)

**Design Principles:**
- ✅ Dependency Inversion (depend on abstractions)
- ✅ Interface Segregation (focused interfaces)
- ✅ Single Responsibility
- ✅ Testability (all mockable)
- ✅ Swappability (Factory pattern)

These contracts enable 95%+ unit test coverage while maintaining clean architecture boundaries.
