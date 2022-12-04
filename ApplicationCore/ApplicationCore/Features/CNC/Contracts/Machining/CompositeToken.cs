namespace ApplicationCore.Features.CNC.Contracts.Machining;

public abstract record CompositeToken : Token {

    public abstract IEnumerable<Token> GetComponents();

}