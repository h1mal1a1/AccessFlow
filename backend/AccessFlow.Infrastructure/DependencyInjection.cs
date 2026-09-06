using AccessFlow.Application.Abstractions;
using AccessFlow.Infrastructure.Persistence;
using AccessFlow.Infrastructure.Persistence.Data;
using AccessFlow.Infrastructure.Persistence.Repositories;
using AccessFlow.Infrastructure.Persistence.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AccessFlow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") ??
            throw new InvalidOperationException("Connection string \'DefaultConnection\' not found");

        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IConnectionRepository, ConnectionRepository>();
        services.AddScoped<ITransactionManager, EFTransactionManager>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }
}