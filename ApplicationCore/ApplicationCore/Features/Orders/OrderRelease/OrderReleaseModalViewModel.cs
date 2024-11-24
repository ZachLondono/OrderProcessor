using Domain.Companies;
using ApplicationCore.Features.Orders.Details.Queries;
using Domain.Orders;
using Domain.Orders.Entities;
using Blazored.Modal;
using Domain.Orders.Entities.Products.Doors;
using Domain.Orders.Entities.Products.DrawerBoxes;
using Domain.Infrastructure.Bus;
using Domain.Orders.Entities.Products.Closets;
using Domain.Orders.Entities.Products.Cabinets;

namespace ApplicationCore.Features.Orders.OrderRelease;

public class OrderReleaseModalViewModel {

    public Action? OnPropertyChanged { get; set; }

    public bool IsLoadingOrders {
        get => _isLoadingOrders;
        set {
            _isLoadingOrders = value;
            OnPropertyChanged?.Invoke();
        }
    }

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

    public bool DoAnyOrdersContainDovetailDBs { get; private set; }

    public bool DoAnyOrdersContainFivePieceDoors { get; private set; }

    public bool DoAnyOrdersContainDoweledDrawerBoxes { get; private set; }

    public bool DoAnyOrdersContainCNCParts { get; private set; }

    public bool DoAnyOrdersContainCabinets { get; private set; }

    public bool DoAnyOrdersContainHardware { get; private set; }

    private bool _isLoadingOrders = true;
    private bool _isLoadingConfiguration = false;
    private bool _isReportLoadingFiles = false;
    private ReleaseConfiguration _configuration = new();
    private readonly CompanyDirectory.GetVendorByIdAsync _getVendorByIdAsync;
    private readonly ReleaseService _service;
    private readonly IBus _bus;

    public OrderReleaseModalViewModel(CompanyDirectory.GetVendorByIdAsync getVendorByIdAsync, ReleaseService service, IBus bus) {
        _getVendorByIdAsync = getVendorByIdAsync;
        _service = service;
        _bus = bus;
    }

    public async Task<List<Order>> LoadOrdersAsync(IEnumerable<Guid> orderIds) {

        IsLoadingOrders = true;

        List<Order> orders = new();

        foreach (var orderId in orderIds) {

            var response = await _bus.Send(new GetOrderById.Query(orderId));
            response.OnSuccess(orders.Add);

        }

        IsLoadingOrders = false;

        return orders;

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

        string workingDirectories = string.Join(';', orders.Select(o => o.WorkingDirectory).Where(s => !string.IsNullOrEmpty(s)).Where(Directory.Exists).Select(wd => Path.Combine(wd, "CUTLIST")));

        string releaseDirectory = @"X:\_CUTLISTS  Incoming";
        if (!Directory.Exists(releaseDirectory)) {
            releaseDirectory = "";
        }

        if (releaseDirectory == string.Empty) {
            releaseDirectory = workingDirectories;
        } else if (workingDirectories != string.Empty) {
            releaseDirectory += ";" + workingDirectories;
        }

        string invoiceDirectory = string.Join(';', orders.Select(o => o.WorkingDirectory).Where(s => !string.IsNullOrEmpty(s)).Where(Directory.Exists));

        string orderNumbers = string.Join(", ", orders.Select(o => o.Number));

        DoAnyOrdersContainDovetailDBs = orders.Any(order => order.Products.OfType<IDovetailDrawerBoxContainer>().Any(p => p.ContainsDovetailDrawerBoxes()));
        DoAnyOrdersContainFivePieceDoors = orders.Any(order => order.Products.Any(p => p is FivePieceDoorProduct));
        DoAnyOrdersContainDoweledDrawerBoxes = orders.Any(order => order.Products.Any(p => p is DoweledDrawerBoxProduct));
        DoAnyOrdersContainCNCParts = orders.Any(order => order.Products.OfType<ICNCPartContainer>().Any(p => p.ContainsCNCParts()));
        DoAnyOrdersContainCabinets = orders.Any(order => order.Products.OfType<Cabinet>().Any());
        DoAnyOrdersContainHardware = orders.Any(order => order.Hardware.Supplies.Any() || order.Hardware.DrawerSlides.Any() || order.Hardware.HangingRails.Any());

        bool installCams = orders.Any(o => o.Products.OfType<ClosetPart>().Any(c => c.InstallCams));

        Configuration = new ReleaseConfiguration() {

            SendReleaseEmail = vendor.ReleaseProfile.SendReleaseEmail,
            IncludeMaterialSummaryInEmailBody = true,
            ReleaseEmailRecipients = vendor.ReleaseProfile.ReleaseEmailRecipients,
            GenerateJobSummary = vendor.ReleaseProfile.GenerateJobSummary,
            IncludeProductTablesInSummary = false,
            IncludeAdditionalItemsInSummary = true,
            IncludeCounterTopsInSummary = true,
            InstallCamsInClosetParts = installCams,
            GeneratePackingList = vendor.ReleaseProfile.GeneratePackingList,
            IncludeInvoiceInRelease = vendor.ReleaseProfile.IncludeInvoice,
            Generate5PieceCutList = DoAnyOrdersContainFivePieceDoors,
            GenerateDoweledDrawerBoxCutList = DoAnyOrdersContainDoweledDrawerBoxes,
            GenerateCabinetList = DoAnyOrdersContainCabinets,
            GenerateHardwareList = DoAnyOrdersContainHardware,
            IncludeDovetailDBPackingList = DoAnyOrdersContainDovetailDBs,
            ReleaseFileName = $"{orderNumbers} CUTLIST",
            ReleaseOutputDirectory = releaseDirectory,
            GenerateCNCRelease = false,
            CopyCNCReportToWorkingDirectory = true,
            GenerateCNCGCode = false,

            GenerateInvoice = vendor.ReleaseProfile.GenerateInvoice,
            SendInvoiceEmail = vendor.ReleaseProfile.SendInvoiceEmail,
            InvoiceEmailRecipients = invoiceEmailRecipients,
            InvoiceFileName = $"{orderNumbers} INVOICE",
            InvoiceOutputDirectory = invoiceDirectory,

            SendDovetailDBEmail = DoAnyOrdersContainDovetailDBs,
            DovetailDBEmailRecipients = "dovetail@royalcabinet.com",

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
