# Data Model: Insurance Policy Status Monitor

**Feature**: Insurance Policy Status Monitor  
**Branch**: 001-policy-status-monitor  
**Date**: May 4, 2026

## Overview

This document defines the data entities, relationships, validation rules, and state transitions for the Insurance Policy Status Monitor system. The model supports automated detection of policy status changes, PDF generation, agent notifications, and audit trail maintenance.

## Entity Relationship Diagram

```
┌──────────────────┐         ┌───────────────────────┐
│     Policy       │ 1     * │  StatusChangeEvent    │
│                  ├─────────┤                       │
│ - PolicyNumber   │         │ - EventId             │
│ - PolicyholderId │         │ - PolicyNumber (FK)   │
│ - Status         │         │ - PreviousStatus      │
│ - AssignedAgents │         │ - NewStatus           │
└────────┬─────────┘         │ - ChangeTimestamp     │
         │                   │ - ChangedByUser       │
         │                   │ - ProcessingStatus    │
         │ *                 └──────────┬────────────┘
         │                              │ 1
┌────────┴─────────┐                    │
│      Agent       │                    │ *
│                  │         ┌──────────┴────────────┐
│ - AgentId        │ 1     * │  GeneratedDocument    │
│ - Name           ├─────────┤                       │
│ - Email          │         │ - DocumentId          │
│ - PhoneNumber    │         │ - EventId (FK)        │
│ - PreferredMethod│         │ - PolicyNumber (FK)   │
└──────────────────┘         │ - FileName            │
                             │ - FilePath            │
                             │ - GenerationTimestamp │
                             │ - GenerationStatus    │
                             └──────────┬────────────┘
                                        │ 1
                                        │
                             ┌──────────┴────────────┐
                             │  FileTransferRecord   │
                             │                       │
                             │ - TransferId          │
                             │ - DocumentId (FK)     │
                             │ - SourcePath          │
                             │ - DestinationPath     │
                             │ - TransferTimestamp   │
                             │ - TransferStatus      │
                             │ - RetryCount          │
                             └───────────────────────┘

         ┌───────────────────────┐
         │  AgentNotification    │
         │                       │
         │ - NotificationId      │
         │ - EventId (FK)        │
         │ - PolicyNumber (FK)   │
         │ - AgentId (FK)        │
         │ - DeliveryMethod      │
         │ - DeliveryStatus      │
         │ - SentTimestamp       │
         │ - RetryCount          │
         └───────────────────────┘
```

## Core Entities

### 1. Policy

Represents an insurance policy with status tracking.

**Attributes:**

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| PolicyNumber | string(20) | PK, NOT NULL, UNIQUE | Unique policy identifier |
| PolicyholderId | int | NOT NULL, FK → Policyholder | Reference to policyholder |
| PolicyholderName | string(200) | NOT NULL | Cached name for performance |
| PolicyType | string(50) | NOT NULL | Auto, Home, Life, Health, etc. |
| CoverageAmount | decimal(18,2) | NOT NULL, >= 0 | Coverage amount in USD |
| EffectiveDate | DateTime | NOT NULL | Policy start date |
| ExpirationDate | DateTime | NOT NULL | Policy end date |
| Status | string(50) | NOT NULL | Current policy status (see Status enum) |
| PreviousStatus | string(50) | NULL | Previous status for audit |
| LastStatusChangeDate | DateTime | NULL | Timestamp of last status change |
| CreatedDate | DateTime | NOT NULL, DEFAULT GETDATE() | Record creation timestamp |
| ModifiedDate | DateTime | NOT NULL, DEFAULT GETDATE() | Last modification timestamp |
| ModifiedByUser | string(100) | NULL | User who last modified |

**Relationships:**
- One policy → Many StatusChangeEvents (1:*)
- One policy → Many Agents (M:N through PolicyAgent junction)

**Validation Rules:**
- PolicyNumber: Format "POL-XXXXXXXX" where X is alphanumeric
- EffectiveDate must be <= ExpirationDate
- CoverageAmount must be positive
- Status must be valid PolicyStatus enum value

**Indexes:**
- Clustered: PolicyNumber (PK)
- Non-clustered: Status, EffectiveDate, ExpirationDate
- Non-clustered: PolicyholderId

### 2. PolicyStatus (Enumeration)

Valid policy lifecycle states.

**Values:**

| Value | Description | Transitions To |
|-------|-------------|----------------|
| Pending | Initial application submitted | UnderReview, Rejected |
| UnderReview | Being evaluated by underwriters | Approved, Rejected |
| Approved | Accepted but not yet active | Active, Cancelled |
| Active | Policy in force | Suspended, Cancelled, Lapsed, Expired |
| Suspended | Temporarily inactive | Active, Cancelled, Lapsed |
| Cancelled | Terminated by policyholder | (terminal state) |
| Lapsed | Terminated due to non-payment | (terminal state) |
| Rejected | Application denied | (terminal state) |
| Expired | Policy term ended | (terminal state) |

**State Transition Rules:**
- Any status can transition to Cancelled (policyholder cancellation)
- Terminal states (Cancelled, Lapsed, Rejected, Expired) cannot transition
- Status changes must follow valid transition paths
- Rollback transitions allowed (e.g., Active → Pending) for corrections

### 3. StatusChangeEvent

Represents a detected policy status change requiring processing.

**Attributes:**

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| EventId | Guid | PK, NOT NULL, DEFAULT NEWID() | Unique event identifier |
| PolicyNumber | string(20) | NOT NULL, FK → Policy | Policy that changed |
| PreviousStatus | string(50) | NOT NULL | Status before change |
| NewStatus | string(50) | NOT NULL | Status after change |
| ChangeTimestamp | DateTime | NOT NULL, DEFAULT GETDATE() | When change occurred |
| ChangedByUser | string(100) | NOT NULL | User/system that made change |
| DetectedTimestamp | DateTime | NOT NULL, DEFAULT GETDATE() | When system detected change |
| ProcessingStatus | string(50) | NOT NULL, DEFAULT 'Pending' | Current processing state |
| ProcessingStarted | DateTime | NULL | When processing began |
| ProcessingCompleted | DateTime | NULL | When processing finished |
| ErrorMessage | string(MAX) | NULL | Error details if failed |
| RetryCount | int | NOT NULL, DEFAULT 0 | Number of retry attempts |

**Relationships:**
- Many events → One policy (EventId FK)
- One event → Many GeneratedDocuments (1:*)
- One event → Many AgentNotifications (1:*)

**Validation Rules:**
- PreviousStatus ≠ NewStatus (must be actual change)
- ChangeTimestamp <= DetectedTimestamp
- ProcessingStatus: Pending, InProgress, Completed, Failed
- RetryCount: 0-3 (max 3 retries per spec)

**Indexes:**
- Clustered: EventId (PK)
- Non-clustered: PolicyNumber, ProcessingStatus, ChangeTimestamp
- Covering index: (ProcessingStatus, DetectedTimestamp) INCLUDE (EventId, PolicyNumber)

### 4. GeneratedDocument

Represents a PDF document generated for a status change.

**Attributes:**

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| DocumentId | Guid | PK, NOT NULL, DEFAULT NEWID() | Unique document identifier |
| EventId | Guid | NOT NULL, FK → StatusChangeEvent | Associated change event |
| PolicyNumber | string(20) | NOT NULL, FK → Policy | Policy reference |
| FileName | string(255) | NOT NULL | Generated file name |
| FilePath | string(500) | NOT NULL | Full path to PDF file |
| FileSize | bigint | NULL | File size in bytes |
| GenerationTimestamp | DateTime | NOT NULL, DEFAULT GETDATE() | When PDF was generated |
| GenerationStatus | string(50) | NOT NULL, DEFAULT 'Pending' | Generation status |
| GenerationDuration | int | NULL | Generation time in milliseconds |
| ErrorMessage | string(MAX) | NULL | Error details if failed |

**Relationships:**
- Many documents → One event (1:*)
- One document → One FileTransferRecord (1:1)

**Validation Rules:**
- FileName format: "PolicyNum_Status_YYYYMMDD_HHMMSS.pdf"
- FilePath must be valid file system path
- GenerationStatus: Pending, InProgress, Completed, Failed
- FileSize must be positive if status = Completed

**Indexes:**
- Clustered: DocumentId (PK)
- Non-clustered: EventId, PolicyNumber, GenerationTimestamp
- Non-clustered: FilePath (for duplicate detection)

### 5. AgentNotification

Represents a notification sent to an insurance agent.

**Attributes:**

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| NotificationId | Guid | PK, NOT NULL, DEFAULT NEWID() | Unique notification identifier |
| EventId | Guid | NOT NULL, FK → StatusChangeEvent | Associated change event |
| PolicyNumber | string(20) | NOT NULL, FK → Policy | Policy reference |
| AgentId | int | NOT NULL, FK → Agent | Target agent |
| DeliveryMethod | string(50) | NOT NULL | Email, SMS, etc. |
| RecipientAddress | string(255) | NOT NULL | Email address or phone |
| Subject | string(255) | NULL | Email subject line |
| MessageBody | string(MAX) | NULL | Notification content |
| DeliveryStatus | string(50) | NOT NULL, DEFAULT 'Pending' | Delivery status |
| SentTimestamp | DateTime | NULL | When notification was sent |
| DeliveredTimestamp | DateTime | NULL | When delivery confirmed |
| ErrorMessage | string(MAX) | NULL | Error details if failed |
| RetryCount | int | NOT NULL, DEFAULT 0 | Number of retry attempts |
| NextRetryTime | DateTime | NULL | Scheduled next retry time |

**Relationships:**
- Many notifications → One event (1:*)
- Many notifications → One agent (1:*)
- Many notifications → One policy (1:*)

**Validation Rules:**
- DeliveryMethod: Email (v1), future: SMS, Push
- RecipientAddress: Valid email format if DeliveryMethod = Email
- DeliveryStatus: Pending, Sending, Sent, Delivered, Failed
- RetryCount: 0-3 (max 3 retries with backoff)
- NextRetryTime follows exponential backoff: 30s, 2min, 5min

**Indexes:**
- Clustered: NotificationId (PK)
- Non-clustered: EventId, AgentId, DeliveryStatus
- Non-clustered: NextRetryTime (for retry queue processing)

### 6. FileTransferRecord

Represents a file transfer operation for generated PDFs.

**Attributes:**

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| TransferId | Guid | PK, NOT NULL, DEFAULT NEWID() | Unique transfer identifier |
| DocumentId | Guid | NOT NULL, FK → GeneratedDocument | Document being transferred |
| SourcePath | string(500) | NOT NULL | Source file location |
| DestinationPath | string(500) | NOT NULL | Target file location |
| TransferTimestamp | DateTime | NULL | When transfer completed |
| TransferStatus | string(50) | NOT NULL, DEFAULT 'Pending' | Transfer status |
| TransferDuration | int | NULL | Transfer time in milliseconds |
| FileSizeBytes | bigint | NULL | File size for verification |
| ErrorMessage | string(MAX) | NULL | Error details if failed |
| RetryCount | int | NOT NULL, DEFAULT 0 | Number of retry attempts |
| NextRetryTime | DateTime | NULL | Scheduled next retry time |

**Relationships:**
- One transfer → One document (1:1)

**Validation Rules:**
- SourcePath must be valid and file must exist
- DestinationPath must be valid UNC or local path
- TransferStatus: Pending, InProgress, Completed, Failed, Queued
- RetryCount: 0-24 (retry every 5 min for 2 hours = 24 attempts)
- NextRetryTime: Current time + (RetryCount * 5 minutes)

**Indexes:**
- Clustered: TransferId (PK)
- Non-clustered: DocumentId, TransferStatus
- Non-clustered: NextRetryTime (for retry queue processing)

### 7. Agent

Represents an insurance agent assigned to policies.

**Attributes:**

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| AgentId | int | PK, IDENTITY, NOT NULL | Unique agent identifier |
| EmployeeId | string(20) | UNIQUE, NOT NULL | Company employee ID |
| FirstName | string(100) | NOT NULL | Agent first name |
| LastName | string(100) | NOT NULL | Agent last name |
| Email | string(255) | NOT NULL | Primary email address |
| PhoneNumber | string(20) | NULL | Phone number (future SMS) |
| PreferredNotificationMethod | string(50) | NOT NULL, DEFAULT 'Email' | Preferred contact method |
| IsActive | bit | NOT NULL, DEFAULT 1 | Active employment status |
| CreatedDate | DateTime | NOT NULL, DEFAULT GETDATE() | Record creation |
| ModifiedDate | DateTime | NOT NULL, DEFAULT GETDATE() | Last modification |

**Relationships:**
- One agent → Many policies (M:N through PolicyAgent)
- One agent → Many notifications (1:*)

**Validation Rules:**
- Email: Valid email format
- PhoneNumber: E.164 format if provided (+1XXXXXXXXXX)
- PreferredNotificationMethod: Email (v1)
- At least one contact method (Email or PhoneNumber) required

**Indexes:**
- Clustered: AgentId (PK)
- Non-clustered: EmployeeId (UNIQUE)
- Non-clustered: Email, IsActive

## Junction Tables

### PolicyAgent

Maps many-to-many relationship between policies and agents.

| Field | Type | Constraints |
|-------|------|-------------|
| PolicyNumber | string(20) | FK → Policy, PK (composite) |
| AgentId | int | FK → Agent, PK (composite) |
| AssignmentDate | DateTime | NOT NULL, DEFAULT GETDATE() |
| IsPrimaryAgent | bit | NOT NULL, DEFAULT 0 |

## Audit & Compliance Entities

### AuditLog

Comprehensive audit trail for all policy status changes and processing.

**Attributes:**

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| AuditId | Guid | PK, NOT NULL, DEFAULT NEWID() | Unique audit entry |
| EventId | Guid | NULL, FK → StatusChangeEvent | Related event if applicable |
| PolicyNumber | string(20) | NULL | Policy reference |
| EventType | string(50) | NOT NULL | StatusChange, PdfGenerated, NotificationSent, etc. |
| EventTimestamp | DateTime | NOT NULL, DEFAULT GETDATE() | When event occurred |
| UserId | string(100) | NULL | User/system performing action |
| Details | string(MAX) | NULL | JSON or text details |
| Success | bit | NOT NULL | Operation success flag |
| ErrorMessage | string(MAX) | NULL | Error details if failed |

**Validation Rules:**
- EventType: StatusChange, PdfGeneration, NotificationDelivery, FileTransfer, SystemError
- Retention: 7 years minimum (compliance requirement)

**Indexes:**
- Clustered: AuditId (PK)
- Non-clustered: EventTimestamp DESC (for reporting)
- Non-clustered: PolicyNumber, EventType

## Performance Optimizations

### Partitioning Strategy

- **StatusChangeEvent**: Partition by ChangeTimestamp (monthly partitions)
- **AuditLog**: Partition by EventTimestamp (quarterly partitions)
- Older partitions can be compressed or archived

### Caching Considerations

- **Policy**: Cache active policies in memory (invalidate on status change)
- **Agent**: Cache agent contact info (low change frequency)
- **PolicyStatus enum**: Static cache

### Archival Strategy

- **StatusChangeEvent**: Archive events older than 1 year to cold storage
- **GeneratedDocument**: Archive file paths but retain PDFs indefinitely
- **AuditLog**: Retain 7 years online, archive beyond that to compliant storage

## Data Integrity Constraints

### Foreign Key Constraints

- All FK relationships enforced with ON DELETE NO ACTION (preserve audit trail)
- Cascading deletes prohibited (insurance compliance)

### Check Constraints

- CoverageAmount >= 0
- EffectiveDate <= ExpirationDate
- RetryCount >= 0 AND RetryCount <= (max retries)
- Timestamps: Created <= Modified

### Unique Constraints

- Policy.PolicyNumber
- Agent.EmployeeId
- Agent.Email (to prevent duplicate notifications)

## State Transition Validation

### Database Trigger Logic

```sql
-- Pseudo-code for status change validation trigger
CREATE TRIGGER trg_ValidateStatusTransition
ON Policy
AFTER UPDATE
AS
BEGIN
    -- Validate status transition is allowed
    IF EXISTS (
        SELECT 1 FROM inserted i
        INNER JOIN deleted d ON i.PolicyNumber = d.PolicyNumber
        WHERE i.Status != d.Status
          AND NOT EXISTS (
              SELECT 1 FROM AllowedStatusTransitions
              WHERE FromStatus = d.Status AND ToStatus = i.Status
          )
    )
    BEGIN
        RAISERROR('Invalid status transition', 16, 1)
        ROLLBACK TRANSACTION
    END
    
    -- Create StatusChangeEvent for valid transitions
    INSERT INTO StatusChangeEvent (
        PolicyNumber, PreviousStatus, NewStatus,
        ChangedByUser, ChangeTimestamp
    )
    SELECT 
        i.PolicyNumber, d.Status, i.Status,
        SUSER_SNAME(), GETDATE()
    FROM inserted i
    INNER JOIN deleted d ON i.PolicyNumber = d.PolicyNumber
    WHERE i.Status != d.Status
END
```

## Summary

This data model supports:
- ✅ Real-time policy status change detection via triggers
- ✅ Complete audit trail for compliance (7-year retention)
- ✅ Asynchronous processing with retry mechanisms
- ✅ PDF generation tracking with file transfer records
- ✅ Agent notification delivery with exponential backoff
- ✅ Performance optimization through indexing and partitioning
- ✅ Data integrity through constraints and validation

**Next Steps:**
- Generate EF Core entity classes from this model
- Create database migration scripts
- Implement Repository pattern interfaces
- Define contracts (Phase 1 continuation)
