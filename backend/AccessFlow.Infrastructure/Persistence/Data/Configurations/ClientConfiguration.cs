using Microsoft.EntityFrameworkCore;
using AccessFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccessFlow.Infrastructure.Persistence.Data.Configurations;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("clients");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .UseIdentityByDefaultColumn();

        builder.Property(x => x.Email)
            .HasColumnName("email")
            .IsRequired();

        builder.Property(x => x.PhoneNumber)
            .HasColumnName("phone_number")
            .IsRequired();

        builder.Property(x => x.Comment)
            .HasColumnName("comment");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasIndex(x => x.Email)
            .HasDatabaseName("ux_clients_email")
            .IsUnique();

        builder.HasIndex(x => x.PhoneNumber)
            .HasDatabaseName("ux_clients_phone_number")
            .IsUnique();
    }
}