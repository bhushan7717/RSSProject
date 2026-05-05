namespace PolicyMonitor.Domain.Exceptions;

/// <summary>
/// Base exception class for all Policy Monitor domain exceptions.
/// </summary>
public class PolicyMonitorException : Exception
{
    public PolicyMonitorException()
    {
    }

    public PolicyMonitorException(string message)
        : base(message)
    {
    }

    public PolicyMonitorException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when a requested entity is not found.
/// </summary>
public class EntityNotFoundException : PolicyMonitorException
{
    public string EntityType { get; }
    public object EntityId { get; }

    public EntityNotFoundException(string entityType, object entityId)
        : base($"{entityType} with ID '{entityId}' was not found.")
    {
        EntityType = entityType;
        EntityId = entityId;
    }

    public EntityNotFoundException(string entityType, object entityId, Exception innerException)
        : base($"{entityType} with ID '{entityId}' was not found.", innerException)
    {
        EntityType = entityType;
        EntityId = entityId;
    }
}

/// <summary>
/// Exception thrown when business validation rules are violated.
/// </summary>
public class ValidationException : PolicyMonitorException
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(string message)
        : base(message)
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(string message, IDictionary<string, string[]> errors)
        : base(message)
    {
        Errors = errors;
    }

    public ValidationException(string field, string error)
        : base($"Validation failed for field '{field}': {error}")
    {
        Errors = new Dictionary<string, string[]>
        {
            { field, new[] { error } }
        };
    }
}

/// <summary>
/// Exception thrown when PDF generation fails.
/// </summary>
public class PdfGenerationException : PolicyMonitorException
{
    public string PolicyNumber { get; }

    public PdfGenerationException(string policyNumber, string message)
        : base($"PDF generation failed for policy '{policyNumber}': {message}")
    {
        PolicyNumber = policyNumber;
    }

    public PdfGenerationException(string policyNumber, string message, Exception innerException)
        : base($"PDF generation failed for policy '{policyNumber}': {message}", innerException)
    {
        PolicyNumber = policyNumber;
    }
}

/// <summary>
/// Exception thrown when email delivery fails after all retries.
/// </summary>
public class EmailDeliveryException : PolicyMonitorException
{
    public string Recipient { get; }

    public EmailDeliveryException(string recipient, string message)
        : base($"Email delivery failed to '{recipient}': {message}")
    {
        Recipient = recipient;
    }

    public EmailDeliveryException(string recipient, string message, Exception innerException)
        : base($"Email delivery failed to '{recipient}': {message}", innerException)
    {
        Recipient = recipient;
    }
}

/// <summary>
/// Exception thrown when file transfer operations fail after all retries.
/// </summary>
public class FileTransferException : PolicyMonitorException
{
    public string FilePath { get; }

    public FileTransferException(string filePath, string message)
        : base($"File transfer failed for '{filePath}': {message}")
    {
        FilePath = filePath;
    }

    public FileTransferException(string filePath, string message, Exception innerException)
        : base($"File transfer failed for '{filePath}': {message}", innerException)
    {
        FilePath = filePath;
    }
}

/// <summary>
/// Exception thrown when database operations fail.
/// </summary>
public class DataAccessException : PolicyMonitorException
{
    public DataAccessException(string message)
        : base(message)
    {
    }

    public DataAccessException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
