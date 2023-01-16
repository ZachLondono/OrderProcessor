using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.Builders;

internal class ProductBuilderFactory {

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

}
