using ApplicationCore.Features.CNC.Contracts.Machining;

namespace ApplicationCore.Features.CNC.ReleasePDF;

public record ImageText
{

    public string Text { get; init; } = string.Empty;
    public Point Location { get; init; } = new(0, 0);

}