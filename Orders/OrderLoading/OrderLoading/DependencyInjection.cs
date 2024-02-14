using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderLoading.ClosetProCSVCutList;
using OrderLoading.LoadAllmoxyOrderData.AllmoxyXMLModels;
using OrderLoading.LoadAllmoxyOrderData.LoadAllmoxyFileOrderData;
using OrderLoading.LoadAllmoxyOrderData.LoadAllmoxyWebOrderData;
using OrderLoading.LoadAllmoxyOrderData.XMLValidation;
using OrderLoading.LoadClosetOrderSpreadsheetOrderData;
using OrderLoading.LoadClosetProOrderData.LoadClosetProFileOrderData;
using OrderLoading.LoadClosetProOrderData.LoadClosetProWebOrderData;
using OrderLoading.LoadDoweledDBSpreadsheetOrderData;
using OrderLoading.LoadDoweledDBSpreadsheetOrderData.Models;
using OrderLoading.LoadHafeleDBSpreadsheetOrderData;

namespace OrderLoading;

public static class DependencyInjection {

	public static IServiceCollection AddOrderLoading(this IServiceCollection services, IConfiguration configuration) {

        return services.Configure<ClosetProSoftwareCredentials>(configuration.GetRequiredSection("ClosetProSoftwareCredentials"))
                        .Configure<AllmoxyCredentials>(configuration.GetRequiredSection("AllmoxyCredentials"))
                        .Configure<AllmoxyConfiguration>(configuration.GetRequiredSection("AllmoxyConfiguration"))
                        .Configure<DoweledDBOrderProviderOptions>(configuration.GetRequiredSection("DoweledDBOrderProviderOptions"))
                        .AddTransient<HafeleDBSpreadSheetOrderProvider>()
                        .AddTransient<AllmoxyWebXMLOrderProvider>()
                        .AddTransient<AllmoxyFileXMLOrderProvider>()
                        .AddTransient<ClosetProFileCSVOrderProvider>()
                        .AddTransient<ClosetProWebCSVOrderProvider>()
                        .AddTransient<ClosetProCSVReader>()
                        .AddTransient<ClosetProPartMapper>()
                        .AddTransient<ClosetProClientFactory>()
                        .AddTransient<DoweledDBSpreadsheetOrderProvider>()
                        .AddTransient<ClosetSpreadsheetOrderProvider>()
                        .AddTransient<AllmoxyClientFactory>()
                        .AddTransient<IXMLValidator, XMLValidator>();

	}

}
