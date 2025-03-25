using Domain.Companies;
using Domain.Companies.Entities;
using Domain.Companies.ValueObjects;
using OrderLoading.LoadDoweledDBSpreadsheetOrderData.Models;
using Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;
using Domain.Orders.Entities.Products;
using Domain.Services;
using Domain.Orders.ValueObjects;
using Address = Domain.Companies.ValueObjects.Address;
using static OrderLoading.IOrderProvider;

namespace OrderLoading.LoadDoweledDBSpreadsheetOrderData;

public class DoweledDBSpreadsheetOrderProvider : IOrderProvider {

	public string FilePath { get; set; } = string.Empty;

	private readonly IFileReader _fileReader;
	private readonly ILogger<DoweledDBSpreadsheetOrderProvider> _logger;
	private readonly DoweledDBOrderProviderOptions _options;
	private readonly CompanyDirectory.InsertCustomerAsync _insertCustomerAsync;
	private readonly CompanyDirectory.GetCustomerIdByNameAsync _getCustomerByNamAsync;
	private readonly CompanyDirectory.GetCustomerWorkingDirectoryRootByIdAsync _getCustomerWorkingDirectoryRootByIdAsync;

	public DoweledDBSpreadsheetOrderProvider(IFileReader fileReader, ILogger<DoweledDBSpreadsheetOrderProvider> logger, IOptions<DoweledDBOrderProviderOptions> options, CompanyDirectory.InsertCustomerAsync insertCustomerAsync, CompanyDirectory.GetCustomerIdByNameAsync getCustomerByNamAsync, CompanyDirectory.GetCustomerWorkingDirectoryRootByIdAsync getCustomerWorkingDirectoryRootByIdAsync) {
		_fileReader = fileReader;
		_logger = logger;
		_options = options.Value;
		_insertCustomerAsync = insertCustomerAsync;
		_getCustomerByNamAsync = getCustomerByNamAsync;
		_getCustomerWorkingDirectoryRootByIdAsync = getCustomerWorkingDirectoryRootByIdAsync;
	}

	public async Task<OrderData?> LoadOrderData(LogProgress logProgress) {

		if (string.IsNullOrWhiteSpace(FilePath)) {
			logProgress(MessageSeverity.Error, "No file path provided");
			return null;
		}

		if (!_fileReader.DoesFileExist(FilePath)) {
			logProgress(MessageSeverity.Error, "Could not access given file path");
			return null;
		}

		var extension = Path.GetExtension(FilePath);
		if (extension is null || extension != ".xlsx" && extension != ".xlsm") {
			logProgress(MessageSeverity.Error, "Given file path is not an excel document");
			return null;
		}
		Application? app = null;
		Workbook? workbook = null;
		Workbooks? workbooks = null;

		try {

			app = new() {
				DisplayAlerts = false,
				Visible = false
			};

			workbooks = app.Workbooks;
			workbook = workbooks.Open(FilePath, ReadOnly: true);

			Worksheet? orderSheet = (Worksheet?)workbook.Worksheets["Dowel Order"];
			Worksheet? specSheet = (Worksheet?)workbook.Worksheets["Dowel Specs"];

			if (orderSheet is null) {
				logProgress(MessageSeverity.Error, "Could not find Order sheet in workbook");
				return null;
			}

			if (specSheet is null) {
				logProgress(MessageSeverity.Error, "Could not find Spec sheet in workbook");
				return null;
			}

			var header = Header.ReadFromSheet(orderSheet);
			var customerInfo = CustomerInfo.ReadFromSheet(orderSheet);

			var slideSpecs = UMSlideSpecs.ReadFromSheet(specSheet);
			var bottomSpecs = BottomSpecs.ReadFromSheet(specSheet);
			var frontDrillingSpecs = FrontDrillingSpecs.ReadFromSheet(specSheet);
			var constructionSpecs = ConstructionSpecs.ReadFromSheet(specSheet);

			bool useInches = header.Units == "English (in)";
			bool machineThicknessForUMSlides = slideSpecs.UMSlideMachining;
			var frontBackHeightAdjustment = Dimension.FromMillimeters(constructionSpecs.FrontBackDrop);
			string notches = header.UnderMountNotches;
			var boxes = LoadAllLineItems(orderSheet, logProgress)
											.Select(i => i.CreateDoweledDrawerBoxProduct(useInches, machineThicknessForUMSlides, frontBackHeightAdjustment, notches))
											.Cast<IProduct>()
											.ToList();

			var vendorId = Guid.Parse(_options.VendorIds[header.VendorName]);
			var customerId = await GetCustomerId(header.CustomerName, customerInfo);
			var address = new Domain.Orders.ValueObjects.Address() {
				Line1 = customerInfo.Line1,
				Line2 = customerInfo.Line2,
				Line3 = customerInfo.Line3,
				City = customerInfo.City,
				State = customerInfo.State,
				Zip = customerInfo.Zip,
				Country = "USA"
			};

			var dirRoot = await _getCustomerWorkingDirectoryRootByIdAsync(customerId);
			if (string.IsNullOrWhiteSpace(dirRoot)) dirRoot = null;
			var workingDirectory = CreateWorkingDirectory(FilePath, header.OrderNumber, header.OrderName, header.CustomerName, dirRoot, logProgress);

			return new() {
				VendorId = vendorId,
				CustomerId = customerId,
				Comment = header.SpecialInstructions,
				OrderDate = header.OrderDate,
				DueDate = header.DueDate,
				Rush = false,
				Name = header.OrderName,
				Number = header.OrderNumber,
				PriceAdjustment = 0,
				Tax = 0,
				WorkingDirectory = workingDirectory,
				AdditionalItems = [],
				Info = [],
				Billing = new() {
					PhoneNumber = "",
					InvoiceEmail = customerInfo.Email,
					Address = address
				},
				Shipping = new() {
					Contact = customerInfo.Contact,
					Method = header.ShippingMethod,
					PhoneNumber = "",
					Price = 0,
					Address = address
				},
				Products = boxes,
				Hardware = Hardware.None()
			};

		} catch (Exception ex) {

			logProgress(MessageSeverity.Error, $"Error occurred while reading order from workbook {ex}");
			_logger.LogError(ex, "Exception thrown while loading order from workbook");

		} finally {

			workbook?.Close(SaveChanges: false);
			workbooks?.Close();
			app?.Quit();

			if (workbook is not null) _ = Marshal.ReleaseComObject(workbook);
			if (workbooks is not null) _ = Marshal.ReleaseComObject(workbooks);
			if (app is not null) _ = Marshal.ReleaseComObject(app);

			// Clean up COM objects, calling these twice ensures it is fully cleaned up.
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			GC.WaitForPendingFinalizers();

		}

		return null;

	}

	private IEnumerable<LineItem> LoadAllLineItems(Worksheet worksheet, LogProgress logProgress) {

		List<LineItem> lineItems = new();

		int row = 0;
		while (true) {

			if (!LineItem.DoesRowContainItem(worksheet, row)) {
				break;
			}

			try {

				var line = LineItem.ReadFromSheet(worksheet, row);

				lineItems.Add(line);

			} catch (Exception ex) {

				logProgress(MessageSeverity.Error, $"Error reading line item at line {row}");
				_logger.LogError(ex, "Exception thrown while reading line item from workbook");

			}

			row++;

		}

		return lineItems;

	}

	private async Task<Guid> GetCustomerId(string customerName, CustomerInfo customerInfo) {

		Guid? result = await _getCustomerByNamAsync(customerName);

		if (result is not null) {
			return (Guid)result;
		}

		var address = new Address() {
			Line1 = customerInfo.Line1,
			Line2 = customerInfo.Line2,
			Line3 = customerInfo.Line3,
			City = customerInfo.City,
			State = customerInfo.State,
			Zip = customerInfo.Zip,
			Country = "USA"
		};

		var billingContact = new Contact() {
			Name = customerInfo.Contact,
			Phone = "",
			Email = customerInfo.Email
		};

		var shippingContact = new Contact() {
			Name = customerInfo.Contact,
			Phone = "",
			Email = customerInfo.Email
		};

		var customer = Customer.Create(customerName, string.Empty, shippingContact, address, billingContact, address);
		await _insertCustomerAsync(customer);

		return customer.Id;

	}

	private string CreateWorkingDirectory(string source, string orderNumber, string orderName, string customerName, string? customerWorkingDirectory, LogProgress logProgress) {
		if (string.IsNullOrWhiteSpace(customerWorkingDirectory) && string.IsNullOrWhiteSpace(_options.DefaultWorkingDirectory)) {
            logProgress(MessageSeverity.Error, "No valid working directory root found. Working directory must be created manually and incoming data copied.");
            return string.Empty;
		}
		string workingDirectory = Path.Combine((customerWorkingDirectory ?? _options.DefaultWorkingDirectory), _fileReader.RemoveInvalidPathCharacters($"{orderNumber} - {customerName} - {orderName}", ' '));
		if (TryToCreateWorkingDirectory(workingDirectory, out string? incomingDirectory, logProgress) && incomingDirectory is not null) {
			string dataFile = _fileReader.GetAvailableFileName(incomingDirectory, $"{orderNumber} - Incoming", Path.GetExtension(source));
			File.Copy(source, dataFile);
		}

		return workingDirectory;
	}

	private bool TryToCreateWorkingDirectory(string workingDirectory, out string? incomingDirectory, LogProgress logProgress) {

		workingDirectory = workingDirectory.Trim();

		try {

			if (Directory.Exists(workingDirectory)) {
				incomingDirectory = CreateSubDirectories(workingDirectory);
				return true;
			} else if (Directory.CreateDirectory(workingDirectory).Exists) {
				incomingDirectory = CreateSubDirectories(workingDirectory);
				return true;
			} else {
				incomingDirectory = null;
				return false;
			}

		} catch (Exception ex) {
			incomingDirectory = null;
			logProgress(MessageSeverity.Warning, $"Could not create working directory {workingDirectory} - {ex.Message}");
		}

		return false;

	}

	private static string? CreateSubDirectories(string workingDirectory) {
		var cutListDir = Path.Combine(workingDirectory, "CUTLIST");
		_ = Directory.CreateDirectory(cutListDir);

		var ordersDir = Path.Combine(workingDirectory, "orders");
		_ = Directory.CreateDirectory(ordersDir);

		var incomingDir = Path.Combine(workingDirectory, "incoming");
		return Directory.CreateDirectory(incomingDir).Exists ? incomingDir : null;
	}

}

