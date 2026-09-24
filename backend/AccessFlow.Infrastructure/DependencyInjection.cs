using AccessFlow.Application.Abstractions;
using AccessFlow.Infrastructure.Persistence;
using AccessFlow.Infrastructure.Persistence.Data;
using AccessFlow.Infrastructure.Persistence.Repositories;
using AccessFlow.Infrastructure.Persistence.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AccessFlow.Infrastructure.Vps;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;

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
        services.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();

        services.AddOptions<VpsOptions>()
            .Bind(configuration.GetSection("Vps"))
            .Validate(x => Uri.TryCreate(x.BaseUrl, UriKind.Absolute, out _), "Vps: BaseUrl is invalid.")
            .Validate(x => !string.IsNullOrWhiteSpace(x.ApiToken), "Vps:ApiToken is required.")
            .Validate(x => !string.IsNullOrWhiteSpace(x.InboundRemark), "Vps:InboundRemark is required.")
            .ValidateOnStart();

        services.AddHttpClient<ThreeXUiHelper>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<VpsOptions>>().Value;

            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.ApiToken);
        });
        services.AddTransient<IVpsClient, ThreeXUiClient>();
        return services;
    }
}