using ApplicationCore.Features.Companies.Contracts;
using ApplicationCore.Features.Orders.OrderLoading.Dialog;
using ApplicationCore.Features.Orders.OrderLoading.LoadClosetProOrderData.Models;
using ApplicationCore.Shared.Data.Ordering;
using ApplicationCore.Shared.Services;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.Orders.OrderLoading.LoadClosetProOrderData.LoadClosetProFileOrderData;

internal class ClosetProFileCSVOrderProvider : ClosetProCSVOrderProvider {

    private readonly IFileReader _fileReader;

    public ClosetProFileCSVOrderProvider(ILogger<ClosetProCSVOrderProvider> logger, ClosetProCSVReader reader, ClosetProPartMapper partMapper,
                                        IFileReader fileReader, IOrderingDbConnectionFactory dbConnectionFactory,
                                        CompanyDirectory.GetCustomerIdByNameAsync getCustomerIdByNameIdAsync, CompanyDirectory.InsertCustomerAsync insertCustomerAsync, CompanyDirectory.GetCustomerOrderPrefixByIdAsync getCustomerOrderPrefixByIdAsync)
                                     : base(logger, reader, partMapper, fileReader, dbConnectionFactory, getCustomerIdByNameIdAsync, insertCustomerAsync, getCustomerOrderPrefixByIdAsync) {
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
