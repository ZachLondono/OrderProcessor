using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Orders.Loader.Providers.AllmoxyXMLModels;
using ApplicationCore.Features.Orders.Loader.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ApplicationCore.Features.Orders.Loader.XMLValidation;
using ApplicationCore.Features.Orders.Loader.Dialog;

namespace ApplicationCore.Features.Orders.Loader;

internal static class DependencyInjection {

    public static IServiceCollection AddOrderLoading(this IServiceCollection services, IConfiguration configuration) {

        services.Configure<AllmoxyCredentials>(configuration.GetRequiredSection("AllmoxyCredentials"));
        services.Configure<AllmoxyConfiguration>(configuration.GetRequiredSection("AllmoxyConfiguration"));
        services.Configure<ConstructionValues>(configuration.GetRequiredSection("DrawerBoxConstruction"));

        services.AddTransient<IOrderProviderFactory, OrderProviderFactory>();
        services.AddTransient<AllmoxyWebXMLOrderProvider>();
        services.AddTransient<AllmoxyFileXMLOrderProvider>();
        services.AddTransient<AllmoxyClientFactory>();
        services.AddTransient<IXMLValidator, XMLValidator>();
        services.AddTransient<OrderLoadWidgetViewModel>();

        return services;

    }

}