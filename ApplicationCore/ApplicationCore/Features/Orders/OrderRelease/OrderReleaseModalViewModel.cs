using ApplicationCore.Features.Companies.Contracts;
using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Features.Orders.Shared.Domain.Products.Doors;
using ApplicationCore.Features.Orders.Shared.Domain.Products.DrawerBoxes;
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

    public bool IncludeSuppliesInSummary {
        get => _includeSuppliesInSummary;
        set {

            _includeSuppliesInSummary = value;

            if (value) {
                Configuration.SupplyOptions.AllSupplies();
            } else {
                Configuration.SupplyOptions.NoSupplies();
            }

            OnPropertyChanged?.Invoke();

        }
    }

    public bool DoAnyOrdersContainDovetailDBs { get; private set; }

    public bool DoAnyOrdersContainFivePieceDoors { get; private set; }

    public bool DoAnyOrdersContainDoweledDrawerBoxes { get; private set; }

    private bool _includeSuppliesInSummary = false;
    private bool _isLoadingConfiguration = false;
    private bool _isReportLoadingFiles = false;
    private ReleaseConfiguration _configuration = new();
    private readonly CompanyDirectory.GetVendorByIdAsync _getVendorByIdAsync;
    private readonly ReleaseService _service;

    public OrderReleaseModalViewModel(CompanyDirectory.GetVendorByIdAsync getVendorByIdAsync, ReleaseService service) {
        _getVendorByIdAsync = getVendorByIdAsync;
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
        var orderInvoiceRecipients = orders.Select(o => o.Billing.InvoiceEmail)
                                    .Distinct()
                                    .Where(e => !string.IsNullOrEmpty(e))
                                    .ToList();
        var customerInvoiceRecipients = string.Join(";", orderInvoiceRecipients);
        if (invoiceEmailRecipients != string.Empty && customerInvoiceRecipients != string.Empty) {
            invoiceEmailRecipients += ";" + customerInvoiceRecipients;
        } else if (invoiceEmailRecipients == string.Empty && customerInvoiceRecipients != string.Empty) {
            invoiceEmailRecipients = customerInvoiceRecipients;
        }

        string workingDirectories = string.Join(';', orders.Select(o => o.WorkingDirectory).Where(s => !string.IsNullOrEmpty(s)).Select(wd => Path.Combine(wd, "CUTLIST")));

        string releaseDirectory = @"X:\_CUTLISTS  Incoming";
        if (workingDirectories != string.Empty) {
            releaseDirectory += ";" + workingDirectories;
        }

        string invoiceDirectory = string.Join(';', orders.Select(o => o.WorkingDirectory).Where(s => !string.IsNullOrEmpty(s)));

        string orderNumbers = string.Join(", ", orders.Select(o => o.Number));

        DoAnyOrdersContainDovetailDBs = orders.Any(order => order.Products.Any(p => p is DovetailDrawerBoxProduct));
        DoAnyOrdersContainFivePieceDoors = orders.Any(order => order.Products.Any(p => p is FivePieceDoorProduct));
        DoAnyOrdersContainDoweledDrawerBoxes = orders.Any(order => order.Products.Any(p => p is DoweledDrawerBoxProduct));

        Configuration = new ReleaseConfiguration() {

            SendReleaseEmail = vendor.ReleaseProfile.SendReleaseEmail,
            IncludeMaterialSummaryInEmailBody = true,
            ReleaseEmailRecipients = vendor.ReleaseProfile.ReleaseEmailRecipients,
            GenerateJobSummary = vendor.ReleaseProfile.GenerateJobSummary,
            IncludeProductTablesInSummary = false,
            SupplyOptions = new(),                      // TODO: add this to the vendor release profile
            GeneratePackingList = vendor.ReleaseProfile.GeneratePackingList,
            IncludeInvoiceInRelease = vendor.ReleaseProfile.IncludeInvoice,
            Generate5PieceCutList = DoAnyOrdersContainFivePieceDoors,
            GenerateDoweledDrawerBoxCutList = DoAnyOrdersContainDoweledDrawerBoxes,
            IncludeDovetailDBPackingList = DoAnyOrdersContainDovetailDBs,
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
