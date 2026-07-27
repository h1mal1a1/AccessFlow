using Microsoft.EntityFrameworkCore;
using AccessFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccessFlow.Infrastructure.Persistence.Data.Configurations;

public class BulkOperationItemConfiguration : IEntityTypeConfiguration<BulkOperationItem>
{
    public void Configure(EntityTypeBuilder<BulkOperationItem> builder)
    {

    }
}