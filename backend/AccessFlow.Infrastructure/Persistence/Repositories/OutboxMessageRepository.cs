using AccessFlow.Application.Abstractions;
using AccessFlow.Domain.Entities;
using AccessFlow.Infrastructure.Persistence.Data;

namespace AccessFlow.Infrastructure.Persistence.Repositories;

public class OutboxMessageRepository(AppDbContext dbContext) : IOutboxMessageRepository
{
    private readonly AppDbContext _dbContext = dbContext;

    public async Task AddMessageAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        await _dbContext.OutboxMessages.AddAsync(message, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}