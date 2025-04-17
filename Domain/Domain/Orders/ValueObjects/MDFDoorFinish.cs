using OneOf;
using OneOf.Types;

namespace Domain.Orders.ValueObjects;

public class MDFDoorFinish : OneOfBase<Paint, Primer, None> {

    public MDFDoorFinish(OneOf<Paint, Primer, None> _) : base(_) { }

	public static implicit operator MDFDoorFinish(Paint _) => new(_);
	public static implicit operator MDFDoorFinish(Primer _) => new(_);
	public static implicit operator MDFDoorFinish(None _) => new(_);

}
