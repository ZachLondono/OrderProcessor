using ApplicationCore.Features.Orders.Shared.Domain;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Loader.Providers.DTO;

public class BaseCabinetData {

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

    public required int DoorQty { get; set; }

    public bool HingeLeft { get; set; } = false;

    public required IToeType ToeType { get; set; }

    public required int DrawerQty { get; set; }

    public required Dimension DrawerFaceHeight { get; set; }

    public required CabinetDrawerBoxMaterial DrawerBoxMaterial { get; set; }

    public required DrawerSlideType DrawerBoxSlideType { get; set; }

    public required int VerticalDividerQty { get; set; }

    public required int AdjustableShelfQty { get; set; }

    public required Dimension[] RollOutBoxPositions { get; set; }

    public required RollOutBlockPosition RollOutBlocks { get; set; }

    public required bool ScoopFrontRollOuts { get; set; }

    public required ShelfDepth ShelfDepth { get; set; }

}
