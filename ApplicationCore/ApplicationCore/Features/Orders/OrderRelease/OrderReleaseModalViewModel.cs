using ApplicationCore.Features.Companies.Contracts;
using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using Blazored.Modal;

namespace ApplicationCore.Features.Orders.OrderRelease;

public class OrderReleaseModalViewModel {

    public Action? OnPropertyChanged { get; set; }

    public bool IsLoadingConfiguration {
        get => _isLoadingConfiguration;
        set {
            _isLoadingConfiguration = value;
            OnPropertyChanged?.Invoke();
        }
    }

    public bool IsReportLoadingFiles {
        get => _isReportLoadingFiles;
        set {
            _isReportLoadingFiles = value;
            OnPropertyChanged?.Invoke();
        }
    }

    public ReleaseConfiguration Configuration {
        get => _configuration;
        set {
            _configuration = value;
            OnPropertyChanged?.Invoke();
        }
    }

    private bool _isLoadingConfiguration = false;
    private bool _isReportLoadingFiles = false;
    private ReleaseConfiguration _configuration = new();
    private readonly CompanyDirectory.GetVendorByIdAsync _getVendorByIdAsync;
    private readonly CompanyDirectory.GetCustomerByIdAsync _getCustomerByIdAsync;
    private readonly ReleaseService _service;

    public OrderReleaseModalViewModel(CompanyDirectory.GetVendorByIdAsync getVendorByIdAsync, CompanyDirectory.GetCustomerByIdAsync getCustomerByIdAsync, ReleaseService service) {
        _getVendorByIdAsync = getVendorByIdAsync;
        _getCustomerByIdAsync = getCustomerByIdAsync;
        _service = service;
    }

    public async Task LoadConfiguration(List<Order> orders) {

        if (orders.Count == 0) {
            return;
        }

        IsLoadingConfiguration = true;

        string invoiceEmailRecipients = string.Empty;

        var firstOrder = orders.First();

        var vendor = await _getVendorByIdAsync(firstOrder.VendorId);
        if (vendor is null) {
            return;
        }

        invoiceEmailRecipients = vendor.ReleaseProfile.InvoiceEmailRecipients;
        invoiceEmailRecipients = await AddCustomerInvoiceEmailRecipients(firstOrder.CustomerId, invoiceEmailRecipients);

        string workingDirectories = string.Join(';', orders.Select(o => o.WorkingDirectory).Where(s => !string.IsNullOrEmpty(s)));

        string releaseDirectory = @"X:\_CUTLISTS  Incoming";
        if (workingDirectories != string.Empty) {
            releaseDirectory += ";" + workingDirectories;
        }
        string invoiceDirectory = workingDirectories;

        string orderNumbers = string.Join(", ", orders.Select(o => o.Number));

        Configuration = new ReleaseConfiguration() {

            SendReleaseEmail = vendor.ReleaseProfile.SendReleaseEmail,
            IncludeSummaryInEmailBody = false,
            ReleaseEmailRecipients = vendor.ReleaseProfile.ReleaseEmailRecipients,
            GenerateJobSummary = vendor.ReleaseProfile.GenerateJobSummary,
            IncludeProductTablesInSummary = false,
            IncludeSuppliesInSummary = false,
            GeneratePackingList = vendor.ReleaseProfile.GeneratePackingList,
            IncludeInvoiceInRelease = vendor.ReleaseProfile.IncludeInvoice,
            ReleaseFileName = $"{orderNumbers} CUTLIST",
            ReleaseOutputDirectory = releaseDirectory,
            GenerateCNCRelease = false,
            CopyCNCReportToWorkingDirectory = true,

            GenerateInvoice = vendor.ReleaseProfile.GenerateInvoice,
            SendInvoiceEmail = vendor.ReleaseProfile.SendInvoiceEmail,
            InvoiceEmailRecipients = invoiceEmailRecipients,
            InvoiceFileName = $"{orderNumbers} INVOICE",
            InvoiceOutputDirectory = invoiceDirectory,

        };

        IsLoadingConfiguration = false;

        await LoadCNCReportFiles(orders);

    }

    public ModalParameters CreateReleaseProgressModalParameters(List<Order> orders) {
        return new ModalParameters() {
            { "ActionRunner",  new ReleaseActionRunner(_service, orders, Configuration) },
            { "InProgressTitle", "Releasing Order..." },
            { "CompleteTitle", "Release Complete" }
        };
    }

    private async Task<string> AddCustomerInvoiceEmailRecipients(Guid customerId, string invoiceEmailRecipients) {
        var customer = await _getCustomerByIdAsync(customerId);
        if (customer is not null && customer.BillingContact.Email is string invoiceEmail) {

            if (!string.IsNullOrEmpty(invoiceEmailRecipients)) {
                invoiceEmailRecipients = invoiceEmail + ";" + invoiceEmailRecipients;
            } else {
                invoiceEmailRecipients = invoiceEmail;
            }

        }

        return invoiceEmailRecipients;
    }

    private async Task LoadCNCReportFiles(List<Order> orders) {
        IsReportLoadingFiles = true;

        foreach (var order in orders) {
            var files = await GetReportFiles(order.Number);
            Configuration.CNCDataFilePaths.AddRange(files);
            Configuration.GenerateCNCRelease = Configuration.CNCDataFilePaths.Any();
        }

        IsReportLoadingFiles = false;
    }

    private static async Task<string[]> GetReportFiles(string number) {
        return await Task.Run(() => {
            try {
                return Directory.GetFiles(@"Y:\CADCode\Reports\", $"{number}* - *.xml");
            } catch {
                return Array.Empty<string>();
            }
        });
    }

}
