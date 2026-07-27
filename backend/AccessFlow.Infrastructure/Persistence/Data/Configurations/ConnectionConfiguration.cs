using Microsoft.EntityFrameworkCore;
using AccessFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccessFlow.Infrastructure.Persistence.Data.Configurations;

public class ConnectionConfiguration : IEntityTypeConfiguration<Connection>
{
    public void Configure(EntityTypeBuilder<Connection> builder)
    {

    }
}