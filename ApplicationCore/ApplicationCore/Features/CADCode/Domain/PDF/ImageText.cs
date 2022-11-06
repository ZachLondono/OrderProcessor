using ApplicationCore.Features.CADCode.Contracts.Machining;

namespace ApplicationCore.Features.CADCode.Services.Domain.PDF;

public record ImageText
{

    public string Text { get; init; } = string.Empty;
    public Point Location { get; init; } = new(0,0);

}