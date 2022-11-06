namespace ApplicationCore.Features.CADCode.Contracts.Machining;

public record MultiBore : Token {

    public Point StartPosition { get; init; } = new(0, 0);
    public Point EndPosition { get; init; } = new(0, 0);
    public float Depth { get; init; }
    public float Pitch { get; init; }
    public int NumberOfHoles { get; init; }

}
