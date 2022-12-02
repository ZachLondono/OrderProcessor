namespace ApplicationCore.Features.CADCode.Contracts.Machining;

public abstract record CompositeToken : Token {

    public abstract IEnumerable<Token> GetComponents();

}