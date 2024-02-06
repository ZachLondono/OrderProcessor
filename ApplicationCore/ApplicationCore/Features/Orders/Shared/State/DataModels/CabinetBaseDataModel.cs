using Domain.Orders.Enums;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;

namespace ApplicationCore.Features.Orders.Shared.State.DataModels;

internal abstract class CabinetDataModelBase : ProductDataModelBase {

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
    public string? PaintColor { get; set; }

    protected MDFDoorOptions? GetMDFDoorConfiguration() {
        if (!ContainsMDFDoor) return null;
        return new(Material ?? "", Thickness ?? Dimension.Zero, FramingBead ?? "", EdgeDetail ?? "", PanelDetail ?? "", PanelDrop ?? Dimension.Zero, PaintColor);
    }

    protected CabinetSlabDoorMaterial? GetSlabDoorMaterial() {
        if (!ContainsSlabDoors) return null;
        if (SlabDoorFinish is null) throw new InvalidOperationException("Slab door finish is missing");
        return new CabinetSlabDoorMaterial(SlabDoorFinish, SlabDoorFinishType, SlabDoorCore, SlabDoorPaint);
    }
}
