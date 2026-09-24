using AccessFlow.Domain.Entities;

namespace AccessFlow.Application.Abstractions;

public interface IOutboxMessageRepository
{
    Task AddMessageAsync(OutboxMessage message, CancellationToken cancellationToken);
}