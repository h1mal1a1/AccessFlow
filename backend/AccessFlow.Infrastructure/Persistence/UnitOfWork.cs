using AccessFlow.Application.Abstractions;
using AccessFlow.Application.Clients.Exceptions;
using AccessFlow.Application.Connections.Exceptions;
using AccessFlow.Domain.Entities;
using AccessFlow.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace AccessFlow.Infrastructure.Persistence;

public class UnitOfWork(AppDbContext dbContext) : IUnitOfWork
{
    private readonly AppDbContext _dbContext = dbContext;

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation
            })
        {
            if (ex.Entries.Any(x => x.Entity is Client))
                throw new ClientConflictException("Client conflicts with an existing client.");

            if (ex.Entries.Any(x => x.Entity is Connection))
                throw new ConnectionConflictException("Connection conflicts with an existing connection.");

            throw;
        }
    }
}