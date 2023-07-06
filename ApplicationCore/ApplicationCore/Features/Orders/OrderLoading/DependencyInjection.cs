using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Orders.OrderLoading.Providers.AllmoxyXMLModels;
using ApplicationCore.Features.Orders.OrderLoading.Dialog;
using ApplicationCore.Features.Orders.OrderLoading.LoadAllmoxyOrderData.LoadAllmoxyWebOrderData;
using ApplicationCore.Features.Orders.OrderLoading.LoadAllmoxyOrderData.LoadAllmoxyFileOrderData;
using ApplicationCore.Features.Orders.OrderLoading.LoadAllmoxyOrderData.XMLValidation;
using ApplicationCore.Features.Orders.OrderLoading.Models;
using ApplicationCore.Features.Orders.OrderLoading.LoadClosetProOrderData;
using ApplicationCore.Features.Orders.OrderLoading.LoadClosetProOrderData.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using ApplicationCore.Features.Orders.OrderLoading.LoadClosetProOrderData.LoadClosetProFileOrderData;
using ApplicationCore.Features.Orders.OrderLoading.LoadClosetProOrderData.LoadClosetProWebOrderData;

namespace ApplicationCore.Features.Orders.OrderLoading;

internal static class DependencyInjection {

    public static IServiceCollection AddOrderLoading(this IServiceCollection services, IConfiguration configuration) {

        services.Configure<ClosetProSoftwareCredentials>(configuration.GetRequiredSection("ClosetProSoftwareCredentials"));
        services.Configure<AllmoxyCredentials>(configuration.GetRequiredSection("AllmoxyCredentials"));
        services.Configure<AllmoxyConfiguration>(configuration.GetRequiredSection("AllmoxyConfiguration"));
        services.Configure<ConstructionValues>(configuration.GetRequiredSection("DrawerBoxConstruction"));

        services.AddTransient<IOrderProviderFactory, OrderProviderFactory>();
        services.AddTransient<AllmoxyWebXMLOrderProvider>();
        services.AddTransient<AllmoxyFileXMLOrderProvider>();
        services.AddTransient<ClosetProFileCSVOrderProvider>();
        services.AddTransient<ClosetProWebCSVOrderProvider>();
        services.AddTransient<ClosetProCSVReader>();
        services.AddTransient<ClosetProPartMapper>();
        services.AddTransient<ClosetProClientFactory>();
        services.AddTransient<AllmoxyClientFactory>();
        services.AddTransient<IXMLValidator, XMLValidator>();
        services.AddTransient<OrderLoadWidgetViewModel>();

        return services;

    }

}