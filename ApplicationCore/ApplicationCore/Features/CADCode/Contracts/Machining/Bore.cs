namespace ApplicationCore.Features.CADCode.Contracts.Machining;

public record Bore : Token {

    public Point Position { get; init; } = new(0, 0);
    public double Depth { get; init; }

}
