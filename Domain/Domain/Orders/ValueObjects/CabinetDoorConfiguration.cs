using OneOf;

namespace Domain.Orders.ValueObjects;

public class CabinetDoorConfiguration : OneOfBase<CabinetSlabDoorMaterial, MDFDoorOptions, DoorsByOthers> {

	public bool IsSlab => IsT0;

	public bool IsMDF => IsT1;

	public bool IsByOthers => IsT2;

	public CabinetSlabDoorMaterial AsSlabDoorMaterial => AsT0;

	public MDFDoorOptions AsMDFDoorOptions => AsT1;

	CabinetDoorConfiguration(OneOf<CabinetSlabDoorMaterial, MDFDoorOptions, DoorsByOthers> _) : base(_) { }

}
