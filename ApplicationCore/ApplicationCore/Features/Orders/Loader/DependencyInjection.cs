using ApplicationCore.Features.ExcelTemplates.Domain;
using ApplicationCore.Features.Labels.Services;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Orders.Loader.Providers.AllmoxyXMLModels;
using ApplicationCore.Features.Orders.Loader.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ApplicationCore.Features.Orders.Loader.XMLValidation;

namespace ApplicationCore.Features.Orders.Loader;

internal static class DependencyInjection {

    public static IServiceCollection AddOrderLoading(this IServiceCollection services, IConfiguration configuration) {

        services.AddTransient<IOrderProviderFactory, OrderProviderFactory>();

        var allmoxyCreds = configuration.GetRequiredSection("AllmoxyCredentials").Get<AllmoxyCredentials>();
        services.AddSingleton<AllmoxyCredentials>(allmoxyCreds);

        var allmoxyConfig = configuration.GetRequiredSection("AllmoxyConfiguration").Get<AllmoxyConfiguration>();
        services.AddSingleton<AllmoxyConfiguration>(allmoxyConfig);
        services.AddTransient<AllmoxyXMLOrderProvider>();
        services.AddTransient<AllmoxyClientFactory>();

        services.AddTransient<IXMLValidator, XMLValidator>();

        services.AddTransient<ITemplateFiller, ClosedXMLTemplateFiller>();
        services.AddTransient<IExcelTemplate, ClosedXMLTemplate>();
        services.AddTransient<IExcelTemplateFactory, ExcelTemplateFactory>();
        services.AddTransient<IExcelPrinter, ProcessExcelPrinter>();

        var constructionConfig = configuration.GetRequiredSection("DrawerBoxConstruction").Get<ConstructionValues>();
        services.AddSingleton<ConstructionValues>(constructionConfig);

        services.AddTransient<ILabelPrinterService, DymoLabelPrinterService>();
        services.AddTransient<ILabelTemplateReader, DymoLabelTemplateReader>();

        return services;

    }

}