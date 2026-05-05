using Polly;
using Polly.Retry;
using Microsoft.Extensions.Logging;

namespace PolicyMonitor.Shared.Resilience;

/// <summary>
/// Polly retry policy for file transfer operations.
/// Implements aggressive retry strategy for network share accessibility.
/// </summary>
public static class FileTransferRetryPolicy
{
    /// <summary>
    /// Creates a retry policy for file transfer with fixed intervals.
    /// Max 24 retries every 5 minutes for 2-hour window (per AC-041).
    /// </summary>
    public static IAsyncPolicy CreatePolicy(ILogger logger)
    {
        return Policy
            .Handle<Exception>(ex => IsTransientException(ex))
            .WaitAndRetryAsync(
                retryCount: 24,
                sleepDurationProvider: retryAttempt => TimeSpan.FromMinutes(5),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    logger.LogWarning(
                        exception,
                        "File transfer retry {RetryCount}/24 after {RetryDelay}m due to: {ExceptionMessage}",
                        retryCount,
                        timeSpan.TotalMinutes,
                        exception.Message);

                    // Log critical alert if reaching final retries
                    if (retryCount >= 20)
                    {
                        logger.LogError(
                            "File transfer approaching max retries ({RetryCount}/24) - manual intervention may be required",
                            retryCount);
                    }
                });
    }

    /// <summary>
    /// Creates a timeout policy for individual file transfer operations (5 minutes).
    /// </summary>
    public static IAsyncPolicy CreateTimeoutPolicy()
    {
        return Policy.TimeoutAsync(TimeSpan.FromMinutes(5));
    }

    private static bool IsTransientException(Exception ex)
    {
        // Transient file system/network failures that should be retried
        return ex is IOException
            || ex is UnauthorizedAccessException
            || ex is TimeoutException
            || (ex.Message?.Contains("network path", StringComparison.OrdinalIgnoreCase) ?? false)
            || (ex.Message?.Contains("network name", StringComparison.OrdinalIgnoreCase) ?? false)
            || (ex.Message?.Contains("access denied", StringComparison.OrdinalIgnoreCase) ?? false)
            || (ex.InnerException != null && IsTransientException(ex.InnerException));
    }
}
