using System.Data;
using AccessFlow.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using AccessFlow.Infrastructure.Persistence.Data;

namespace AccessFlow.Infrastructure.Persistence.Transactions;

public class EFTransactionManager(AppDbContext dbContext) : ITransactionManager
{
    private readonly AppDbContext _dbContext = dbContext;
    public async Task<T> ExecuteInTransactionAsync<T>(Func<CancellationToken, Task<T>> action,
        CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(
            IsolationLevel.ReadCommitted,
            cancellationToken);

        try
        {
            var result = await action(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }
    public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action,
        CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(
            IsolationLevel.ReadCommitted,
            cancellationToken);

        try
        {
            await action(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }
}