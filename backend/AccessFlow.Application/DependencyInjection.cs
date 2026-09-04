using AccessFlow.Application.Clients;
using AccessFlow.Application.Connections;
using Microsoft.Extensions.DependencyInjection;

namespace AccessFlow.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<IConnectionService, ConnectionService>();
        return services;
    }
}