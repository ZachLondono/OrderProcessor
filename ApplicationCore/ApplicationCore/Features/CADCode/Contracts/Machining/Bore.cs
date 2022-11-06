namespace ApplicationCore.Features.CADCode.Contracts.Machining;

internal record Bore : Token {

    public Point Position { get; init; } = new(0, 0);
    public float Depth { get; init; }

}
