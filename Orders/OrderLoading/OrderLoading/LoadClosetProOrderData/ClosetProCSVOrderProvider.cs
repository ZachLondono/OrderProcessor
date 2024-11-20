using OrderLoading.ClosetProCSVCutList;
using Domain.Companies.ValueObjects;
using Domain.Orders.Entities;
using Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using static Domain.Companies.CompanyDirectory;
using CompanyCustomer = Domain.Companies.Entities.Customer;
using Domain.Orders.Persistance;
using Domain.Services;
using Domain.Orders.ValueObjects;
using Domain.Orders.Entities.Hardware;
using OrderLoading.ClosetProCSVCutList.PartList;
using OrderLoading.ClosetProCSVCutList.PickList;

namespace OrderLoading.LoadClosetProOrderData;

public abstract class ClosetProCSVOrderProvider : IOrderProvider {

	public IOrderLoadWidgetViewModel? OrderLoadingViewModel { get; set; }

	private readonly ILogger<ClosetProCSVOrderProvider> _logger;
	private readonly ClosetProCSVReader _reader;
	private readonly PartListProcessor _partListProcessor;
	private readonly IFileReader _fileReader;
	private readonly IOrderingDbConnectionFactory _dbConnectionFactory;
	private readonly GetCustomerIdByNameAsync _getCustomerIdByNameAsync;
	private readonly GetCustomerOrderPrefixByIdAsync _getCustomerOrderPrefixByIdAsync;
	private readonly GetCustomerWorkingDirectoryRootByIdAsync _getCustomerWorkingDirectoryRootByIdAsync;
	private readonly InsertCustomerAsync _insertCustomerAsync;
	private readonly GetCustomerByIdAsync _getCustomerByIdAsync;

	public ClosetProCSVOrderProvider(ILogger<ClosetProCSVOrderProvider> logger, ClosetProCSVReader reader, PartListProcessor partListProcessor, IFileReader fileReader, IOrderingDbConnectionFactory dbConnectionFactory, GetCustomerIdByNameAsync getCustomerIdByNameIdAsync, InsertCustomerAsync insertCustomerAsync, GetCustomerOrderPrefixByIdAsync getCustomerOrderPrefixByIdAsync, GetCustomerByIdAsync getCustomerByIdAsync, GetCustomerWorkingDirectoryRootByIdAsync getCustomerWorkingDirectoryRootByIdAsync) {
		_logger = logger;
		_reader = reader;
		_fileReader = fileReader;
		_partListProcessor = partListProcessor;
		_dbConnectionFactory = dbConnectionFactory;
		_getCustomerIdByNameAsync = getCustomerIdByNameIdAsync;
		_insertCustomerAsync = insertCustomerAsync;
		_getCustomerOrderPrefixByIdAsync = getCustomerOrderPrefixByIdAsync;
		_getCustomerByIdAsync = getCustomerByIdAsync;
		_getCustomerWorkingDirectoryRootByIdAsync = getCustomerWorkingDirectoryRootByIdAsync;
	}

	protected abstract Task<string?> GetCSVDataFromSourceAsync(string source);

	public async Task<OrderData?> LoadOrderData(string sourceObj) {

        OrderLoadingSettings settings = GetOrderLoadingSettings(sourceObj);

        var csvData = await GetCSVDataFromSourceAsync(settings.FilePath);

        if (csvData is null) {
            OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, "No order data found");
            return null;
        }

        _reader.OnReadError += (msg) => OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, msg);
        var info = await _reader.ReadCSVData(csvData);

        // TODO: get this info from a configuration file
        var vendorId = Guid.Parse("a81d759d-5b6c-4053-8cec-55a6c94d609e");
        string designerName = info.Header.GetDesignerName();
        var customer = await GetOrCreateCustomer(info.Header.DesignerCompany, designerName);

        string orderNumber = await GetOrderNumber(settings.CustomOrderNumber, customer);
        string orderName = GetOrderName(info.Header.OrderName);
        string? workingDirectoryRoot = await GetWorkingDirectoryRoot(settings.CustomWorkingDirectoryRoot, customer);
        string workingDirectory = await CreateWorkingDirectory(csvData, info.Header.DesignerCompany, orderName, orderNumber, workingDirectoryRoot);

		var pickList = PickListProcessor.ParsePickList(info.PickList);
		var partList = _partListProcessor.ParsePartList(customer.ClosetProSettings, info.Parts, pickList.HardwareSpread);

		IEnumerable<Supply> supplies = [
			.. pickList.Supplies,
			.. partList.Supplies
		];

        var suppliesArray = supplies.Where(s => s.Qty != 0).ToArray();
        Hardware hardware = new(suppliesArray, partList.DrawerSlides, partList.HangingRails);

        List<AdditionalItem> additionalItems = [];

        return new OrderData() {
            VendorId = vendorId,
            CustomerId = customer.Id,
            Name = orderName,
            Number = orderNumber,
            WorkingDirectory = workingDirectory,
            Products = partList.Products.ToList(),
            AdditionalItems = additionalItems,

            OrderDate = DateTime.Today,
            DueDate = null,
            Rush = false,
            Info = [],
            Comment = string.Empty,
            PriceAdjustment = 0M,
            Tax = 0M,
            Billing = new() {
                InvoiceEmail = null,
                PhoneNumber = "",
                Address = new()
            },
            Shipping = new() {
                Contact = designerName,
                Address = new(),
                Method = "Pick Up",
                PhoneNumber = "",
                Price = 0M
            },
            Hardware = hardware
        };

    }

    private static OrderLoadingSettings GetOrderLoadingSettings(string sourceObj) {
        var sourceObjParts = sourceObj.Split('*');

        if (sourceObjParts.Length != 3) {
            throw new InvalidOperationException("Invalid data source");
        }

        string source = sourceObjParts[0];
        string? customOrderNumber = string.IsNullOrWhiteSpace(sourceObjParts[1]) ? null : sourceObjParts[1];
        string? customWorkingDirectoryRoot = string.IsNullOrWhiteSpace(sourceObjParts[2]) ? null : sourceObjParts[2];
        var settings = new OrderLoadingSettings(source, customOrderNumber, customWorkingDirectoryRoot);
        return settings;
    }

    private async Task<string> GetOrderNumber(string? customOrderNumber, CompanyCustomer customer) {
        string orderNumber;
        if (customOrderNumber is null && string.IsNullOrWhiteSpace(customOrderNumber)) {
            orderNumber = await GetNextOrderNumber(customer.Id);
            var orderNumberPrefix = await _getCustomerOrderPrefixByIdAsync(customer.Id) ?? "";
            orderNumber = $"{orderNumberPrefix}{orderNumber}";
        } else {
            orderNumber = customOrderNumber;
        }

        return orderNumber;
    }

    public static string GetOrderName(string orderName) {
        int start = orderName.IndexOf('-');
        if (start != -1) {
            orderName = orderName[(start + 1)..];
        }
		return orderName;
    }

    private async Task<string> CreateWorkingDirectory(string csvData, string company, string orderName, string orderNumber, string? customerWorkingDirectoryRoot) {

		string cpDefaultWorkingDirectory = @"R:\Job Scans\ClosetProSoftware"; // TODO: Get base directory from configuration file
		string workingDirectory = Path.Combine((customerWorkingDirectoryRoot ?? cpDefaultWorkingDirectory), _fileReader.RemoveInvalidPathCharacters($"{orderNumber} - {company} - {orderName}", ' '));

		if (TryToCreateWorkingDirectory(workingDirectory, out string? incomingDir) && incomingDir is not null) {
			string dataFile = _fileReader.GetAvailableFileName(incomingDir, "Incoming", ".csv");
			await File.WriteAllTextAsync(dataFile, csvData);
		}

		return workingDirectory;
	}

    private async Task<string?> GetWorkingDirectoryRoot(string? customWorkingDirectoryRoot, CompanyCustomer customer) {
        string? workingDirectoryRoot;
        if (customWorkingDirectoryRoot is null) {
            workingDirectoryRoot = await _getCustomerWorkingDirectoryRootByIdAsync(customer.Id);
        } else {
            workingDirectoryRoot = customWorkingDirectoryRoot;
        }

        return workingDirectoryRoot;

    }

	private bool TryToCreateWorkingDirectory(string workingDirectory, out string? incomingDirectory) {

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
			OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Warning, $"Could not create working directory {workingDirectory} - {ex.Message}");
		}

		return false;

	}

	private async Task<string> GetNextOrderNumber(Guid customerId) {

		using var connection = await _dbConnectionFactory.CreateConnection();

		connection.Open();
		var trx = connection.BeginTransaction();

		try {

			var newNumber = connection.QuerySingleOrDefault<int?>("SELECT number FROM order_numbers WHERE customer_id = @CustomerId;", new {
				CustomerId = customerId
			});

			if (newNumber is null) {
				int initialNumber = 1;
				connection.Execute("INSERT INTO order_numbers (customer_id, number) VALUES (@CustomerId, @InitialNumber);", new {
					CustomerId = customerId,
					InitialNumber = initialNumber
				}, trx);
				newNumber = initialNumber;
			}

			connection.Execute("UPDATE order_numbers SET number = @IncrementedValue WHERE customer_id = @CustomerId", new {
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

	private async Task<CompanyCustomer> GetOrCreateCustomer(string designerCompanyName, string designerName) {

		Guid? customerId = await _getCustomerIdByNameAsync(designerCompanyName);

		if (customerId is Guid id) {

			var customer = await _getCustomerByIdAsync(id);
			if (customer is null) {
				throw new InvalidOperationException("Unable to load customer information");
			}
			return customer;

		} else {

			var contact = new Contact() {
				Name = designerName,
				Email = null,
				Phone = null
			};

			var newCustomer = CompanyCustomer.Create(designerCompanyName, "Pick Up", contact, new(), contact, new());

			await _insertCustomerAsync(newCustomer);

			return newCustomer;

		}

	}

	private static string? CreateSubDirectories(string workingDirectory) {
		var cutListDir = Path.Combine(workingDirectory, "CUTLIST");
		_ = Directory.CreateDirectory(cutListDir);

		var ordersDir = Path.Combine(workingDirectory, "orders");
		_ = Directory.CreateDirectory(ordersDir);

		var incomingDir = Path.Combine(workingDirectory, "incoming");
		return Directory.CreateDirectory(incomingDir).Exists ? incomingDir : null;
	}

	public static bool TryParseMoneyString(string text, out decimal value) {
		return decimal.TryParse(text.Replace("$", ""), out value);
	}

	public record FrontHardware(string Name, Dimension Spread);

	private record OrderLoadingSettings(string FilePath, string? CustomOrderNumber, string? CustomWorkingDirectoryRoot);

}
