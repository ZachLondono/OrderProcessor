using ApplicationCore.Features.Orders.Loader.Providers.AllmoxyXMLModels;
using ApplicationCore.Features.Orders.Loader.XMLValidation;
using ApplicationCore.Features.Orders.Loader.Dialog;
using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using static ApplicationCore.Features.Companies.Contracts.CompanyDirectory;
using Microsoft.Extensions.Options;
using ApplicationCore.Features.Shared.Services;

namespace ApplicationCore.Features.Orders.Loader.Providers;

internal class AllmoxyWebXMLOrderProvider : AllmoxyXMLOrderProvider {

    private readonly AllmoxyClientFactory _clientfactory;

    public AllmoxyWebXMLOrderProvider(IOptions<AllmoxyConfiguration> configuration, AllmoxyClientFactory clientfactory, IXMLValidator validator, ProductBuilderFactory builderFactory, GetCustomerIdByAllmoxyIdAsync getCustomerIdByAllmoxyIdAsync, InsertCustomerAsync insertCustomerAsync, IFileReader fileReader)
        : base(configuration, validator, builderFactory, getCustomerIdByAllmoxyIdAsync, insertCustomerAsync, fileReader) {
        _clientfactory = clientfactory;
    }

    protected override async Task<string> GetExportXMLFromSource(string source) {
        try {
            string exportXML = await _clientfactory.CreateClient().GetExportAsync(source, 6);
            return exportXML;
        } catch (Exception ex) {
            OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, $"Could not load order data from Allmoxy: {ex.Message}");
            return string.Empty;
        }
    }
}
