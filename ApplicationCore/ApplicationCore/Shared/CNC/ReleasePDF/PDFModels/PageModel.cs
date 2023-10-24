using ApplicationCore.Shared.CNC.ReleasePDF.Styling;

namespace ApplicationCore.Shared.CNC.ReleasePDF.PDFModels;

public class PageModel {

    public string Header { get; init; } = string.Empty;
    public Dictionary<string, SheetProgam> MachinePrograms { get; init; } = new();
    public string Subtitle { get; init; } = string.Empty;
    public byte[] ImageData { get; init; } = Array.Empty<byte>();
    public Table Parts { get; init; } = new();
    public DateTime TimeStamp { get; init; } = DateTime.MinValue;

}
