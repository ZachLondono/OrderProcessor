using ApplicationCore.Features.CNC.Shared;

namespace ApplicationCore.Features.CNC.ReleasePDF.PDFModels;

public record ImageText {

    public string Text { get; init; } = string.Empty;
    public Point Location { get; init; } = new(0, 0);

}