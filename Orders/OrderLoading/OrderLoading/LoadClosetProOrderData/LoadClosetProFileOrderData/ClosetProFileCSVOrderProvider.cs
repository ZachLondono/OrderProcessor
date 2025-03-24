using OrderLoading.ClosetProCSVCutList;
using Domain.Companies;
using Domain.Orders.Builders;
using Domain.Orders.Persistance;
using Domain.Services;
using static OrderLoading.IOrderProvider;

namespace OrderLoading.LoadClosetProOrderData.LoadClosetProFileOrderData;

public class ClosetProFileCSVOrderProvider : ClosetProCSVOrderProvider {

	private readonly IFileReader _fileReader;

	public ClosetProFileCSVOrderProvider(ClosetProCSVReader reader, ClosetProPartMapper partMapper,
										IFileReader fileReader, IOrderingDbConnectionFactory dbConnectionFactory,
										CompanyDirectory.GetCustomerIdByNameAsync getCustomerIdByNameIdAsync, CompanyDirectory.InsertCustomerAsync insertCustomerAsync, CompanyDirectory.GetCustomerOrderPrefixByIdAsync getCustomerOrderPrefixByIdAsync, CompanyDirectory.GetCustomerByIdAsync getCustomerByIdAsync, CompanyDirectory.GetCustomerWorkingDirectoryRootByIdAsync getCustomerWorkingDirectoryRootByIdAsync, ComponentBuilderFactory componentBuilderFactory)
									 : base(reader, partMapper, fileReader, dbConnectionFactory, getCustomerIdByNameIdAsync, insertCustomerAsync, getCustomerOrderPrefixByIdAsync, getCustomerByIdAsync, getCustomerWorkingDirectoryRootByIdAsync, componentBuilderFactory) {
		_fileReader = fileReader;
	}

	protected override async Task<string?> GetCSVDataFromSourceAsync(string source, LogProgress logProgress) {
		try {
			using var stream = _fileReader.OpenReadFileStream(source);
			using var reader = new StreamReader(stream);
			return await reader.ReadToEndAsync();
		} catch (Exception ex) {
			logProgress(MessageSeverity.Error, $"Could not load order data from Closet Pro: {ex.Message}");
			return null;
		}
	}

}
