using ApplicationCore.Infrastructure.Bus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ApplicationCore.Shared.Bus;

public static class DependencyInjection {

    public static IServiceCollection AddBus(this IServiceCollection services, IConfiguration configuration) {

        services.AddSingleton<IBus, MediatRBus>();

        return services;

    }

}
