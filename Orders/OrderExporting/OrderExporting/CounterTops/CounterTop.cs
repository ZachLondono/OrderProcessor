using Domain.ValueObjects;

namespace OrderExporting.CounterTops;

public record CounterTop {

    public required Dimension Width { get; init; }
    public required Dimension Length { get; init; }
    public required string Finish { get; init; }
    public required bool FinishedLeft { get; init; }
    public required bool FinishedTop { get; init; }
    public required bool FinishedBottom { get; init; }
    public required bool FinishedRight { get; init; }

}