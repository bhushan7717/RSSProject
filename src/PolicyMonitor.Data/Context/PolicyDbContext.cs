using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace PolicyMonitor.Data.Context;

/// <summary>
/// Entity Framework Core database context for the Policy Status Monitor.
/// </summary>
public class PolicyDbContext : DbContext
{
    private readonly IConfiguration? _configuration;

    public PolicyDbContext(DbContextOptions<PolicyDbContext> options)
        : base(options)
    {
    }

    public PolicyDbContext(DbContextOptions<PolicyDbContext> options, IConfiguration configuration)
        : base(options)
    {
        _configuration = configuration;
    }

    // DbSets will be added here as entities are created
    // public DbSet<Policy> Policies { get; set; } = null!;
    // public DbSet<Agent> Agents { get; set; } = null!;
    // public DbSet<StatusChangeEvent> StatusChangeEvents { get; set; } = null!;
    // public DbSet<PdfDocument> PdfDocuments { get; set; } = null!;
    // public DbSet<NotificationLog> NotificationLogs { get; set; } = null!;
    // public DbSet<FileTransferLog> FileTransferLogs { get; set; } = null!;
    // public DbSet<AuditLog> AuditLogs { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured && _configuration != null)
        {
            var connectionString = _configuration.GetConnectionString("PolicyMonitorDb")
                ?? throw new InvalidOperationException("Connection string 'PolicyMonitorDb' not found.");

            optionsBuilder.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorNumbersToAdd: null);
                sqlOptions.CommandTimeout(30);
            });
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations as they are created
        // modelBuilder.ApplyConfiguration(new PolicyConfiguration());
        // modelBuilder.ApplyConfiguration(new AgentConfiguration());
        // modelBuilder.ApplyConfiguration(new StatusChangeEventConfiguration());
        // modelBuilder.ApplyConfiguration(new PdfDocumentConfiguration());
        // modelBuilder.ApplyConfiguration(new NotificationLogConfiguration());
        // modelBuilder.ApplyConfiguration(new FileTransferLogConfiguration());
        // modelBuilder.ApplyConfiguration(new AuditLogConfiguration());

        // Global query filters and conventions will be added here
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Audit trail logic will be added here
        return base.SaveChangesAsync(cancellationToken);
    }
}
