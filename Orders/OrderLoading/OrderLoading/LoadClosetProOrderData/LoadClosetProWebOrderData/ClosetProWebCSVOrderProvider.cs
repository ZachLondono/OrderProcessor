using OrderLoading.ClosetProCSVCutList;
using Domain.Companies;
using Microsoft.Extensions.Logging;
using Domain.Orders.Persistance;
using Domain.Services;
using OrderLoading.ClosetProCSVCutList.PartList;

namespace OrderLoading.LoadClosetProOrderData.LoadClosetProWebOrderData;

public class ClosetProWebCSVOrderProvider : ClosetProCSVOrderProvider {

	private readonly ClosetProClientFactory _factory;

	public ClosetProWebCSVOrderProvider(ILogger<ClosetProCSVOrderProvider> logger, ClosetProCSVReader reader, PartListProcessor partListProcessor, IFileReader fileReader, IOrderingDbConnectionFactory dbConnectionFactory, CompanyDirectory.GetCustomerIdByNameAsync getCustomerIdByNameIdAsync, CompanyDirectory.InsertCustomerAsync insertCustomerAsync, CompanyDirectory.GetCustomerOrderPrefixByIdAsync getCustomerOrderPrefixByIdAsync, ClosetProClientFactory factory, CompanyDirectory.GetCustomerByIdAsync getCustomerByIdAsync, CompanyDirectory.GetCustomerWorkingDirectoryRootByIdAsync getCustomerWorkingDirectoryRootByIdAsync)
		: base(logger, reader, partListProcessor, fileReader, dbConnectionFactory, getCustomerIdByNameIdAsync, insertCustomerAsync, getCustomerOrderPrefixByIdAsync, getCustomerByIdAsync, getCustomerWorkingDirectoryRootByIdAsync) {
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
