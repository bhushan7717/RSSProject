using Microsoft.Extensions.Configuration;

namespace PolicyMonitor.Shared.Configuration;

/// <summary>
/// Extension methods for IConfiguration to retrieve strongly-typed settings.
/// </summary>
public static class ConfigurationExtensions
{
    /// <summary>
    /// Gets a configuration value or throws if not found.
    /// </summary>
    public static string GetRequiredValue(this IConfiguration configuration, string key)
    {
        return configuration[key]
            ?? throw new InvalidOperationException($"Required configuration key '{key}' not found.");
    }

    /// <summary>
    /// Gets a connection string or throws if not found.
    /// </summary>
    public static string GetRequiredConnectionString(this IConfiguration configuration, string name)
    {
        return configuration.GetConnectionString(name)
            ?? throw new InvalidOperationException($"Connection string '{name}' not found.");
    }

    /// <summary>
    /// Gets a configuration section and binds it to a strongly-typed object.
    /// </summary>
    public static T GetRequiredSection<T>(this IConfiguration configuration, string sectionName) where T : new()
    {
        var section = configuration.GetSection(sectionName);
        if (!section.Exists())
        {
            throw new InvalidOperationException($"Configuration section '{sectionName}' not found.");
        }

        var settings = new T();
        section.Bind(settings);
        return settings;
    }

    /// <summary>
    /// Gets a configuration value as an integer with a default fallback.
    /// </summary>
    public static int GetIntValue(this IConfiguration configuration, string key, int defaultValue)
    {
        var value = configuration[key];
        return int.TryParse(value, out var result) ? result : defaultValue;
    }

    /// <summary>
    /// Gets a configuration value as a boolean with a default fallback.
    /// </summary>
    public static bool GetBoolValue(this IConfiguration configuration, string key, bool defaultValue)
    {
        var value = configuration[key];
        return bool.TryParse(value, out var result) ? result : defaultValue;
    }

    /// <summary>
    /// Gets a configuration value as a TimeSpan with a default fallback.
    /// </summary>
    public static TimeSpan GetTimeSpanValue(this IConfiguration configuration, string key, TimeSpan defaultValue)
    {
        var value = configuration[key];
        return TimeSpan.TryParse(value, out var result) ? result : defaultValue;
    }
}
