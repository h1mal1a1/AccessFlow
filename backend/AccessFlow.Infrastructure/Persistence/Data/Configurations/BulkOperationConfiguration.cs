using Microsoft.EntityFrameworkCore;
using AccessFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccessFlow.Infrastructure.Persistence.Data.Configurations;

public class BulkOperationConfiguration : IEntityTypeConfiguration<BulkOperation>
{
    public void Configure(EntityTypeBuilder<BulkOperation> builder)
    {

    }
}