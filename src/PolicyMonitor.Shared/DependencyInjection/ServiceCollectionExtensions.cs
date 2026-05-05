using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace PolicyMonitor.Shared.DependencyInjection;

/// <summary>
/// Extension methods for configuring dependency injection across the application.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds shared services (logging, configuration, resilience policies).
    /// </summary>
    public static IServiceCollection AddSharedServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Logging is configured at the host level using Serilog
        // Configuration is already available through IConfiguration injection

        // Resilience policies will be registered here
        // services.AddSingleton<IPdfRetryPolicy, PdfRetryPolicy>();
        // services.AddSingleton<IEmailRetryPolicy, EmailRetryPolicy>();
        // services.AddSingleton<IFileTransferRetryPolicy, FileTransferRetryPolicy>();

        return services;
    }

    /// <summary>
    /// Adds domain layer services.
    /// </summary>
    public static IServiceCollection AddDomainServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Domain services will be registered here
        // services.AddScoped<IPolicyService, PolicyService>();
        // services.AddScoped<IAgentService, AgentService>();
        // services.AddScoped<IStatusChangeService, StatusChangeService>();

        return services;
    }

    /// <summary>
    /// Adds infrastructure layer services (PDF, Email, File Transfer).
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Infrastructure services will be registered here
        // services.AddScoped<IPdfGenerator, QuestPdfGenerator>();
        // services.AddScoped<IEmailClient, MailKitEmailClient>();
        // services.AddScoped<IFileTransferService, NetworkShareFileTransferService>();
        // services.AddScoped<IPdfGeneratorFactory, PdfGeneratorFactory>();
        // services.AddScoped<INotificationServiceFactory, NotificationServiceFactory>();

        return services;
    }

    /// <summary>
    /// Adds data layer services (repositories, DbContext).
    /// </summary>
    public static IServiceCollection AddDataServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Data services will be registered here
        // services.AddDbContext<PolicyDbContext>(options =>
        //     options.UseSqlServer(configuration.GetConnectionString("PolicyMonitorDb")));
        
        // services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));
        // services.AddScoped<IPolicyRepository, PolicyRepository>();
        // services.AddScoped<IAgentRepository, AgentRepository>();
        // services.AddScoped<IStatusChangeEventRepository, StatusChangeEventRepository>();
        // services.AddScoped<IPdfDocumentRepository, PdfDocumentRepository>();
        // services.AddScoped<INotificationLogRepository, NotificationLogRepository>();
        // services.AddScoped<IFileTransferLogRepository, FileTransferLogRepository>();
        // services.AddScoped<IAuditLogRepository, AuditLogRepository>();

        return services;
    }
}
