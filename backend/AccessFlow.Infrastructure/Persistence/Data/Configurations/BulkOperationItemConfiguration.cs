using Microsoft.EntityFrameworkCore;
using AccessFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccessFlow.Infrastructure.Persistence.Data.Configurations;

public class BulkOperationItemConfiguration : IEntityTypeConfiguration<BulkOperationItem>
{
    public void Configure(EntityTypeBuilder<BulkOperationItem> builder)
    {
        builder.ToTable("bulk_operation_items");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .UseIdentityByDefaultColumn();

        builder.HasOne(x => x.BulkOperation)
            .WithMany(x => x.BulkOperationItems)
            .HasForeignKey(x => x.IdBulkOperation)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Property(x => x.IdBulkOperation)
            .HasColumnName("id_bulk_operation")
            .IsRequired();

        builder.HasOne(x => x.Client)
            .WithMany(x => x.BulkOperationItems)
            .HasForeignKey(x => x.IdClient)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Property(x => x.IdClient)
            .HasColumnName("id_client")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .IsRequired();

        builder.Property(x => x.Error)
            .HasColumnName("error");

        builder.HasIndex(x => new { x.IdBulkOperation, x.IdClient })
            .HasDatabaseName("ux_bulk_operation_items_operation_client")
            .IsUnique();
    }
}