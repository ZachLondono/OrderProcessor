namespace ApplicationCore.Features.CADCode.Contracts.Machining;

internal record Point() {

    public float X { get; init; }
    public float Y { get; init; }

    public Point(float x, float y) : this() {
        X = x;
        Y = y;
    }

}
