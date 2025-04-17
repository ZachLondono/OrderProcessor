using Domain.Orders.Enums;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;
using OneOf.Types;

namespace Domain.Orders.Persistance.DataModels;

public abstract class CabinetDataModelBase : ProductDataModelBase {

    public Dimension Height { get; set; }
    public Dimension Width { get; set; }
    public Dimension Depth { get; set; }
    public CabinetMaterialCore BoxMatCore { get; set; }
    public string BoxMatFinish { get; set; } = string.Empty;
    public CabinetMaterialFinishType BoxFinishType { get; set; }
    public CabinetMaterialCore FinishMatCore { get; set; }
    public CabinetMaterialFinishType FinishFinishType { get; set; }
    public string FinishMatFinish { get; set; } = string.Empty;
    public string? FinishMatPaint { get; set; }
    public string EdgeBandColor { get; set; } = string.Empty;
    public CabinetSideType LeftSideType { get; set; }
    public CabinetSideType RightSideType { get; set; }
    public bool Assembled { get; set; }
    public string Comment { get; set; } = string.Empty;

    public bool ContainsSlabDoors { get; set; }
    public CabinetMaterialCore SlabDoorCore { get; set; }
    public string? SlabDoorFinish { get; set; }
    public CabinetMaterialFinishType SlabDoorFinishType { get; set; }
    public string? SlabDoorPaint { get; set; }

    public bool ContainsMDFDoor { get; set; }
    public string? FramingBead { get; set; }
    public string? EdgeDetail { get; set; }
    public string? PanelDetail { get; set; }
    public Dimension? Thickness { get; set; }
    public string? Material { get; set; }
    public Dimension? PanelDrop { get; set; }
    public MDFDoorFinishType FinishType { get; set; }
    public string? FinishColor { get; set; }

    protected MDFDoorOptions? GetMDFDoorConfiguration() {

        if (!ContainsMDFDoor) return null;

        MDFDoorFinish finish = FinishType switch {
            MDFDoorFinishType.None => new None(),
            MDFDoorFinishType.Paint => new Paint(FinishColor ?? "Unknown"),
            MDFDoorFinishType.Primer => new Primer(FinishColor ?? "Unknown"),
            _ => throw new InvalidOperationException("Invalid MDF door finish type")
        };

        return new(Material ?? "", Thickness ?? Dimension.Zero, FramingBead ?? "", EdgeDetail ?? "", PanelDetail ?? "", PanelDrop ?? Dimension.Zero, finish);

    }

    protected CabinetSlabDoorMaterial? GetSlabDoorMaterial() {
        if (!ContainsSlabDoors) return null;
        if (SlabDoorFinish is null) throw new InvalidOperationException("Slab door finish is missing");
        return new CabinetSlabDoorMaterial(SlabDoorFinish, SlabDoorFinishType, SlabDoorCore, SlabDoorPaint);
    }

	protected CabinetDoorConfiguration GetDoorConfiguration() {

		var mdfConfig = GetMDFDoorConfiguration();
		var slabDoorMaterial = GetSlabDoorMaterial();

		CabinetDoorConfiguration doorConfiguration;
		if (mdfConfig is null && slabDoorMaterial is null) {
			doorConfiguration = new DoorsByOthers();
		} else if (mdfConfig is MDFDoorOptions mdf) {
			doorConfiguration = mdf;
		} else if (slabDoorMaterial is CabinetSlabDoorMaterial slab) {
			doorConfiguration = slab;
		} else {
			throw new InvalidDataException("Invalid base cabinet door configuration in database.");
		}

		return doorConfiguration;

	}

}
