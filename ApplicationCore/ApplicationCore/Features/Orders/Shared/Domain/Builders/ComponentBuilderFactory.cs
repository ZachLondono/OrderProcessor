using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.Builders;

public class ComponentBuilderFactory {

    public MDFDoorBuilder CreateMDFDoorBuilder() {

        MDFDoorConfiguration configuration = new() {
            Material = "MDF-3/4\"",
            Thickness = Dimension.FromInches(0.75),
            FramingBead = "Shaker",
            EdgeDetail = "Eased",
            PanelDetail = "Flat",
            TopRail = Dimension.Zero,
            BottomRail = Dimension.Zero,
            LeftStile = Dimension.Zero,
            RightStile = Dimension.Zero,
        };

        return new MDFDoorBuilder(configuration);

    }

    public DovetailDrawerBoxBuilder CreateDovetailDrawerBoxBuilder() => new();

}
