using OneOf;

namespace Domain.Orders.ValueObjects;

public class MDFDoorPanel : OneOfBase<SolidPanel, OpenPanel> {

    public MDFDoorPanel(OneOf<SolidPanel, OpenPanel> _) : base(_) { }

	public static implicit operator MDFDoorPanel(SolidPanel _) => new(_);
	public static implicit operator MDFDoorPanel(OpenPanel _) => new(_);

}
