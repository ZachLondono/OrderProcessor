namespace ApplicationCore.Features.CADCode.Contracts.Machining;

internal record Text : Token {

    public Point Position { get; init; } = new(0, 0);
    public float Rotation { get; init; }
    public string Value { get; init; } = string.Empty;
    public string Font { get; init; } = string.Empty;
    public float Height { get; init; }
    public float Depth { get; init; }
    public RouteOffset Offset { get; init; } = new(OffsetType.None, 0);

}
