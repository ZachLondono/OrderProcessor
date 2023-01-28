using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.Builders;

public class ProductBuilderFactory {

    public MDFDoorBuilder CreateMDFDoorBuilder() {

        MDFDoorConfiguration configuration = new() {
            Material = "MDF-3/4\"",
            FramingBead = "Shaker",
            EdgeDetail = "Eased",
            TopRail = Dimension.Zero,
            BottomRail = Dimension.Zero,
            LeftStile = Dimension.Zero,
            RightStile = Dimension.Zero,
        };

        return new MDFDoorBuilder(configuration);

    }

    public DovetailDrawerBoxBuilder CreateDovetailDrawerBoxBuilder() => new();

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


}
