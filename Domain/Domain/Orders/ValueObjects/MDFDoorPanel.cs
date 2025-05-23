using OneOf;

namespace Domain.Orders.ValueObjects;

public class MDFDoorPanel : OneOfBase<SolidPanel, OpenPanel> {

    public MDFDoorPanel(OneOf<SolidPanel, OpenPanel> _) : base(_) { }

	public static implicit operator MDFDoorPanel(SolidPanel _) => new(_);
	public static explicit operator SolidPanel(MDFDoorPanel _) => _.AsT0;

	public static implicit operator MDFDoorPanel(OpenPanel _) => new(_);
	public static explicit operator OpenPanel(MDFDoorPanel _) => _.AsT1;

	public bool IsSolid => IsT0;
	public bool IsOpen => IsT1;

}
