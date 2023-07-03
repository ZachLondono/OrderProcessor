using ApplicationCore.Features.Companies.Contracts;
using ApplicationCore.Features.Orders.OrderLoading.LoadClosetProOrderData.Models;
using ApplicationCore.Shared.Data.Ordering;
using ApplicationCore.Shared.Services;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.Orders.OrderLoading.LoadClosetProOrderData.LoadClosetProWebOrderData;

internal class ClosetProWebCSVOrderProvider : ClosetProCSVOrderProvider {

    private readonly ClosetProClientFactory _factory;

    public ClosetProWebCSVOrderProvider(ILogger<ClosetProCSVOrderProvider> logger, ClosetProCSVReader reader, ClosetProPartMapper partMapper, IFileReader fileReader, IOrderingDbConnectionFactory dbConnectionFactory, CompanyDirectory.GetCustomerIdByNameAsync getCustomerIdByNameIdAsync, CompanyDirectory.InsertCustomerAsync insertCustomerAsync, CompanyDirectory.GetCustomerOrderPrefixByIdAsync getCustomerOrderPrefixByIdAsync, ClosetProClientFactory factory)
        : base(logger, reader, partMapper, fileReader, dbConnectionFactory, getCustomerIdByNameIdAsync, insertCustomerAsync, getCustomerOrderPrefixByIdAsync) {
        _factory = factory;
    }

    protected override async Task<string> GetCSVDataFromSourceAsync(string source) {

        if (int.TryParse(source, out int designId)) {

            var client = _factory.CreateClient();

            var data = await client.GetCutListDataAsync(designId);

            if (data is null) {
                throw new InvalidOperationException();
            }

            return data;

        } else {

            throw new NotImplementedException();

        }

    }

}
