using Microsoft.EntityFrameworkCore;
using AccessFlow.Domain.Entities;
using AccessFlow.Domain.Constants;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccessFlow.Infrastructure.Persistence.Data.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .UseIdentityByDefaultColumn();

        builder.HasOne(x => x.Client)
            .WithMany(x => x.Notifications)
            .HasForeignKey(x => x.IdClient)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.IdClient)
            .HasColumnName("id_client")
            .IsRequired();
        builder.HasQueryFilter(x => x.Client.Status != ClientStatus.Deleted);
        builder.HasOne(x => x.Connection)
            .WithMany(x => x.Notifications)
            .HasForeignKey(x => x.IdConnection)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.IdConnection)
            .HasColumnName("id_connection")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .IsRequired();

        builder.Property(x => x.SendAt)
            .HasColumnName("send_at");

        builder.Property(x => x.Error)
            .HasColumnName("error");
    }
}