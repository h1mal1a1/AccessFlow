using Microsoft.EntityFrameworkCore;
using AccessFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccessFlow.Infrastructure.Persistence.Data.Configurations;

public class BulkOperationConfiguration : IEntityTypeConfiguration<BulkOperation>
{
    public void Configure(EntityTypeBuilder<BulkOperation> builder)
    {
        builder.ToTable("bulk_operation");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .UseIdentityByDefaultColumn();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .IsRequired();

        builder.Property(x => x.UsersProcessed)
            .HasColumnName("users_processed");

        builder.Property(x => x.SuccessfullyProcessed)
            .HasColumnName("successfully_processed");

        builder.Property(x => x.ErrorProcessed)
            .HasColumnName("error_processed");

        builder.Property(x => x.StartAt)
            .HasColumnName("start_at")
            .IsRequired();

        builder.Property(x => x.EndAt)
            .HasColumnName("end_at");
    }
}