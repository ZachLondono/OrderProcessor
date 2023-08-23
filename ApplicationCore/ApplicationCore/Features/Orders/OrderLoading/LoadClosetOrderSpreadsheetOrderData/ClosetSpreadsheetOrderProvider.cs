using ApplicationCore.Features.Orders.OrderLoading.Dialog;
using ApplicationCore.Features.Orders.OrderLoading.LoadClosetOrderSpreadsheetOrderData.Models;
using ApplicationCore.Features.Orders.OrderLoading.Models;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Shared.Data.Ordering;
using ApplicationCore.Shared.Services;
using Dapper;
using Microsoft.Office.Interop.Excel;
using static ApplicationCore.Features.Companies.Contracts.CompanyDirectory;

namespace ApplicationCore.Features.Orders.OrderLoading.LoadClosetOrderSpreadsheetOrderData;

public class ClosetSpreadsheetOrderProvider : IOrderProvider {

	public IOrderLoadWidgetViewModel? OrderLoadingViewModel { get; set; }

    private readonly IFileReader _fileReader;
    private readonly GetCustomerOrderPrefixByIdAsync _getCustomerOrderPrefixByIdAsync;
    private readonly GetCustomerWorkingDirectoryRootByIdAsync _getCustomerWorkingDirectoryRootByIdAsync;
    private readonly GetCustomerIdByNameAsync _getCustomerIdByNameAsync;
    private readonly IOrderingDbConnectionFactory _dbConnectionFactory; // TODO: don't reference this directly, create a delegate like the ones above

	public ClosetSpreadsheetOrderProvider(IFileReader fileReader, GetCustomerOrderPrefixByIdAsync getCustomerOrderPrefixByIdAsync, GetCustomerWorkingDirectoryRootByIdAsync getCustomerWorkingDirectoryRootByIdAsync, IOrderingDbConnectionFactory dbConnectionFactory, GetCustomerIdByNameAsync getCustomerIdByNameAsync) {
		_fileReader = fileReader;
		_getCustomerOrderPrefixByIdAsync = getCustomerOrderPrefixByIdAsync;
		_getCustomerWorkingDirectoryRootByIdAsync = getCustomerWorkingDirectoryRootByIdAsync;
		_dbConnectionFactory = dbConnectionFactory;
		_getCustomerIdByNameAsync = getCustomerIdByNameAsync;
	}

	public async Task<OrderData?> LoadOrderData(string source) {

        if (!_fileReader.DoesFileExist(source)) {
            OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, "Could not access given filepath");
            return null;
        }

        var extension = Path.GetExtension(source);
        if (extension is null || extension != ".xlsx" && extension != ".xlsm") {
            OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, "Given filepath is not an excel document");
            return null;
        }

        Microsoft.Office.Interop.Excel.Application? app = null;
        Workbook? workbook = null;

        try {

            app = new() {
                DisplayAlerts = false,
                Visible = false
            };

            workbook = app.Workbooks.Open(source, ReadOnly: true);
			if (!TryGetWorksheet(workbook, "Cover", out Worksheet? coverSheet) || coverSheet is null) return null;
			if (!TryGetWorksheet(workbook, "Closet Parts", out Worksheet? closetPartSheet) || closetPartSheet is null) return null;
			if (!TryGetWorksheet(workbook, "Corner Shelves", out Worksheet? cornerShelfSheet) || cornerShelfSheet is null) return null;
			if (!TryGetWorksheet(workbook, "Zargen", out Worksheet? zargenSheet) || zargenSheet is null) return null;
			if (!TryGetWorksheet(workbook, "Dovetail", out Worksheet? dovetailSheet) || dovetailSheet is null) return null;
			if (!TryGetWorksheet(workbook, "Melamine DB", out Worksheet? melaDbSheet) || melaDbSheet is null) return null;
			if (!TryGetWorksheet(workbook, "MDF Fronts", out Worksheet? mdfFrontSheet) || mdfFrontSheet is null) return null;

            var cover = Cover.ReadFromWorksheet(coverSheet);

            Guid? customerId = await _getCustomerIdByNameAsync(cover.CustomerName);

            if (customerId is null) {
                OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, $"Could not find customer");
                return null;
            }

            string orderNumber;
            //if (customOrderNumber is null && string.IsNullOrWhiteSpace(customOrderNumber)) {
                orderNumber = await GetNextOrderNumber((Guid) customerId);
                var orderNumberPrefix = await _getCustomerOrderPrefixByIdAsync((Guid) customerId) ?? throw new InvalidOperationException("Could not get customer data");
                orderNumber = $"{orderNumberPrefix}{orderNumber}";
            //} else {
            //    orderNumber = customOrderNumber;
            //}

            string? workingDirectoryRoot = await _getCustomerWorkingDirectoryRootByIdAsync((Guid) customerId);
            if (workingDirectoryRoot is null) {
                OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, $"Could not find customer working directory root");
                return null;
            }
            string workingDirectory = Path.Combine(workingDirectoryRoot, _fileReader.RemoveInvalidPathCharacters($"{orderNumber} {cover.JobName}", ' '));
            if (!TryToCreateWorkingDirectory(workingDirectory)) {
                OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, $"Could not create working directory");
                return null;
            }

            var closetParts = LoadItemsFromWorksheet<ClosetPart>(closetPartSheet);
            var cornerShelves = LoadItemsFromWorksheet<CornerShelf>(cornerShelfSheet);
            var zargens = LoadItemsFromWorksheet<Zargen>(zargenSheet);

            var dovetailDBHeader = DovetailDBHeader.ReadFromWorksheet(dovetailSheet);
			var dovetailDBs = LoadItemsFromWorksheet<DovetailDB>(dovetailSheet);

            var melamineDBHeader = MelamineDBHeader.ReadFromWorksheet(melaDbSheet);
			var melamineDBs = LoadItemsFromWorksheet<MelamineDB>(melaDbSheet);

            var mdfFrontHeader = MDFFrontHeader.ReadFromWorksheet(mdfFrontSheet);
            var mdfFronts = LoadItemsFromWorksheet<MDFFront>(mdfFrontSheet);

            List<IProduct> products = new();
            var billing = new BillingInfo() {
                InvoiceEmail = null,
                PhoneNumber = "",
                Address = new()
            };

            ShippingInfo shipping = new() {
                Contact = "",
                Method = "",
                PhoneNumber = "",
                Price = 0M,
                Address = new Address() {
                    Line1 = "",
                    Line2 = "",
                    Line3 = "",
                    City = "",
                    State = "",
                    Zip = "",
                    Country = ""
                }
            };

            var info = new Dictionary<string, string>();
            var vendorId = Guid.Parse("579badff-4579-481d-98cf-0012eb2cc75e");

            return new() {
                Number = orderNumber,
                Name = cover.JobName,
                WorkingDirectory = workingDirectory,                                         // TODO: Get default working directory from configuration file
                Comment = cover.SpecialRequirements,
                Shipping = shipping,
                Billing = billing,
                Tax = 0M,
                PriceAdjustment = 0M,
                OrderDate = cover.OrderDate,
                CustomerId = (Guid) customerId,
                VendorId = vendorId,
                AdditionalItems = new(),
                Products = products,
                Rush = false,
                Info = info
            };

        } catch (Exception ex) {

            OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, $"Error occurred while reading order from workbook {ex}");

        } finally {

            workbook?.Close(SaveChanges: false);
            app?.Quit();

            // Clean up COM objects, calling these twice ensures it is fully cleaned up.
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();

        }

        return null;

	}
    
	private IEnumerable<T> LoadItemsFromWorksheet<T>(Worksheet worksheet) where T : IWorksheetReadable<T>, new() {

		List<T> items = new();

		int row = T.FirstRow;
		while (true) {

			var lineVal = worksheet.Range[$"B{row}"].Value2;
			if (lineVal is null || lineVal.ToString() == "") {
				break;
			}

			try {
				items.Add(T.ReadFromWorksheet(worksheet, row));
			} catch (Exception ex) {
				OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, $"Error reading item at row {row}");
				// TODO: log exception
			}

			row += T.RowStep;

		}

		return items;

	}

	private bool TryGetWorksheet(Workbook workbook, string name,  out Worksheet? worksheet) {
        worksheet = (Worksheet?)workbook.Sheets[name];
        if (worksheet is null) {
            OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, $"Could not find sheet '{name}' in workbook");
            return false;
        }
        return true;
    }

    private bool TryToCreateWorkingDirectory(string workingDirectory) {

        workingDirectory = workingDirectory.Trim();

        if (Directory.Exists(workingDirectory)) {
            return true;
        }

        try {
            var dirInfo = Directory.CreateDirectory(workingDirectory);
            return dirInfo.Exists;
        } catch (Exception ex) {
            OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Warning, $"Could not create working directory {workingDirectory} - {ex.Message}");
        }

        return false;

    }

    private async Task<string> GetNextOrderNumber(Guid customerId) {

        using var connection = await _dbConnectionFactory.CreateConnection();

        connection.Open();
        var trx = connection.BeginTransaction();

        try {

            var newNumber = await connection.QuerySingleOrDefaultAsync<int?>("SELECT number FROM order_numbers WHERE customer_id = @CustomerId;", new {
                CustomerId = customerId
            });

            if (newNumber is null) {
                int initialNumber = 1;
                await connection.ExecuteAsync("INSERT INTO order_numbers (customer_id, number) VALUES (@CustomerId, @InitialNumber);", new {
                    CustomerId = customerId,
                    InitialNumber = initialNumber
                }, trx);
                newNumber = initialNumber;
            }

            await connection.ExecuteAsync("UPDATE order_numbers SET number = @IncrementedValue WHERE customer_id = @CustomerId", new {
                CustomerId = customerId,
                IncrementedValue = newNumber + 1
            });

            trx.Commit();

            return newNumber?.ToString() ?? "0";

        } catch {
            trx.Rollback();
            throw;
        } finally {
            connection.Close();
        }

    }

}
