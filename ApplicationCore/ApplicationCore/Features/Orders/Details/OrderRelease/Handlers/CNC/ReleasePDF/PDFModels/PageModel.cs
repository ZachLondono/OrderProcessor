using ApplicationCore.Features.CNC.ReleasePDF.Styling;

namespace ApplicationCore.Features.CNC.ReleasePDF.PDFModels;

public class PageModel {

    public string Header { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Title2 { get; init; } = string.Empty;
    public string Subtitle { get; init; } = string.Empty;
    public byte[] ImageData { get; init; } = Array.Empty<byte>();
    public Table Parts { get; init; } = new();
    public string Footer { get; init; } = string.Empty;

}
