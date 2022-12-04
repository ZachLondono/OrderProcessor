namespace ApplicationCore.Features.CNC.Contracts.Machining;

public record MultiBore : Token {

    public Point StartPosition { get; init; } = new(0, 0);
    public Point EndPosition { get; init; } = new(0, 0);
    public double Depth { get; init; }
    public double Pitch { get; init; }
    public int NumberOfHoles { get; init; }

}
