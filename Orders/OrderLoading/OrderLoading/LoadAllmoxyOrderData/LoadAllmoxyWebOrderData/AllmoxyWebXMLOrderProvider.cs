using OrderLoading.LoadAllmoxyOrderData.AllmoxyXMLModels;
using Domain.Orders.Builders;
using static Domain.Companies.CompanyDirectory;
using Microsoft.Extensions.Options;
using OrderLoading.LoadAllmoxyOrderData.XMLValidation;
using Microsoft.Extensions.Logging;
using Domain.Services;
using static OrderLoading.IOrderProvider;

namespace OrderLoading.LoadAllmoxyOrderData.LoadAllmoxyWebOrderData;

public class AllmoxyWebXMLOrderProvider : AllmoxyXMLOrderProvider {

	private readonly AllmoxyClientFactory _clientFactory;

	public AllmoxyWebXMLOrderProvider(IOptions<AllmoxyConfiguration> configuration, AllmoxyClientFactory clientFactory, IXMLValidator validator, ProductBuilderFactory builderFactory, GetCustomerIdByAllmoxyIdAsync getCustomerIdByAllmoxyIdAsync, InsertCustomerAsync insertCustomerAsync, IFileReader fileReader,
									GetCustomerOrderPrefixByIdAsync getCustomerOrderPrefixByIdAsync, GetCustomerWorkingDirectoryRootByIdAsync getCustomerWorkingDirectoryRootByIdAsync,
									ILogger<AllmoxyXMLOrderProvider> logger)
		: base(configuration, validator, builderFactory, getCustomerIdByAllmoxyIdAsync, insertCustomerAsync, fileReader, getCustomerOrderPrefixByIdAsync, getCustomerWorkingDirectoryRootByIdAsync, logger) {
		_clientFactory = clientFactory;
	}

	protected override async Task<string> GetExportXML(LogProgress logProgress) {
		try {
			string exportXML = await _clientFactory.CreateClient().GetExportAsync(Source, 6);
			return exportXML;
		} catch (Exception ex) {
			logProgress(MessageSeverity.Error, $"Could not load order data from Allmoxy: {ex.Message}");
			return string.Empty;
		}
	}
}
