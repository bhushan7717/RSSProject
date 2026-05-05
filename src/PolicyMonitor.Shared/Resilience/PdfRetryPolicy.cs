using Polly;
using Polly.Retry;
using Microsoft.Extensions.Logging;

namespace PolicyMonitor.Shared.Resilience;

/// <summary>
/// Polly retry policy for PDF generation operations.
/// Implements exponential backoff with jitter for transient failures.
/// </summary>
public static class PdfRetryPolicy
{
    /// <summary>
    /// Creates a retry policy for PDF generation with exponential backoff.
    /// Max 3 retries with delays: 2s, 4s, 8s (plus jitter).
    /// </summary>
    public static IAsyncPolicy CreatePolicy(ILogger logger)
    {
        return Policy
            .Handle<Exception>(ex => IsTransientException(ex))
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + TimeSpan.FromMilliseconds(Random.Shared.Next(0, 1000)),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    logger.LogWarning(
                        exception,
                        "PDF generation retry {RetryCount} after {RetryDelay}s due to: {ExceptionMessage}",
                        retryCount,
                        timeSpan.TotalSeconds,
                        exception.Message);
                });
    }

    /// <summary>
    /// Creates a timeout policy for PDF generation operations (30 seconds).
    /// </summary>
    public static IAsyncPolicy CreateTimeoutPolicy()
    {
        return Policy.TimeoutAsync(TimeSpan.FromSeconds(30));
    }

    private static bool IsTransientException(Exception ex)
    {
        // Transient failures that should be retried
        return ex is IOException
            || ex is TimeoutException
            || ex is InvalidOperationException
            || (ex.InnerException != null && IsTransientException(ex.InnerException));
    }
}
