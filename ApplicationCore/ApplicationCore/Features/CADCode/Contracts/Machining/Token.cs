namespace ApplicationCore.Features.CADCode.Contracts.Machining;

internal abstract record Token {

    public Tool Tool { get; init; } = new("", 0);
    public int Sequence { get; init; }
    public string RType { get; init; } = string.Empty;
    public int PassCount { get; init; } = 1;

}
