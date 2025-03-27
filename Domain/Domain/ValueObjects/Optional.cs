using OneOf;
using OneOf.Types;

namespace Domain.ValueObjects;

public class Optional<T> : OneOfBase<T, None> {

    protected Optional(T input) : base(input) { }
    protected Optional(None input) : base(input) { }

    public static Optional<T> None => new(new None());

    public static implicit operator Optional<T>(T _) => new(_);
    public static implicit operator Optional<T>(None _) => new(_);

}
