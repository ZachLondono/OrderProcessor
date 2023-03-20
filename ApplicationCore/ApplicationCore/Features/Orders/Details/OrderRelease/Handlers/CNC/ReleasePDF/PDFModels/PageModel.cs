using ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.ReleasePDF.Styling;

namespace ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.ReleasePDF.PDFModels;

public class PageModel {

    public string Header { get; init; } = string.Empty;
    public Dictionary<string, SheetProgam> MachinePrograms { get; init; } = new();
    public string Subtitle { get; init; } = string.Empty;
    public byte[] ImageData { get; init; } = Array.Empty<byte>();
    public Table Parts { get; init; } = new();
    public string Footer { get; init; } = string.Empty;

}
