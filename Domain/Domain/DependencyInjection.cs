using Domain.Components.ProgressModal;
using Domain.Infrastructure.Bus;
using Domain.Infrastructure.Data;
using Domain.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Domain;

public static class DependencyInjection {

    public static IServiceCollection AddDomainServices(this IServiceCollection services) {

        SqlMapping.AddSqlMaps();

		return services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly))
						.AddTransient<NavigationService>()
						.AddSingleton<IFileReader, FileReader>()
						.AddTransient<ProgressModalViewModel>()
						.AddSingleton<IBus, MediatRBus>();

	}

}
