using Domain.Companies.Entities;
using Domain.Companies.ValueObjects;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;
using Microsoft.Extensions.Options;
using Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;
using ExcelApp = Microsoft.Office.Interop.Excel.Application;
using Domain.Orders.Entities.Products;
using Domain.Orders.Entities.Products.DrawerBoxes;
using Domain.Infrastructure.Bus;
using Companies.Customers.Queries;
using Companies.Customers.Commands;
using Companies.Vendors.Queries;
using Domain.Services;
using static OrderLoading.IOrderProvider;

namespace OrderLoading.LoadHafeleDBSpreadsheetOrderData;

public class HafeleDBSpreadSheetOrderProvider : IOrderProvider {

	public string FilePath { get; set; } = string.Empty;

	private readonly HafeleDBOrderProviderSettings _settings;
	private readonly IFileReader _fileReader;
	private readonly IBus _bus;

	public HafeleDBSpreadSheetOrderProvider(IOptions<HafeleDBOrderProviderSettings> options, IFileReader fileReader, IBus bus) {
		_settings = options.Value;
		_fileReader = fileReader;
		_bus = bus;
	}

	public async Task<OrderData?> LoadOrderData(LogProgress logProgress) {

        if (!_fileReader.DoesFileExist(FilePath)) {
            logProgress(MessageSeverity.Error, "Could not access given file path");
            return null;
		}

		var extension = Path.GetExtension(FilePath);
		if (extension is null || extension != ".xlsx") {
			logProgress(MessageSeverity.Error, "Given file path is not an excel document");
			return null;
		}

		ExcelApp? app = null;
		Workbook? workbook = null;
		Workbooks? workbooks = null;

		try {

			app = new() {
				DisplayAlerts = false,
				Visible = false
			};

			workbooks = app.Workbooks;
			workbook = workbooks.Open(FilePath, ReadOnly: true);

			var data = WorkbookOrderData.ReadWorkbook(workbook);

			workbook.Close(SaveChanges: false);
			workbooks.Close();
			app.Quit();
			_ = Marshal.ReleaseComObject(workbook);
			_ = Marshal.ReleaseComObject(workbooks);
			_ = Marshal.ReleaseComObject(app);
			workbook = null;
			workbooks = null;
			app = null;

			if (data is null) {
				logProgress(MessageSeverity.Error, "Could not load order data from workbook");
				return null;
			}

			var (orderData, incomingDirectory) = await MapWorkbookDataToOrderData(data, logProgress);

			if (incomingDirectory is not null) {
				var fileName = Path.GetFileNameWithoutExtension(FilePath);
				var ext = Path.GetExtension(FilePath);
				var newFileName = _fileReader.GetAvailableFileName(incomingDirectory, fileName, ext);
				File.Copy(FilePath, newFileName);
			}

			return orderData;

		} catch (Exception ex) {

			logProgress(MessageSeverity.Error, $"Error occurred while reading order from workbook {ex}");

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

	public async Task<(OrderData, string?)> MapWorkbookDataToOrderData(WorkbookOrderData workbookData, LogProgress logProgress) {

		bool metric = workbookData.GlobalDrawerSpecs.Units.Equals("millimeters", StringComparison.InvariantCultureIgnoreCase);

		var products = MapLineItemsToProduct(workbookData.Items, workbookData.GlobalDrawerSpecs.BoxMaterial, metric).ToList();
		var shipping = new ShippingInfo() {
			Contact = workbookData.ContactInformation.Contact,
			Method = "Delivery",
			PhoneNumber = workbookData.ContactInformation.Phone,
			Price = 0M,
			Address = new() {
				Line1 = workbookData.ContactInformation.Address1,
				Line2 = workbookData.ContactInformation.Address2,
				Line3 = "",
				City = workbookData.ContactInformation.City,
				State = workbookData.ContactInformation.State,
				Zip = workbookData.ContactInformation.Zip,
				Country = "USA"
			}
		};

		var billing = await GetVendorBillingInfo(_settings.VendorId);

		string workingDirectory = Path.Combine(_settings.WorkingDirectoryRoot.Replace('/', '\\'), $"{workbookData.OrderDetails.HafelePO} - {workbookData.OrderDetails.JobName} - {workbookData.ContactInformation.Company}");
		if (!TryToCreateWorkingDirectory(workingDirectory, out string? incomingDirectory, logProgress)) {
			incomingDirectory = null;
		}

		bool rush = workbookData.OrderDetails.ProductionTime.Contains("rush", StringComparison.InvariantCultureIgnoreCase);

		var customerId = await GetCustomerId(workbookData.ContactInformation);

		var orderData = new OrderData() {
			Number = workbookData.OrderDetails.HafelePO,
			Name = workbookData.OrderDetails.JobName,
			Products = products,
			Shipping = shipping,
			Billing = billing,
			DueDate = null,
			CustomerId = customerId ?? Guid.Empty,
			Rush = rush,
			WorkingDirectory = workingDirectory,
			OrderDate = workbookData.OrderDetails.OrderDate,
			VendorId = _settings.VendorId,
			Comment = workbookData.OrderComments,
            Info = new() {
				{ "Hafele PO", workbookData.OrderDetails.HafelePO },
				{ "Hafele Order Number", workbookData.OrderDetails.HafeleOrderNumber },
				{ "Hafele Account Number", workbookData.OrderDetails.AccountNumber },
				{ "Customer PO", workbookData.OrderDetails.PurchaseOrder }
			},
			AdditionalItems = [],
			Tax = 0M,
			PriceAdjustment = 0M,
			Hardware = Hardware.None()
		};

		return (orderData, incomingDirectory);

	}

	public IEnumerable<IProduct> MapLineItemsToProduct(IEnumerable<LineItem> lineItems, string boxMaterialName, bool metric) {

		if (!_settings.MaterialThicknessesMM.TryGetValue(boxMaterialName, out var materialThicknessMM)) {
			throw new InvalidOperationException($"Material thickness not configured for '{boxMaterialName}'");
		}

		var frontBackThickness = Dimension.FromMillimeters(materialThicknessMM);
		var sideThickness = Dimension.FromMillimeters(materialThicknessMM);
		var frontBackHeightAdj = Dimension.FromMillimeters(_settings.FrontBackHeightAdjMM);

		return lineItems.Select(
			item => {

				if (!_settings.MaterialThicknessesMM.TryGetValue(item.BottomMaterial, out var bottomThicknessMM)) {
					throw new InvalidOperationException($"Material thickness not configured for '{item.BottomMaterial}'");
				}

				var bottomThickness = Dimension.FromMillimeters(bottomThicknessMM);

				Func<double, Dimension> dimConvert = metric ? Dimension.FromMillimeters : Dimension.FromInches;

				var frontBack = new DoweledDrawerBoxMaterial(boxMaterialName, frontBackThickness, true);
				var sides = new DoweledDrawerBoxMaterial(boxMaterialName, sideThickness, true);
				var bottom = new DoweledDrawerBoxMaterial(item.BottomMaterial, bottomThickness, true);

				return new DoweledDrawerBoxProduct(
							Guid.NewGuid(),
							item.UnitPrice,
							item.Qty,
							string.Empty,
							item.Line,
							dimConvert(item.Height),
							dimConvert(item.Width),
							dimConvert(item.Depth),
							frontBack,
							frontBack,
							sides,
							bottom,
							false,
							frontBackHeightAdj,
							DoweledDrawerBoxConfig.NO_NOTCH);

			});

	}

	private async Task<Guid?> GetCustomerId(ContactInformation contactInfo) {

		var result = await _bus.Send(new GetCustomerIdByName.Query(contactInfo.Company));

		var customerId = result.Match(
			id => id,
			error => null);

		if (customerId is not null) {
			return (Guid)customerId;
		}

		var billingContact = new Contact() {
			Name = contactInfo.Contact,
			Phone = contactInfo.Phone,
			Email = contactInfo.Email
		};

		var shippingContact = new Contact() {
			Name = contactInfo.Contact,
			Phone = contactInfo.Phone,
			Email = contactInfo.Email
		};

		Domain.Companies.ValueObjects.Address address = new() {

		};

		var customer = Customer.Create(contactInfo.Company, string.Empty, shippingContact, address, billingContact, address);

		_ = await _bus.Send(new InsertCustomer.Command(customer, null));

		return customer.Id;

	}

	private async Task<BillingInfo> GetVendorBillingInfo(Guid vendorId) {

		var response = await _bus.Send(new GetVendorById.Query(vendorId));

		return response.Match(
			vendor => {

				return new BillingInfo() {
					InvoiceEmail = null,
					PhoneNumber = vendor?.Phone ?? "",
					Address = new() {
						Line1 = vendor?.Address.Line1 ?? "",
						Line2 = vendor?.Address.Line2 ?? "",
						Line3 = vendor?.Address.Line3 ?? "",
						City = vendor?.Address.City ?? "",
						State = vendor?.Address.State ?? "",
						Zip = vendor?.Address.Zip ?? "",
						Country = vendor?.Address.Country ?? ""
					}
				};

			},
			error => {

				return new BillingInfo() {
					InvoiceEmail = null,
					PhoneNumber = "",
					Address = new() {
						Line1 = "",
						Line2 = "",
						Line3 = "",
						City = "",
						State = "",
						Zip = "",
						Country = ""
					}
				};

			});

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