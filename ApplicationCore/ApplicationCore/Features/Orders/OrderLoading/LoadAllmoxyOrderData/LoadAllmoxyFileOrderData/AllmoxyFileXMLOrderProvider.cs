using ApplicationCore.Features.Orders.OrderLoading.LoadAllmoxyOrderData.AllmoxyXMLModels;
using ApplicationCore.Features.Orders.OrderLoading.Dialog;
using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using static ApplicationCore.Features.Companies.Contracts.CompanyDirectory;
using Microsoft.Extensions.Options;
using ApplicationCore.Shared.Services;
using ApplicationCore.Features.Orders.OrderLoading.LoadAllmoxyOrderData.XMLValidation;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.Orders.OrderLoading.LoadAllmoxyOrderData.LoadAllmoxyFileOrderData;

internal class AllmoxyFileXMLOrderProvider : AllmoxyXMLOrderProvider {

    private readonly IFileReader _fileReader;

    public AllmoxyFileXMLOrderProvider(IOptions<AllmoxyConfiguration> configuration, IXMLValidator validator, ProductBuilderFactory builderFactory, GetCustomerIdByAllmoxyIdAsync getCustomerIdByAllmoxyIdAsync, InsertCustomerAsync insertCustomerAsync, IFileReader fileReader,
                                        GetCustomerOrderPrefixByIdAsync getCustomerOrderPrefixByIdAsync, GetCustomerWorkingDirectoryRootByIdAsync getCustomerWorkingDirectoryRootByIdAsync,
                                        ILogger<AllmoxyXMLOrderProvider> logger)
        : base(configuration, validator, builderFactory, getCustomerIdByAllmoxyIdAsync, insertCustomerAsync, fileReader, getCustomerOrderPrefixByIdAsync, getCustomerWorkingDirectoryRootByIdAsync, logger) {
        _fileReader = fileReader;
    }

    protected override async Task<string> GetExportXMLFromSource(string source) {
        try {
            string exportXML = await _fileReader.ReadFileContentsAsync(source);
            return exportXML;
        } catch (Exception ex) {
            OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, $"Could not load order data from Allmoxy: {ex.Message}");
            return string.Empty;
        }
    }
}
