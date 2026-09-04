using AccessFlow.Application.Clients;
using Microsoft.Extensions.DependencyInjection;

namespace AccessFlow.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IClientService, ClientService>();
        return services;
    }
}