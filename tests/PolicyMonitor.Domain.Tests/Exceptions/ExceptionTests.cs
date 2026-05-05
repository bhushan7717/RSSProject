using FluentAssertions;
using PolicyMonitor.Domain.Exceptions;
using Xunit;

namespace PolicyMonitor.Domain.Tests.Exceptions;

public class ExceptionTests
{
    [Fact]
    public void PolicyMonitorException_WithMessage_CreatesException()
    {
        // Act
        var exception = new PolicyMonitorException("Test error");

        // Assert
        exception.Message.Should().Be("Test error");
    }

    [Fact]
    public void PolicyMonitorException_WithMessageAndInnerException_CreatesException()
    {
        // Arrange
        var inner = new InvalidOperationException("Inner error");

        // Act
        var exception = new PolicyMonitorException("Outer error", inner);

        // Assert
        exception.Message.Should().Be("Outer error");
        exception.InnerException.Should().Be(inner);
    }

    [Fact]
    public void EntityNotFoundException_WithEntityTypeAndId_CreatesExceptionWithCorrectMessage()
    {
        // Act
        var exception = new EntityNotFoundException("Policy", "ABC123");

        // Assert
        exception.Message.Should().Be("Policy with ID 'ABC123' was not found.");
        exception.EntityType.Should().Be("Policy");
        exception.EntityId.Should().Be("ABC123");
    }

    [Fact]
    public void EntityNotFoundException_WithInnerException_PreservesInnerException()
    {
        // Arrange
        var inner = new InvalidOperationException("Database error");

        // Act
        var exception = new EntityNotFoundException("Agent", 42, inner);

        // Assert
        exception.EntityType.Should().Be("Agent");
        exception.EntityId.Should().Be(42);
        exception.InnerException.Should().Be(inner);
    }

    [Fact]
    public void ValidationException_WithMessage_CreatesException()
    {
        // Act
        var exception = new ValidationException("Validation failed");

        // Assert
        exception.Message.Should().Be("Validation failed");
        exception.Errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidationException_WithFieldAndError_CreatesExceptionWithErrors()
    {
        // Act
        var exception = new ValidationException("PolicyNumber", "Policy number is required");

        // Assert
        exception.Message.Should().Contain("PolicyNumber");
        exception.Errors.Should().ContainKey("PolicyNumber");
        exception.Errors["PolicyNumber"].Should().Contain("Policy number is required");
    }

    [Fact]
    public void ValidationException_WithErrorsDictionary_CreatesExceptionWithMultipleErrors()
    {
        // Arrange
        var errors = new Dictionary<string, string[]>
        {
            { "PolicyNumber", new[] { "Required", "Must be 9 digits" } },
            { "EffectiveDate", new[] { "Must be in the future" } }
        };

        // Act
        var exception = new ValidationException("Multiple validation errors", errors);

        // Assert
        exception.Errors.Should().HaveCount(2);
        exception.Errors["PolicyNumber"].Should().HaveCount(2);
        exception.Errors["EffectiveDate"].Should().HaveCount(1);
    }

    [Fact]
    public void PdfGenerationException_WithPolicyNumber_CreatesExceptionWithCorrectMessage()
    {
        // Act
        var exception = new PdfGenerationException("POL123456", "Template not found");

        // Assert
        exception.Message.Should().Contain("POL123456");
        exception.Message.Should().Contain("Template not found");
        exception.PolicyNumber.Should().Be("POL123456");
    }

    [Fact]
    public void PdfGenerationException_WithInnerException_PreservesInnerException()
    {
        // Arrange
        var inner = new FileNotFoundException("Template.pdf");

        // Act
        var exception = new PdfGenerationException("POL123456", "Template error", inner);

        // Assert
        exception.PolicyNumber.Should().Be("POL123456");
        exception.InnerException.Should().Be(inner);
    }

    [Fact]
    public void EmailDeliveryException_WithRecipient_CreatesExceptionWithCorrectMessage()
    {
        // Act
        var exception = new EmailDeliveryException("agent@example.com", "SMTP connection failed");

        // Assert
        exception.Message.Should().Contain("agent@example.com");
        exception.Message.Should().Contain("SMTP connection failed");
        exception.Recipient.Should().Be("agent@example.com");
    }

    [Fact]
    public void EmailDeliveryException_WithInnerException_PreservesInnerException()
    {
        // Arrange
        var inner = new System.Net.Sockets.SocketException();

        // Act
        var exception = new EmailDeliveryException("test@test.com", "Network error", inner);

        // Assert
        exception.Recipient.Should().Be("test@test.com");
        exception.InnerException.Should().Be(inner);
    }

    [Fact]
    public void FileTransferException_WithFilePath_CreatesExceptionWithCorrectMessage()
    {
        // Act
        var exception = new FileTransferException(@"\\server\share\file.pdf", "Network path not found");

        // Assert
        exception.Message.Should().Contain(@"\\server\share\file.pdf");
        exception.Message.Should().Contain("Network path not found");
        exception.FilePath.Should().Be(@"\\server\share\file.pdf");
    }

    [Fact]
    public void FileTransferException_WithInnerException_PreservesInnerException()
    {
        // Arrange
        var inner = new IOException("Disk full");

        // Act
        var exception = new FileTransferException(@"C:\temp\file.pdf", "Write failed", inner);

        // Assert
        exception.FilePath.Should().Be(@"C:\temp\file.pdf");
        exception.InnerException.Should().Be(inner);
    }

    [Fact]
    public void DataAccessException_WithMessage_CreatesException()
    {
        // Act
        var exception = new DataAccessException("Connection timeout");

        // Assert
        exception.Message.Should().Be("Connection timeout");
    }

    [Fact]
    public void DataAccessException_WithInnerException_PreservesInnerException()
    {
        // Arrange
        var inner = new TimeoutException("SQL timeout");

        // Act
        var exception = new DataAccessException("Database error", inner);

        // Assert
        exception.Message.Should().Be("Database error");
        exception.InnerException.Should().Be(inner);
    }

    [Fact]
    public void AllExceptions_InheritFromPolicyMonitorException()
    {
        // Assert
        typeof(EntityNotFoundException).Should().BeDerivedFrom<PolicyMonitorException>();
        typeof(ValidationException).Should().BeDerivedFrom<PolicyMonitorException>();
        typeof(PdfGenerationException).Should().BeDerivedFrom<PolicyMonitorException>();
        typeof(EmailDeliveryException).Should().BeDerivedFrom<PolicyMonitorException>();
        typeof(FileTransferException).Should().BeDerivedFrom<PolicyMonitorException>();
        typeof(DataAccessException).Should().BeDerivedFrom<PolicyMonitorException>();
    }
}
