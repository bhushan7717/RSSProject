using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;

namespace PolicyMonitor.Shared.Logging;

/// <summary>
/// Serilog configuration helper for consistent logging setup across the application.
/// </summary>
public static class SerilogConfiguration
{
    /// <summary>
    /// Configures Serilog using application configuration settings.
    /// </summary>
    public static ILogger CreateLogger(IConfiguration configuration)
    {
        var logLevel = configuration["Logging:LogLevel:Default"] ?? "Information";
        var minimumLevel = ParseLogLevel(logLevel);

        var logConfiguration = new LoggerConfiguration()
            .MinimumLevel.Is(minimumLevel)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.WithProperty("Application", "PolicyMonitor");

        // Console sink
        logConfiguration.WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");

        // File sink with rolling daily logs
        var logPath = configuration["Logging:File:Path"] ?? "./logs/policy-monitor-.log";
        logConfiguration.WriteTo.File(
            path: logPath,
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");

        return logConfiguration.CreateLogger();
    }

    private static LogEventLevel ParseLogLevel(string logLevel)
    {
        return logLevel.ToUpperInvariant() switch
        {
            "TRACE" => LogEventLevel.Verbose,
            "DEBUG" => LogEventLevel.Debug,
            "INFORMATION" => LogEventLevel.Information,
            "WARNING" => LogEventLevel.Warning,
            "ERROR" => LogEventLevel.Error,
            "CRITICAL" => LogEventLevel.Fatal,
            _ => LogEventLevel.Information
        };
    }
}
