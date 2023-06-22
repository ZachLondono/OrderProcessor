using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Orders.OrderLoading.Providers.AllmoxyXMLModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ApplicationCore.Features.Orders.OrderLoading.Dialog;
using ApplicationCore.Features.Orders.OrderLoading.LoadAllmoxyOrderData.LoadAllmoxyWebOrderData;
using ApplicationCore.Features.Orders.OrderLoading.LoadAllmoxyOrderData.LoadAllmoxyFileOrderData;
using ApplicationCore.Features.Orders.OrderLoading.LoadAllmoxyOrderData.XMLValidation;
using ApplicationCore.Features.Orders.OrderLoading.Models;
using ApplicationCore.Features.Orders.OrderLoading.LoadClosetProOrderData;

namespace ApplicationCore.Features.Orders.OrderLoading;

internal static class DependencyInjection {

    public static IServiceCollection AddOrderLoading(this IServiceCollection services, IConfiguration configuration) {

        services.Configure<AllmoxyCredentials>(configuration.GetRequiredSection("AllmoxyCredentials"));
        services.Configure<AllmoxyConfiguration>(configuration.GetRequiredSection("AllmoxyConfiguration"));
        services.Configure<ConstructionValues>(configuration.GetRequiredSection("DrawerBoxConstruction"));

        services.AddTransient<IOrderProviderFactory, OrderProviderFactory>();
        services.AddTransient<AllmoxyWebXMLOrderProvider>();
        services.AddTransient<AllmoxyFileXMLOrderProvider>();
        services.AddTransient<ClosetProCSVOrderProvider>();
        services.AddTransient<AllmoxyClientFactory>();
        services.AddTransient<IXMLValidator, XMLValidator>();
        services.AddTransient<OrderLoadWidgetViewModel>();

        return services;

    }

}