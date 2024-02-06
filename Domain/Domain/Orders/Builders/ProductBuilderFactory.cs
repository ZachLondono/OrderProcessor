namespace Domain.Orders.Builders;

public class ProductBuilderFactory {

    public BaseCabinetBuilder CreateBaseCabinetBuilder() => new();

    public WallCabinetBuilder CreateWallCabinetBuilder() => new();

    public BaseDiagonalCornerCabinetBuilder CreateBaseDiagonalCornerCabinetBuilder() => new();

    public BasePieCutCornerCabinetBuilder CreateBasePieCutCornerCabinetBuilder() => new();

    public BlindBaseCabinetBuilder CreateBlindBaseCabinetBuilder() => new();

    public BlindWallCabinetBuilder CreateBlindWallCabinetBuilder() => new();

    public DrawerBaseCabinetBuilder CreateDrawerBaseCabinetBuilder() => new();

    public SinkCabinetBuilder CreateSinkCabinetBuilder() => new();

    public TallCabinetBuilder CreateTallCabinetBuilder() => new();

    public WallDiagonalCornerCabinetBuilder CreateWallDiagonalCornerCabinetBuilder() => new();

    public WallPieCutCornerCabinetBuilder CreateWallPieCutCornerCabinetBuilder() => new();

    public TrashCabinetBuilder CreateTrashCabinetBuilder() => new();

}
