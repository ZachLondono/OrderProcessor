using ApplicationCore.Features.Companies.Contracts;
using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using Blazored.Modal;

namespace ApplicationCore.Features.Orders.Details.OrderExport;

// TODO: Merge ExportService and ExportWidgetViewModel
internal class ExportWidgetViewModel {

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

    public ExportWidgetViewModel(CompanyDirectory.GetVendorByIdAsync getVendorByIdAsync, ExportService service) {
        _getVendorByIdAsync = getVendorByIdAsync;
        _service = service;
    }

    public async Task LoadConfiguration(Order order) {
        
        var vendor = await _getVendorByIdAsync(order.VendorId);

        if (vendor is null) return;

        Configuration = new() {
            FillDovetailOrder = vendor.ExportProfile.ExportDBOrder,
            FillMDFDoorOrder = vendor.ExportProfile.ExportMDFDoorOrder,
            GenerateEXT = vendor.ExportProfile.ExportExtFile,
            ExtJobName = $"{order.Number} - {order.Name}",
            OutputDirectory = order.WorkingDirectory
        };

    }

    public ModalParameters CreateExportProgressModalParameters() {
        return new ModalParameters() {
            { "ActionRunner",  new ExportActionRunner(_service, Configuration) }
        };
    }

}
