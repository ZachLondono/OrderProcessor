using Microsoft.Extensions.DependencyInjection;

namespace Domain;

public static class DependencyInjection {

    public static IServiceCollection AddDomainServices(this IServiceCollection services)
        => services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

}
