using Microsoft.EntityFrameworkCore;
using SharedLibrary.Models;
using SharedLibraryAgents.Models;

public class PatchDbContext : DbContext
{
    public PatchDbContext(DbContextOptions<PatchDbContext> options) : base(options)
    {
    }

    public DbSet<Patch> Patches { get; set; }
    public DbSet<PatchStatusReport> PatchStatusReports { get; set; }
    public DbSet<CustomerAgent> CustomerAgents { get; set; }

    public DbSet<AgentMonitoredProducts> AgentMonitoredProducts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Patch>().ToTable("patches");
        modelBuilder.Entity<PatchStatusReport>().ToTable("patch_status_reports");
        modelBuilder.Entity<CustomerAgent>().ToTable("customer_agents");
        modelBuilder.Entity<AgentMonitoredProducts>().ToTable("agent_monitored_products");
    }
}
