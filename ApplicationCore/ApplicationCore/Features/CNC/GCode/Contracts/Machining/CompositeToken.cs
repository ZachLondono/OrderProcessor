namespace ApplicationCore.Features.CNC.GCode.Contracts.Machining;

public abstract record CompositeToken : Token
{

    public abstract IEnumerable<Token> GetComponents();

}