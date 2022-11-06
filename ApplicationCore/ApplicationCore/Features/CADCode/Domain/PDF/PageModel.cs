namespace ApplicationCore.Features.CADCode.Services.Domain.PDF;

internal class PageModel {

    public string Header { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Subtitle { get; init; } = string.Empty;
    public byte[] ImageData { get; init; } = Array.Empty<byte>();
    public Table Parts { get; init; } = new();
    public string Footer { get; init; } = string.Empty;

}
