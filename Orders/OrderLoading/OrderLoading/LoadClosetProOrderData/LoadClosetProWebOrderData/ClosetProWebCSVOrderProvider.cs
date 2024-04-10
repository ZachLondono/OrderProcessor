using OrderLoading.ClosetProCSVCutList;
using Domain.Companies;
using Domain.Orders.Builders;
using Microsoft.Extensions.Logging;
using Domain.Orders.Persistance;
using Domain.Services;

namespace OrderLoading.LoadClosetProOrderData.LoadClosetProWebOrderData;

public class ClosetProWebCSVOrderProvider : ClosetProCSVOrderProvider {

	private readonly ClosetProClientFactory _factory;

	public ClosetProWebCSVOrderProvider(ILogger<ClosetProCSVOrderProvider> logger, ClosetProCSVReader reader, ClosetProPartMapper partMapper, IFileReader fileReader, IOrderingDbConnectionFactory dbConnectionFactory, CompanyDirectory.GetCustomerIdByNameAsync getCustomerIdByNameIdAsync, CompanyDirectory.InsertCustomerAsync insertCustomerAsync, CompanyDirectory.GetCustomerOrderPrefixByIdAsync getCustomerOrderPrefixByIdAsync, ClosetProClientFactory factory, CompanyDirectory.GetCustomerByIdAsync getCustomerByIdAsync, CompanyDirectory.GetCustomerWorkingDirectoryRootByIdAsync getCustomerWorkingDirectoryRootByIdAsync, ComponentBuilderFactory componentBuilderFactory)
		: base(logger, reader, partMapper, fileReader, dbConnectionFactory, getCustomerIdByNameIdAsync, insertCustomerAsync, getCustomerOrderPrefixByIdAsync, getCustomerByIdAsync, getCustomerWorkingDirectoryRootByIdAsync, componentBuilderFactory) {
		_factory = factory;
	}

	protected override async Task<string?> GetCSVDataFromSourceAsync(string source) {

		if (int.TryParse(source, out int designId)) {

			var client = _factory.CreateClient();

			client.OnError += (msg) => OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, msg);

			var data = await client.GetCutListDataAsync(designId);

			return data;

		} else {

			OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, $"Invalid design ID format '{source}'. Design ID must be an integer.");
			return null;

		}

	}

}
