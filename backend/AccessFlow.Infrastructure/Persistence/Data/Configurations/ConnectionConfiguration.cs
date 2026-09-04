using Microsoft.EntityFrameworkCore;
using AccessFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccessFlow.Infrastructure.Persistence.Data.Configurations;

public class ConnectionConfiguration : IEntityTypeConfiguration<Connection>
{
    public void Configure(EntityTypeBuilder<Connection> builder)
    {
        builder.ToTable("connections");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .UseIdentityByDefaultColumn();

        builder.Property(x => x.IdClient)
            .HasColumnName("id_client")
            .IsRequired();
        builder.HasOne(x => x.Client)
            .WithMany(x => x.Connections)
            .HasForeignKey(x => x.IdClient)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(x => x.Client.Status != "Deleted");

        builder.Property(x => x.IdExternal).HasColumnName("id_external").IsRequired();
        builder.Property(x => x.Name).HasColumnName("name").IsRequired();
        builder.Property(x => x.ConnectionString).HasColumnName("connection_string").IsRequired();
        builder.Property(x => x.SubUrl).HasColumnName("sub_url").IsRequired();
        builder.Property(x => x.Status).HasColumnName("status").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();

        builder.HasIndex(x => x.IdExternal)
            .HasDatabaseName("ux_connections_id_external")
            .IsUnique();
        builder.HasIndex(x => x.Name)
            .HasDatabaseName("ux_connections_name")
            .IsUnique();

        builder.HasIndex(x => x.SubUrl)
            .HasDatabaseName("ux_connections_sub_url")
            .IsUnique();

    }
}