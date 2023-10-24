using ApplicationCore.Shared.CNC.Domain;

namespace ApplicationCore.Shared.CNC.ReleasePDF.PDFModels;

public record ImageText {

    public string Text { get; init; } = string.Empty;
    public Point Location { get; init; } = new(0, 0);

}