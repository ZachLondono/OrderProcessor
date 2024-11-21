using Domain.Orders.ValueObjects;
using OrderLoading.LoadAllmoxyOrderData.AllmoxyXMLModels;
using System.Xml.Serialization;
using System.Text;
using Domain.Orders.Builders;
using System.Xml.Schema;
using static Domain.Companies.CompanyDirectory;
using Address = Domain.Orders.ValueObjects.Address;
using CompanyAddress = Domain.Companies.ValueObjects.Address;
using CompanyCustomer = Domain.Companies.Entities.Customer;
using Domain.Companies.ValueObjects;
using System.Xml;
using Microsoft.Extensions.Options;
using OrderLoading.LoadAllmoxyOrderData.XMLValidation;
using Microsoft.Extensions.Logging;
using Domain.Orders.Entities;
using Domain.Orders.Entities.Products;
using Domain.Services;
using Domain.Extensions;
using Domain.Orders.Entities.Hardware;
using Domain.Orders.Entities.Products.Closets;
using Domain.Services.WorkingDirectory;

namespace OrderLoading.LoadAllmoxyOrderData;

public abstract class AllmoxyXMLOrderProvider : IOrderProvider {

	private readonly AllmoxyConfiguration _configuration;
	private readonly IXMLValidator _validator;
	private readonly ProductBuilderFactory _builderFactory;
	private readonly GetCustomerIdByAllmoxyIdAsync _getCustomerIdByAllmoxyIdAsync;
	private readonly InsertCustomerAsync _insertCustomerAsync;
	private readonly IFileReader _fileReader;
	private readonly GetCustomerOrderPrefixByIdAsync _getCustomerOrderPrefixByIdAsync;
	private readonly GetCustomerWorkingDirectoryRootByIdAsync _getCustomerWorkingDirectoryRootByIdAsync;
	private readonly ILogger<AllmoxyXMLOrderProvider> _logger;

	public IOrderLoadWidgetViewModel? OrderLoadingViewModel { get; set; }

	public AllmoxyXMLOrderProvider(IOptions<AllmoxyConfiguration> configuration, IXMLValidator validator, ProductBuilderFactory builderFactory, GetCustomerIdByAllmoxyIdAsync getCustomerIdByAllmoxyIdAsync, InsertCustomerAsync insertCustomerAsync, IFileReader fileReader,
									GetCustomerOrderPrefixByIdAsync getCustomerOrderPrefixByIdAsync, GetCustomerWorkingDirectoryRootByIdAsync getCustomerWorkingDirectoryRootByIdAsync,
									ILogger<AllmoxyXMLOrderProvider> logger) {
		_configuration = configuration.Value;
		_validator = validator;
		_builderFactory = builderFactory;
		_getCustomerIdByAllmoxyIdAsync = getCustomerIdByAllmoxyIdAsync;
		_insertCustomerAsync = insertCustomerAsync;
		_fileReader = fileReader;
		_getCustomerOrderPrefixByIdAsync = getCustomerOrderPrefixByIdAsync;
		_getCustomerWorkingDirectoryRootByIdAsync = getCustomerWorkingDirectoryRootByIdAsync;
		_logger = logger;
	}

	protected abstract Task<string> GetExportXMLFromSource(string source);

	public async Task<OrderData?> LoadOrderData(string source) {

		var exportXML = await GetExportXMLFromSource(source);

		if (exportXML == string.Empty) {
			OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, "No order data found");
			return null;
		}

		// Validate data
		if (!ValidateData(exportXML)) {
			OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, "Order data was not valid");
			return null;
		}

		// Deserialize data
		OrderModel? data = DeserializeData(exportXML);
		if (data is null) {
			OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, "Could not find order information in given data");
			return null;
		}

		ShippingInfo shipping = new() {
			Contact = data.Shipping.Attn,
			Method = data.Shipping.Method,
			PhoneNumber = "",
			Price = AllmoxyXMLOrderProviderHelpers.StringToMoney(data.Invoice.Shipping),
			Address = new Address() {
				Line1 = data.Shipping.Address.Line1,
				Line2 = data.Shipping.Address.Line2,
				Line3 = data.Shipping.Address.Line3,
				City = data.Shipping.Address.City,
				State = data.Shipping.Address.State,
				Zip = data.Shipping.Address.Zip,
				Country = data.Shipping.Address.Country
			}
		};

		string customerName = data.Customer.Company;

		Guid customerId = await CreateCustomerIfNotExists(data, customerName);

		var info = new Dictionary<string, string>() {
			{ "Notes", data.Note },
			{ "Shipping Attn", data.Shipping.Attn },
			{ "Shipping Instructions", data.Shipping.Instructions },
			{ "Allmoxy Customer Id", data.Customer.CompanyId.ToString() }
		};

		DateTime orderDate = ParseOrderDate(data.OrderDate);

		var billing = new BillingInfo() {
			InvoiceEmail = null,
			PhoneNumber = "",
			Address = new() {
				Line1 = data.Invoice.Address.Line1,
				Line2 = data.Invoice.Address.Line2,
				Line3 = data.Invoice.Address.Line3,
				City = data.Invoice.Address.City,
				State = data.Invoice.Address.State,
				Zip = data.Invoice.Address.Zip,
				Country = data.Invoice.Address.Country
			}
		};

		List<IProduct> products = [];
		List<AdditionalItem> items = [];
		data.Products.ForEach(c => MapAndAddProduct(c, products, items));

		// 'Folder 1' is the default allmoxy folder name, if that is the only folder name than it can be removed
		var roomNames = products.Select(p => p.Room).Distinct().ToList();
		if (roomNames.Count == 1 && roomNames.First() == "Folder 1") {
			products.ForEach(p => p.Room = string.Empty);
		}

		string number = data.Number.ToString();
		string? prefix = await _getCustomerOrderPrefixByIdAsync(customerId);
		if (prefix is not null) {
			number = prefix + number;
		}

		string? customerWorkingDirectoryRoot = await _getCustomerWorkingDirectoryRootByIdAsync(customerId);
		string allmoxyDefaultWorkingDirectory = @"R:\Job Scans\Allmoxy"; // TODO: Get base directory from configuration file
		string workingDirectory = Path.Combine((customerWorkingDirectoryRoot ?? allmoxyDefaultWorkingDirectory), _fileReader.RemoveInvalidPathCharacters($"{number} - {data.Customer.Company} - {data.Name}", ' '));
		var wdStructure = WorkingDirectoryStructure.Create(workingDirectory, true);
		await wdStructure.WriteAllTextToIncomingAsync("Allmoxy Export.xml", exportXML, false);

		DateTime? dueDate = null;
		if (DateTime.TryParse(data.Shipping.Date, out DateTime parsedDate)) {
			dueDate = parsedDate;
		}

		var allSupplies = products.OfType<ISupplyContainer>()
									.SelectMany(p => p.GetSupplies())
									.GroupBy(p => p.Description)
									.Select(g => new Supply(Guid.NewGuid(), g.Sum(s => s.Qty), g.Key))
									.ToList();
		var cams = GetCams(products.OfType<ClosetPart>().Where(p => p.InstallCams));
		allSupplies.AddRange(cams);

		var allSlides = products.OfType<IDrawerSlideContainer>()
									.SelectMany(p => p.GetDrawerSlides())
									.GroupBy(p => (p.Length, p.Style))
									.Select(g => new DrawerSlide(Guid.NewGuid(), g.Sum(s => s.Qty), g.Key.Length, g.Key.Style))
									.ToArray();

		var suppliesArray = allSupplies.Where(s => s.Qty > 0).ToArray();

		Hardware hardware = new(suppliesArray, allSlides, []);

		OrderData? order = new() {
			Number = number,
			Name = data.Name,
			WorkingDirectory = workingDirectory,                                         // TODO: Get default working directory from configuration file
			Comment = data.Description,
			Shipping = shipping,
			Billing = billing,
			Tax = AllmoxyXMLOrderProviderHelpers.StringToMoney(data.Invoice.Tax),
			PriceAdjustment = 0M,
			OrderDate = orderDate,
			DueDate = dueDate,
			CustomerId = customerId,
			VendorId = Guid.Parse(_configuration.VendorId),
			AdditionalItems = items,
			Products = products,
			Rush = data.Shipping.Method.Contains("Rush"),
			Info = info,
			Hardware = hardware,
		};

		return order;

	}

	private async Task<Guid> CreateCustomerIfNotExists(OrderModel data, string customerName) {

		int allmoxyCustomerId = data.Customer.CompanyId;
		Guid? customerId = await _getCustomerIdByAllmoxyIdAsync(allmoxyCustomerId);

		if (customerId is Guid id) {
			return id;
		} else {

			var shippingContact = new Contact() {
				Name = data.Shipping.Attn,
				Email = null,
				Phone = null
			};

			var shippingAddress = new CompanyAddress() {
				Line1 = data.Shipping.Address.Line1,
				Line2 = data.Shipping.Address.Line2,
				Line3 = data.Shipping.Address.Line3,
				City = data.Shipping.Address.City,
				State = data.Shipping.Address.State,
				Zip = data.Shipping.Address.Zip,
				Country = data.Shipping.Address.Country
			};

			var billingContact = new Contact();

			var billingAddress = new CompanyAddress();

			var newCustomer = CompanyCustomer.Create(customerName, data.Shipping.Method, shippingContact, shippingAddress, billingContact, billingAddress);

			await _insertCustomerAsync(newCustomer, allmoxyCustomerId);

			return newCustomer.Id;

		}

	}

	public bool ValidateData(string data) {

		try {

			using var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));

			var directory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

			if (directory is null) {
				OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, "Failed to get directory of Allmoxy XML Schema");
				_logger.LogError("Failed to get directory of Allmoxy XML Schema from assembly location {AssemblyLocation}", System.Reflection.Assembly.GetExecutingAssembly().Location);
				return false;
			}

			var fullPath = Path.Combine(directory, _configuration.SchemaFilePath);
			var errors = _validator.ValidateXML(stream, fullPath);

			errors.ForEach(error => {
				string message = error.Exception.Message;
				if (error.Exception is XmlSchemaValidationException schemaEx) {
					message = $"L{schemaEx.LineNumber}P{schemaEx.LinePosition} " + message;
				}
				OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, $"[XML Error] [{error.Severity}] {message}");
			});

			return !errors.Any();

		} catch (XmlSchemaException ex) {

			OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, $"[XML Schema Error] XML schema is not valid L{ex.LineNumber} - {ex.Message}");
			_logger.LogError(ex, "Exception thrown while comparing Allmoxy XML order data against schema");
			return false;

		} catch (XmlException ex) {

			OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, $"[XML Schema Error] XML schema is not valid L{ex.LineNumber} - {ex.Message}");
			_logger.LogError(ex, "Exception thrown while comparing Allmoxy XML order data against schema");
			return false;

		} catch (Exception ex) {

			OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, "Could not verify order data against schema");
			_logger.LogError(ex, "Exception thrown while comparing Allmoxy XML order data against schema");
			return false;

		}

	}

	private DateTime ParseOrderDate(string orderDateStr) {

		if (DateTime.TryParse(orderDateStr, out DateTime orderDate)) {
			return orderDate;
		}

		OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Warning, $"Could not parse order date '{(orderDateStr == "" ? "[BLANK]" : orderDateStr)}'");

		return DateTime.Now;

	}

	private void MapAndAddProduct(ProductOrItemModel data, List<IProduct> products, List<AdditionalItem> items) {

		try {

			var productOrItem = data.CreateProductOrItem(_builderFactory);

			productOrItem.Switch(
				product => {

					if (product.Room == "folder_name") {
						product.Room = string.Empty;
					}
					products.Add(product);

				},
				items.Add);

		} catch (Exception ex) {

			OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, $"Could not load product {ex.Message}");
			_logger.LogError(ex, "Exception thrown while mapping order data to product: {Data}", data);

		}

	}

	private static OrderModel? DeserializeData(string exportXML) {
		var serializer = new XmlSerializer(typeof(OrderModel));
		var reader = new StringReader(exportXML);
		if (serializer.Deserialize(reader) is OrderModel data) {
			return data;
		}
		return null;
	}

	public static IEnumerable<Supply> GetCams(IEnumerable<ClosetPart> closetParts) {

		foreach (var part in closetParts) {

			switch (part.SKU) {

				case "PCDV-1CAM":
				case "PCDV-B-1CAM":
				case "PEDV-1CAM":
				case "PEDV-B-1CAM":
					yield return Supply.RafixCam(2 * part.Qty);
					break;

				case "SF":
				case "SF-R1":
				case "SF-R2":
				case "SF-R3":
				case "SF-R19":
				case "SF-R20":
				case "SF-E":
				case "SF-LED":
				case "SF-R1-LED":
				case "SF-R2-LED":
				case "SF-R3-LED":
				case "SF-R19-LED":
				case "SF-R20-LED":
				case "SF-E-LED":
				case "SF-E-Lock":
				case "SF-Lock":
				case "PCDV-CAM":
				case "PCDV-B-CAM":
				case "PEDV-CAM":
				case "PEDV-B-CAM":
				case "TK-F":
					yield return Supply.RafixCam(4 * part.Qty);
					break;

				case "SFL":
				case "SFL19":
				case "SFL22":
				case "SFL25":
				case "SFD":
				case "SFD19":
				case "SFD22":
				case "SFD25":
					yield return Supply.RafixCam(6 * part.Qty);
					break;

				case "PCDV-1CAM-D":
				case "PCDV-B-1CAM-D":
				case "PEDV-1CAM-D":
				case "PEDV-B-1CAM-D":
					yield return Supply.RafixDoubleCam(2 * part.Qty);
					break;

				case "SF-D":
				case "SF-R1-D":
				case "SF-R2-D":
				case "SF-R3-D":
				case "SF-R19-D":
				case "SF-R20-D":
				case "SF-E-D":
				case "SF-LED-D":
				case "SF-R1-D-LED":
				case "SF-R2-D-LED":
				case "SF-R3-D-LED":
				case "SF-R19-D-LED":
				case "SF-R20-D-LED":
				case "SF-E-Lock-D":
				case "SF-Lock-D":
				case "PCDV-CAM-D":
				case "PCDV-B-CAM-D":
				case "PEDV-CAM-D":
				case "PEDV-B-CAM-D":
				case "TK-FD":
					yield return Supply.RafixDoubleCam(4 * part.Qty);
					break;

				case "SFL-D":
				case "SFL19-D":
				case "SFL22-D":
				case "SFL25-D":
				case "SFD-D":
				case "SFD19-D":
				case "SFD22-D":
				case "SFD25-D":
					yield return Supply.RafixDoubleCam(6 * part.Qty);
					break;

			}

		}

	}

}
