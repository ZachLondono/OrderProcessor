using ApplicationCore.Features.Orders.Domain;
using ApplicationCore.Features.Orders.Domain.ValueObjects;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.Loader.Providers.DTO;

public class TallCabinetData {

    public required int Qty { get; set; }

    public required decimal UnitPrice { get; set; }

    public string Room { get; set; } = string.Empty;

    public bool Assembled { get; set; }

    public required Dimension Height { get; set; }

    public required Dimension Width { get; set; }

    public required Dimension Depth { get; set; }

    public required string BoxMaterialFinish { get; set; }

    public required CabinetMaterialCore BoxMaterialCore { get; set; }

    public required string FinishMaterialFinish { get; set; }

    public required CabinetMaterialCore FinishMaterialCore { get; set; }

    public required string EdgeBandingColor { get; set; }

    public required CabinetSideType LeftSideType { get; set; }

    public required CabinetSideType RightSideType { get; set; }

    public required string DoorType { get; set; }

    public MDFDoorOptions? DoorStyle { get; set; } = null;

    public required int UpperDoorQty { get; set; }

    public required int LowerDoorQty { get; set; }

    public required Dimension LowerDoorHeight { get; set; }

    public bool HingeLeft { get; set; } = false;

    public required IToeType ToeType { get; set; }

    public required CabinetDrawerBoxMaterial DrawerBoxMaterial { get; set; }

    public required DrawerSlideType DrawerBoxSlideType { get; set; }

    public required int VerticalDividerUpperQty { get; set; }

    public required int VerticalDividerLowerQty { get; set; }

    public required int AdjustableShelfUpperQty { get; set; }

    public required int AdjustableShelfLowerQty { get; set; }

    public required Dimension[] RollOutBoxPositions { get; set; }

    public required RollOutBlockPosition RollOutBlocks { get; set; }

    public required bool ScoopFrontRollOuts { get; set; }

}