using LearnHuggingFaceLangChainApp1.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace LearnHuggingFaceLangChainApp1.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext() : base() {}
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    public DbSet<ChatMessage> ChatMessages { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .Build();

            var cs = config.GetConnectionString("DefaultConnection")
                     ?? Environment.GetEnvironmentVariable("PG_CONN")
                     ?? throw new InvalidOperationException("No connection string found.");

            optionsBuilder.UseNpgsql(cs);
        }
    }
}