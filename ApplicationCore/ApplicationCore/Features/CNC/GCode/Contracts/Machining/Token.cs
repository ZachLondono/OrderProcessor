namespace ApplicationCore.Features.CNC.GCode.Contracts.Machining;

public abstract record Token
{

    // TODO: add comment property to token
    public Tool Tool { get; init; } = new("", 0);
    public int Sequence { get; init; }
    public string RType { get; init; } = string.Empty;
    public int PassCount { get; init; } = 1;

}
