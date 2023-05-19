using ApplicationCore.Features.Companies.Contracts;
using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using Blazored.Modal;

namespace ApplicationCore.Features.Orders.Details.OrderRelease;

public class OrderReleaseWidgetViewModel {

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

    public OrderReleaseWidgetViewModel(CompanyDirectory.GetVendorByIdAsync getVendorByIdAsync, CompanyDirectory.GetCustomerByIdAsync getCustomerByIdAsync, ReleaseService service) {
        _getVendorByIdAsync = getVendorByIdAsync;
        _getCustomerByIdAsync = getCustomerByIdAsync;
        _service = service;
    }

    public async Task LoadConfiguration(Order order) {

        IsLoadingConfiguration = true;

        string invoiceEmailRecipients = string.Empty;

        var vendor = await _getVendorByIdAsync(order.VendorId);
        if (vendor is null) {
            return;
        }

        invoiceEmailRecipients = vendor.ReleaseProfile.InvoiceEmailRecipients;
        invoiceEmailRecipients = await AddCustomerInvoiceEmailRecipients(order.CustomerId, invoiceEmailRecipients);

        string releaseDirectory = order.WorkingDirectory;
        string invoiceDirectory = order.WorkingDirectory;

        Configuration = new ReleaseConfiguration() {

            SendReleaseEmail = vendor.ReleaseProfile.SendReleaseEmail,
            ReleaseEmailRecipients = vendor.ReleaseProfile.ReleaseEmailRecipients,
            GenerateJobSummary = vendor.ReleaseProfile.GenerateJobSummary,
            IncludeProductTablesInSummary = false,
            IncludeSuppliesInSummary = false,
            GeneratePackingList = vendor.ReleaseProfile.GeneratePackingList,
            IncludeInvoiceInRelease = vendor.ReleaseProfile.IncludeInvoice,
            ReleaseFileName = $"{order.Number} CUTLIST",
            ReleaseOutputDirectory = releaseDirectory,
            GenerateCNCRelease = false,

            GenerateInvoice = vendor.ReleaseProfile.GenerateInvoice,
            SendInvoiceEmail = vendor.ReleaseProfile.SendInvoiceEmail,
            InvoiceEmailRecipients = invoiceEmailRecipients,
            InvoiceFileName = $"{order.Number} INVOICE",
            InvoiceOutputDirectory = invoiceDirectory,

            EmailSenderName = vendor.EmailSender.Name,
            EmailSenderEmail = vendor.EmailSender.Email,
            EmailSenderPassword = vendor.EmailSender.ProtectedPassword

        };
        
        IsLoadingConfiguration = false;

        await LoadCNCReportFiles(order);

    }

    public ModalParameters CreateReleaseProgressModalParameters(Order order) {
        return new ModalParameters() {
            { "ActionRunner",  new ReleaseActionRunner(_service, order, Configuration) }
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

    private async Task LoadCNCReportFiles(Order order) {
        IsReportLoadingFiles = true;

        var files = await GetReportFiles(order.Number);
        Configuration.CNCDataFilePaths.AddRange(files);
        Configuration.GenerateCNCRelease = Configuration.CNCDataFilePaths.Any();

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
