using Domain.Companies;
using Domain.Orders;
using Domain.Orders.Entities;
using Blazored.Modal;
using Domain.ProductPlanner;
using Domain.Services.WorkingDirectory;

namespace ApplicationCore.Features.Orders.OrderExport;

// TODO: Merge ExportService and ExportWidgetViewModel
internal class OrderExportModalViewModel {

    public Action? OnPropertyChanged { get; set; } = null;

    private ExportConfiguration _configuration = new();
    public ExportConfiguration Configuration {
        get => _configuration;
        set {
            _configuration = value;
            OnPropertyChanged?.Invoke();
        }
    }

    private Order? _order;
    private readonly CompanyDirectory.GetVendorByIdAsync _getVendorByIdAsync;
    private readonly ExportService _service;

    public OrderExportModalViewModel(CompanyDirectory.GetVendorByIdAsync getVendorByIdAsync, ExportService service) {
        _getVendorByIdAsync = getVendorByIdAsync;
        _service = service;
    }

    public async Task LoadConfiguration(Order order) {

        _order = order;

        var vendor = await _getVendorByIdAsync(order.VendorId);

        if (vendor is null) return;

        bool containsMDFDoors = order.Products
                                        .Where(p => p is IMDFDoorContainer)
                                        .Cast<IMDFDoorContainer>()
                                        .Any(p => p.ContainsDoors());

        bool containsDBs = order.Products
                                .Where(p => p is IDovetailDrawerBoxContainer)
                                .Cast<IDovetailDrawerBoxContainer>()
                                .Any(p => p.ContainsDovetailDrawerBoxes());

        bool containsPPs = order.Products
                                .Where(p => p is IPPProductContainer)
                                .Cast<IPPProductContainer>()
                                .Any(p => p.ContainsPPProducts());

        bool containsCNC = order.Products
                                .Where(p => p is ICNCPartContainer)
                                .Cast<ICNCPartContainer>()
                                .Any(p => p.ContainsCNCParts());

        Configuration = new() {
            FillDovetailOrder = vendor.ExportProfile.ExportDBOrder && containsDBs,
            FillMDFDoorOrder = vendor.ExportProfile.ExportMDFDoorOrder && containsMDFDoors,
            GenerateEXT = vendor.ExportProfile.ExportExtFile && containsPPs,
            GenerateCSV = containsCNC,
            CsvJobName = $"{order.Number} - {order.Name}",
            ExtJobName = $"{order.Number} - {order.Name}",
            OutputDirectory = WorkingDirectoryStructure.Create(order.WorkingDirectory).OrdersDirectory
        };

    }

    public ModalParameters CreateExportProgressModalParameters() {

        if (_order is null) throw new InvalidOperationException("Cannot create export modal parameters before order is loaded");

        return new ModalParameters() {
            { "ActionRunner",  new ExportActionRunner(_order, _service, Configuration) },
            { "InProgressTitle", "Exporting Order..." },
            { "CompleteTitle", "Export Complete" }
        };

    }

}
