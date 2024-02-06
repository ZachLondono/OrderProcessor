namespace Domain.Orders.Builders;

public class ProductBuilderFactory {

    internal BaseCabinetBuilder CreateBaseCabinetBuilder() => new();

    internal WallCabinetBuilder CreateWallCabinetBuilder() => new();

    internal BaseDiagonalCornerCabinetBuilder CreateBaseDiagonalCornerCabinetBuilder() => new();

    internal BasePieCutCornerCabinetBuilder CreateBasePieCutCornerCabinetBuilder() => new();

    internal BlindBaseCabinetBuilder CreateBlindBaseCabinetBuilder() => new();

    internal BlindWallCabinetBuilder CreateBlindWallCabinetBuilder() => new();

    internal DrawerBaseCabinetBuilder CreateDrawerBaseCabinetBuilder() => new();

    internal SinkCabinetBuilder CreateSinkCabinetBuilder() => new();

    internal TallCabinetBuilder CreateTallCabinetBuilder() => new();

    internal WallDiagonalCornerCabinetBuilder CreateWallDiagonalCornerCabinetBuilder() => new();

    internal WallPieCutCornerCabinetBuilder CreateWallPieCutCornerCabinetBuilder() => new();

    internal TrashCabinetBuilder CreateTrashCabinetBuilder() => new();

}
