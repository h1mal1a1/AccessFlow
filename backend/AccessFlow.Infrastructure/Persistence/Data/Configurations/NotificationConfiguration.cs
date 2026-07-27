using Microsoft.EntityFrameworkCore;
using AccessFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccessFlow.Infrastructure.Persistence.Data.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {

    }
}