using Polly;
using Polly.Retry;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;

namespace PolicyMonitor.Shared.Resilience;

/// <summary>
/// Polly retry policy for email delivery operations.
/// Implements exponential backoff for SMTP connection and delivery failures.
/// </summary>
public static class EmailRetryPolicy
{
    /// <summary>
    /// Creates a retry policy for email delivery with exponential backoff.
    /// Max 5 retries with delays: 2s, 4s, 8s, 16s, 32s (plus jitter).
    /// </summary>
    public static IAsyncPolicy CreatePolicy(ILogger logger)
    {
        return Policy
            .Handle<Exception>(ex => IsTransientException(ex))
            .WaitAndRetryAsync(
                retryCount: 5,
                sleepDurationProvider: retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + TimeSpan.FromMilliseconds(Random.Shared.Next(0, 1000)),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    logger.LogWarning(
                        exception,
                        "Email delivery retry {RetryCount} after {RetryDelay}s due to: {ExceptionMessage}",
                        retryCount,
                        timeSpan.TotalSeconds,
                        exception.Message);
                });
    }

    /// <summary>
    /// Creates a circuit breaker policy for email delivery.
    /// Opens circuit after 3 consecutive failures, stays open for 60 seconds.
    /// </summary>
    public static IAsyncPolicy CreateCircuitBreakerPolicy(ILogger logger)
    {
        return Policy
            .Handle<Exception>(ex => IsTransientException(ex))
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 3,
                durationOfBreak: TimeSpan.FromSeconds(60),
                onBreak: (exception, duration) =>
                {
                    logger.LogError(
                        exception,
                        "Email circuit breaker opened for {BreakDuration}s due to: {ExceptionMessage}",
                        duration.TotalSeconds,
                        exception.Message);
                },
                onReset: () =>
                {
                    logger.LogInformation("Email circuit breaker reset - service restored");
                },
                onHalfOpen: () =>
                {
                    logger.LogInformation("Email circuit breaker half-open - testing service");
                });
    }

    private static bool IsTransientException(Exception ex)
    {
        // Transient SMTP failures that should be retried
        return ex is SocketException
            || ex is IOException
            || ex is TimeoutException
            || (ex.Message?.Contains("temporarily unavailable", StringComparison.OrdinalIgnoreCase) ?? false)
            || (ex.Message?.Contains("connection refused", StringComparison.OrdinalIgnoreCase) ?? false)
            || (ex.InnerException != null && IsTransientException(ex.InnerException));
    }
}
