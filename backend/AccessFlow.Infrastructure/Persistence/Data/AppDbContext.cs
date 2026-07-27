using Microsoft.EntityFrameworkCore;
using AccessFlow.Domain.Entities;

namespace AccessFlow.Infrastructure.Persistence.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<BulkOperation> BulkOperations => Set<BulkOperation>();
    public DbSet<BulkOperationItem> BulkOperationItems => Set<BulkOperationItem>();
    public DbSet<Connection> Connections => Set<Connection>();
    public DbSet<Notification> Notifications => Set<Notification>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}