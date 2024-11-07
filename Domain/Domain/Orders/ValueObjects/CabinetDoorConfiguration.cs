using OneOf;
using System.Diagnostics.CodeAnalysis;

namespace Domain.Orders.ValueObjects;

public class CabinetDoorConfiguration : OneOfBase<CabinetSlabDoorMaterial, MDFDoorOptions, DoorsByOthers> {

	public bool IsSlab => IsT0;

	public bool IsMDF => IsT1;

	public bool IsByOthers => IsT2;

	public CabinetSlabDoorMaterial AsSlabDoorMaterial => AsT0;

	public MDFDoorOptions AsMDFDoorOptions => AsT1;

	CabinetDoorConfiguration(OneOf<CabinetSlabDoorMaterial, MDFDoorOptions, DoorsByOthers> _) : base(_) { }

	public static implicit operator CabinetDoorConfiguration(CabinetSlabDoorMaterial _) => new(_);
	public static implicit operator CabinetDoorConfiguration(MDFDoorOptions _) => new(_);
	public static implicit operator CabinetDoorConfiguration(DoorsByOthers _) => new(_);

	public bool TryGetMDFOptions([NotNullWhen(true)] out MDFDoorOptions mdf) {

		MDFDoorOptions? mdfOptions = Match<MDFDoorOptions?>(
			slab => null,
			mdfOptions => mdfOptions,
			byothers => null
		);

		if (mdfOptions is not null) {
			mdf = mdfOptions;
			return true;
		}

		mdf = default!;

		return false;

	}

}
