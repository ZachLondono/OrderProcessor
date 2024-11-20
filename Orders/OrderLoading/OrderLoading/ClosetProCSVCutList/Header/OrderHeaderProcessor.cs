using Domain.Companies.Entities;
using Domain.Companies.ValueObjects;
using Domain.Orders.Persistance;
using Domain.Services;
using OrderLoading.ClosetProCSVCutList.CSVModels;
using static Domain.Companies.CompanyDirectory;

namespace OrderLoading.ClosetProCSVCutList.Header;

public class OrderHeaderProcessor {

	private readonly IFileReader _fileReader;
	private readonly IOrderingDbConnectionFactory _dbConnectionFactory;
	private readonly GetCustomerIdByNameAsync _getCustomerIdByNameAsync;
	private readonly GetCustomerOrderPrefixByIdAsync _getCustomerOrderPrefixByIdAsync;
	private readonly GetCustomerWorkingDirectoryRootByIdAsync _getCustomerWorkingDirectoryRootByIdAsync;
	private readonly InsertCustomerAsync _insertCustomerAsync;
	private readonly GetCustomerByIdAsync _getCustomerByIdAsync;

    public OrderHeaderProcessor(IFileReader fileReader, IOrderingDbConnectionFactory dbConnectionFactory, GetCustomerIdByNameAsync getCustomerIdByNameAsync, GetCustomerOrderPrefixByIdAsync getCustomerOrderPrefixByIdAsync, GetCustomerWorkingDirectoryRootByIdAsync getCustomerWorkingDirectoryRootByIdAsync, InsertCustomerAsync insertCustomerAsync, GetCustomerByIdAsync getCustomerByIdAsync) {
        _fileReader = fileReader;
        _dbConnectionFactory = dbConnectionFactory;
        _getCustomerIdByNameAsync = getCustomerIdByNameAsync;
        _getCustomerOrderPrefixByIdAsync = getCustomerOrderPrefixByIdAsync;
        _getCustomerWorkingDirectoryRootByIdAsync = getCustomerWorkingDirectoryRootByIdAsync;
        _insertCustomerAsync = insertCustomerAsync;
        _getCustomerByIdAsync = getCustomerByIdAsync;
    }

    public async Task<OrderHeaderContents> ParseOrderHeader(OrderHeader header, OrderLoadingSettings settings) {

        string designerName = header.GetDesignerName();
        var customer = await GetOrCreateCustomer(header.DesignerCompany, designerName);

        string orderNumber = await GetOrderNumber(settings.CustomOrderNumber, customer);
        string orderName = GetOrderName(header.OrderName);
        string? workingDirectoryRoot = await GetWorkingDirectoryRoot(settings.CustomWorkingDirectoryRoot, customer);
        string workingDirectory = CreateWorkingDirectory(header.DesignerCompany, orderName, orderNumber, workingDirectoryRoot);

		return new(orderNumber, orderName, workingDirectory, customer);

    }

    public static string GetOrderName(string orderName) {
        int start = orderName.IndexOf('-');
        if (start != -1) {
            orderName = orderName[(start + 1)..];
        }
		return orderName;
    }

    private async Task<string> GetOrderNumber(string? customOrderNumber, Customer customer) {
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

    private string CreateWorkingDirectory(string company, string orderName, string orderNumber, string? customerWorkingDirectoryRoot) {

		string cpDefaultWorkingDirectory = @"R:\Job Scans\ClosetProSoftware"; // TODO: Get base directory from configuration file
		string workingDirectory = Path.Combine((customerWorkingDirectoryRoot ?? cpDefaultWorkingDirectory), _fileReader.RemoveInvalidPathCharacters($"{orderNumber} - {company} - {orderName}", ' '));

		return workingDirectory;
	}

    private async Task<string?> GetWorkingDirectoryRoot(string? customWorkingDirectoryRoot, Customer customer) {
        string? workingDirectoryRoot;
        if (customWorkingDirectoryRoot is null) {
            workingDirectoryRoot = await _getCustomerWorkingDirectoryRootByIdAsync(customer.Id);
        } else {
            workingDirectoryRoot = customWorkingDirectoryRoot;
        }

        return workingDirectoryRoot;

    }

	private async Task<Customer> GetOrCreateCustomer(string designerCompanyName, string designerName) {

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

			var newCustomer = Customer.Create(designerCompanyName, "Pick Up", contact, new(), contact, new());

			await _insertCustomerAsync(newCustomer);

			return newCustomer;

		}

	}

}