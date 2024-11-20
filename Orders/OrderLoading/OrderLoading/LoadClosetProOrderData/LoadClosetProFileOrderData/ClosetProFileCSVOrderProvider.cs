using OrderLoading.ClosetProCSVCutList;
using Domain.Companies;
using Microsoft.Extensions.Logging;
using Domain.Orders.Persistance;
using Domain.Services;
using OrderLoading.ClosetProCSVCutList.PartList;

namespace OrderLoading.LoadClosetProOrderData.LoadClosetProFileOrderData;

public class ClosetProFileCSVOrderProvider : ClosetProCSVOrderProvider {

	private readonly IFileReader _fileReader;

	public ClosetProFileCSVOrderProvider(ILogger<ClosetProCSVOrderProvider> logger, ClosetProCSVReader reader, PartListProcessor partListProcessor,
										IFileReader fileReader, IOrderingDbConnectionFactory dbConnectionFactory,
										CompanyDirectory.GetCustomerIdByNameAsync getCustomerIdByNameIdAsync, CompanyDirectory.InsertCustomerAsync insertCustomerAsync, CompanyDirectory.GetCustomerOrderPrefixByIdAsync getCustomerOrderPrefixByIdAsync, CompanyDirectory.GetCustomerByIdAsync getCustomerByIdAsync, CompanyDirectory.GetCustomerWorkingDirectoryRootByIdAsync getCustomerWorkingDirectoryRootByIdAsync)
									 : base(logger, reader, partListProcessor, fileReader, dbConnectionFactory, getCustomerIdByNameIdAsync, insertCustomerAsync, getCustomerOrderPrefixByIdAsync, getCustomerByIdAsync, getCustomerWorkingDirectoryRootByIdAsync) {
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
