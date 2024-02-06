using ApplicationCore.Features.ClosetProCSVCutList;
using Domain.Companies;
using ApplicationCore.Features.Orders.OrderLoading.Dialog;
using Domain.Orders.Builders;
using ApplicationCore.Shared.Services;
using Microsoft.Extensions.Logging;
using Domain.Orders.Persistance;

namespace ApplicationCore.Features.Orders.OrderLoading.LoadClosetProOrderData.LoadClosetProFileOrderData;

internal class ClosetProFileCSVOrderProvider : ClosetProCSVOrderProvider {

    private readonly IFileReader _fileReader;

    public ClosetProFileCSVOrderProvider(ILogger<ClosetProCSVOrderProvider> logger, ClosetProCSVReader reader, ClosetProPartMapper partMapper,
                                        IFileReader fileReader, IOrderingDbConnectionFactory dbConnectionFactory,
                                        CompanyDirectory.GetCustomerIdByNameAsync getCustomerIdByNameIdAsync, CompanyDirectory.InsertCustomerAsync insertCustomerAsync, CompanyDirectory.GetCustomerOrderPrefixByIdAsync getCustomerOrderPrefixByIdAsync, CompanyDirectory.GetCustomerByIdAsync getCustomerByIdAsync, CompanyDirectory.GetCustomerWorkingDirectoryRootByIdAsync getCustomerWorkingDirectoryRootByIdAsync, ComponentBuilderFactory componentBuilderFactory)
                                     : base(logger, reader, partMapper, fileReader, dbConnectionFactory, getCustomerIdByNameIdAsync, insertCustomerAsync, getCustomerOrderPrefixByIdAsync, getCustomerByIdAsync, getCustomerWorkingDirectoryRootByIdAsync, componentBuilderFactory) {
        _fileReader = fileReader;
    }


    protected override async Task<string?> GetCSVDataFromSourceAsync(string source) {
        try {
            using var stream = _fileReader.OpenReadFileStream(source);
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        } catch (Exception ex) {
            OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, $"Could not load order data from Closet Pro: {ex.Message}");
            return null;
        }
    }

}
