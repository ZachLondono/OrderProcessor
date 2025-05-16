using Domain.Companies.Entities;
using Domain.Companies.ValueObjects;
using Domain.Orders.Entities.Products;
using Domain.Orders.ValueObjects;
using OrderLoading.LoadHafeleMDFDoorSpreadsheetOrderData.ReadOrderFile;
using static Domain.Companies.CompanyDirectory;
using static OrderLoading.IOrderProvider;
using Address = Domain.Orders.ValueObjects.Address;

namespace OrderLoading.LoadHafeleMDFDoorSpreadsheetOrderData;

public class HafeleMDFDoorOrderProvider : IOrderProvider {

    public HafeleMDFDoorOrderSource? Source { get; set; }  = null;

    private const string _workingDirectoryRoot = @"R:\Door Orders\Hafele\Orders";
    private Guid _vendorId = new Guid("38dc201f-669d-41be-b0f4-adfa4c003a99");
	private readonly GetCustomerIdByNameAsync _getCustomerIdByNameAsync;
	private readonly InsertCustomerAsync _insertCustomerAsync;

    public HafeleMDFDoorOrderProvider(GetCustomerIdByNameAsync getCustomerIdByNameAsync, InsertCustomerAsync insertCustomerAsync) {
        _getCustomerIdByNameAsync = getCustomerIdByNameAsync;
        _insertCustomerAsync = insertCustomerAsync;
    }

    public async Task<OrderData?> LoadOrderData(LogProgress logProgress) {

        if (Source is null) {
            logProgress(MessageSeverity.Error, $"Invalid Hafele MDF Door Order Form Source");
            return null;
        }

        var company = Source.Company;
        var orderNumber = Source.OrderNumber;
        var filePath = Source.FilePath;

        var structure = CreateDirectoryStructure(_workingDirectoryRoot, $"{orderNumber} - {company}", logProgress);
        
        if (structure is null) {
            return null;
        }

        File.Copy(filePath, Path.Combine(structure.IncomingDirectory, $"{orderNumber} - incoming{Path.GetExtension(filePath)}"), true);

        var result = HafeleMDFDoorOrder.Load(filePath);

        foreach (var error in result.Errors) {
            logProgress(MessageSeverity.Error, error);
        }

        foreach (var warning in result.Warnings) {
            logProgress(MessageSeverity.Warning, warning);
        }

        if (result.Data is null) {
            logProgress(MessageSeverity.Error, $"Failed to load order data from '{filePath}'");
            return null;
        }

        var fileData = result.Data;

        var customerId = await GetOrCreateCustomerAsync(result.Data.Options);

        var orderData = MapHafeleOrder(orderNumber, customerId, _vendorId, fileData, structure.WorkingDirectory);

        return orderData;

    }

    private static DirectoryStructure? CreateDirectoryStructure(string workingDirectoryRoot, string workingDirectoryName, LogProgress logProgress) {

        string workingDirectory = Path.Combine(_workingDirectoryRoot, workingDirectoryName);
        var dirInfo = Directory.CreateDirectory(workingDirectory);
        if (!dirInfo.Exists) {
            logProgress(MessageSeverity.Error, $"Failed to create directory '{workingDirectory}'");
            return null;
        }

        string cutListDir = Path.Combine(workingDirectory, "CUTLIST");
        _ = Directory.CreateDirectory(cutListDir);
        string incomingDir = Path.Combine(workingDirectory, "incoming");
        _ = Directory.CreateDirectory(incomingDir);
        string ordersDir = Path.Combine(workingDirectory, "orders");
        _ = Directory.CreateDirectory(ordersDir);

        return new DirectoryStructure(workingDirectory, incomingDir, ordersDir, cutListDir );

    }

    private OrderData MapHafeleOrder(string orderNumber, Guid customerId, Guid vendorId, HafeleMDFDoorOrder data, string workingDirectory) {

        if (!data.Data.MaterialThicknessesByName.TryGetValue(data.Options.Material, out var materialThickness)) {
            throw new InvalidOperationException($"Material '{data.Options.Material}' not found in material thicknesses look up");
        }

        var products = new List<IProduct>(data.GetProducts());

        var orderedDate = data.Options.Date;
        var dueDate = data.Options.GetDueDate();

        var isRush = IsRush(data.Options.ProductionTime);

        var info = new Dictionary<string, string> {
            { "Customer", data.Options.Company },
            { "Account #", data.Options.AccountNumber },
            { "Customer Email", data.Options.Email },
            { "Customer PO", data.Options.PurchaseOrder },
            { "Customer Job Name", data.Options.JobName },
            { "Hafele PO", data.Options.HafelePO },
            { "Hafele Order #", data.Options.HafeleOrderNumber },
            { "Tracking #", data.Options.TrackingNumber },
        };

        return new OrderData() {
            Number = orderNumber,
            Name = data.Options.JobName,
            Comment = data.Options.OrderComments,
            Tax = 0,
            PriceAdjustment = 0,
            Shipping = new ShippingInfo() {
                Method = data.Options.Delivery,
                Contact = data.Options.Contact,
                Price = 0,
                PhoneNumber = data.Options.Phone,
                Address = new Address() {
                    Line1 = data.Options.AddressLine1,
                    Line2 = data.Options.AddressLine2,
                    City = data.Options.City,
                    State = data.Options.State,
                    Zip = data.Options.Zip,
                    Country = "USA"
                }
            },
            Billing = new BillingInfo() {
                Address = new Address(),
                PhoneNumber = "",
                InvoiceEmail = ""
            },
            OrderDate = orderedDate,
            DueDate = dueDate,
            CustomerId = customerId,
            VendorId = vendorId,
            Rush = isRush,
            Info = info,
            Products = products,
            Hardware = Hardware.None(),
            AdditionalItems = [],
            WorkingDirectory = workingDirectory
        };

    }

    private static bool IsRush(string leadTime) => leadTime switch {
        "Standard 10 day" => false,
        "5 Day Rush" => true,
        _ => throw new InvalidOperationException($"Unexpected lead time - '{leadTime}'")
    };

    private async Task<Guid> GetOrCreateCustomerAsync(Options options) {

        Guid? customerId = await _getCustomerIdByNameAsync(options.Company);

        if (customerId is Guid id) {

            return id;

        } else {

            var contact = new Contact() {
                Name = options.Contact,
                Email = options.Email,
                Phone = options.Phone 
            };

            var newCustomer = Customer.Create(options.Company, "", contact, new(), contact, new());

			await _insertCustomerAsync(newCustomer);

            return newCustomer.Id;

        }

    }

    public record HafeleMDFDoorOrderSource(string FilePath, string Company, string OrderNumber);

}