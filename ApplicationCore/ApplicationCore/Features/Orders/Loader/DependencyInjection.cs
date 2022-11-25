using ApplicationCore.Features.ExcelTemplates.Domain;
using ApplicationCore.Features.Labels.Services;
using ApplicationCore.Features.Orders.Complete;
using ApplicationCore.Features.Orders.Domain.ValueObjects;
using ApplicationCore.Features.Orders.Loader.Providers.AllmoxyXMLModels;
using ApplicationCore.Features.Orders.Loader.Providers.RichelieuXMLModels;
using ApplicationCore.Features.Orders.Loader.Providers;
using ApplicationCore.Features.Orders.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ApplicationCore.Features.Orders.Loader;

internal static class DependencyInjection {

    public static IServiceCollection AddOrderLoading(this IServiceCollection services, IConfiguration configuration) {

        services.AddTransient<IOrderProviderFactory, OrderProviderFactory>();
        var allmoxyConfig = configuration.GetRequiredSection("AllmoxyConfiguration").Get<AllmoxyConfiguration>();
        services.AddSingleton<AllmoxyConfiguration>(allmoxyConfig);
        services.AddTransient<AllmoxyXMLOrderProvider>();

        var richelieuConfig = configuration.GetRequiredSection("RichelieuConfiguration").Get<RichelieuConfiguration>();
        services.AddSingleton<RichelieuConfiguration>(richelieuConfig);
        services.AddSingleton<RichelieuXMLOrderProvider>();

        var otConfig = configuration.GetRequiredSection("OTConfiguration").Get<OTConfiguration>();
        services.AddSingleton<OTConfiguration>(otConfig);
        services.AddTransient<OTExcelProvider>();

        var hafeleConfig = configuration.GetRequiredSection("HafeleConfiguration").Get<HafeleConfiguration>();
        services.AddSingleton<HafeleConfiguration>(hafeleConfig);
        services.AddSingleton<HafeleExcelProvider>();

        services.AddTransient<ITemplateFiller, ClosedXMLTemplateFiller>();
        services.AddTransient<IExcelTemplate, ClosedXMLTemplate>();
        services.AddTransient<IExcelTemplateFactory, ExcelTemplateFactory>();
        services.AddTransient<IExcelPrinter, ProcessExcelPrinter>();

        var constructionConfig = configuration.GetRequiredSection("DrawerBoxConstruction").Get<ConstructionValues>();
        services.AddSingleton<ConstructionValues>(constructionConfig);

        services.AddTransient<ILabelPrinterService, DymoLabelPrinterService>();
        services.AddTransient<ILabelTemplateReader, DymoLabelTemplateReader>();

        var invoiceEmailConfig = configuration.GetRequiredSection("Email").Get<EmailConfiguration>();
        services.AddSingleton<EmailConfiguration>(invoiceEmailConfig);

        services.AddOrderLoading();

        return services;

    }

}