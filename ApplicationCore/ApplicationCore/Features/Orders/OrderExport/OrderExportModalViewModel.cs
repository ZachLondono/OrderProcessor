using ApplicationCore.Features.Companies.Contracts;
using ApplicationCore.Features.Orders.Shared.Domain;
using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using Blazored.Modal;

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

    private readonly CompanyDirectory.GetVendorByIdAsync _getVendorByIdAsync;
    private readonly ExportService _service;

    public OrderExportModalViewModel(CompanyDirectory.GetVendorByIdAsync getVendorByIdAsync, ExportService service) {
        _getVendorByIdAsync = getVendorByIdAsync;
        _service = service;
    }

    public async Task LoadConfiguration(Order order) {

        var vendor = await _getVendorByIdAsync(order.VendorId);

        if (vendor is null) return;

        bool containsMDFDoors = order.Products
                                        .Where(p => p is IDoorContainer)
                                        .Cast<IDoorContainer>()
                                        .Any(p => p.ContainsDoors());

        bool containsDBs = order.Products
                                .Where(p => p is IDrawerBoxContainer)
                                .Cast<IDrawerBoxContainer>()
                                .Any(p => p.ContainsDrawerBoxes());

        bool containsPPs = order.Products
                                .Where(p => p is IPPProductContainer)
                                .Cast<IPPProductContainer>()
                                .Any(p => p.ContainsPPProducts());

        bool containsCNC = order.Products
                                .Where(p => p is ICNCPartContainer)
                                .Cast<ICNCPartContainer>()
                                .Any(p => p.ContainsCNCParts());

        Configuration = new() {
            FillDovetailOrder = vendor.ExportProfile.ExportDBOrder && containsMDFDoors,
            FillMDFDoorOrder = vendor.ExportProfile.ExportMDFDoorOrder && containsDBs,
            GenerateEXT = vendor.ExportProfile.ExportExtFile && containsPPs,
            GenerateCSV = containsCNC,
            CsvJobName = $"{order.Number} - {order.Name}",
            ExtJobName = $"{order.Number} - {order.Name}",
            OutputDirectory = order.WorkingDirectory
        };

    }

    public ModalParameters CreateExportProgressModalParameters() {
        return new ModalParameters() {
            { "ActionRunner",  new ExportActionRunner(_service, Configuration) },
            { "InProgressTitle", "Exporting Order..." },
            { "CompleteTitle", "Export Complete" }
        };
    }

}
