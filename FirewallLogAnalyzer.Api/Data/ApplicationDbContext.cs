// FirewallLogAnalyzer.Api/Data/ApplicationDbContext.cs
using FirewallLogAnalyzer.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FirewallLogAnalyzer.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<LogEntry> LogEntries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure LogEntry entity if needed (e.g., indexes)
        modelBuilder.Entity<LogEntry>(entity =>
        {
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.SourceIP);
            entity.HasIndex(e => e.DestinationIP);
            entity.HasIndex(e => e.Action);
            // Add more indexes as needed for query performance
        });
    }
}