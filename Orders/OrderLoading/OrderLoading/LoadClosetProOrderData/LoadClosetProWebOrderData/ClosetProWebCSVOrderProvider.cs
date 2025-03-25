using OrderLoading.ClosetProCSVCutList;
using Domain.Companies;
using Domain.Orders.Builders;
using Domain.Orders.Persistance;
using Domain.Services;
using static OrderLoading.IOrderProvider;

namespace OrderLoading.LoadClosetProOrderData.LoadClosetProWebOrderData;

public class ClosetProWebCSVOrderProvider : ClosetProCSVOrderProvider {

	private readonly ClosetProClientFactory _factory;

	public ClosetProWebCSVOrderProvider(ClosetProCSVReader reader, ClosetProPartMapper partMapper, IFileReader fileReader, IOrderingDbConnectionFactory dbConnectionFactory, CompanyDirectory.GetCustomerIdByNameAsync getCustomerIdByNameIdAsync, CompanyDirectory.InsertCustomerAsync insertCustomerAsync, CompanyDirectory.GetCustomerOrderPrefixByIdAsync getCustomerOrderPrefixByIdAsync, ClosetProClientFactory factory, CompanyDirectory.GetCustomerByIdAsync getCustomerByIdAsync, CompanyDirectory.GetCustomerWorkingDirectoryRootByIdAsync getCustomerWorkingDirectoryRootByIdAsync, ComponentBuilderFactory componentBuilderFactory)
		: base(reader, partMapper, fileReader, dbConnectionFactory, getCustomerIdByNameIdAsync, insertCustomerAsync, getCustomerOrderPrefixByIdAsync, getCustomerByIdAsync, getCustomerWorkingDirectoryRootByIdAsync, componentBuilderFactory) {
		_factory = factory;
	}

	protected override async Task<string?> GetCSVDataFromSourceAsync(LogProgress logProgress) {

		if (Source is null) {
			logProgress(MessageSeverity.Error, "No source data provided");
			return null;
		}

		if (int.TryParse(Source.OrderId, out int designId)) {

			var client = _factory.CreateClient();

			client.OnError += (msg) => logProgress(MessageSeverity.Error, msg);

			var data = await client.GetCutListDataAsync(designId);

			return data;

		} else {

			logProgress(MessageSeverity.Error, $"Invalid design ID format '{Source.OrderId}'. Design ID must be an integer.");
			return null;

		}

	}

}
