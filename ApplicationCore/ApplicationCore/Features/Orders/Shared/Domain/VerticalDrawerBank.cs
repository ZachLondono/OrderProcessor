using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain;

public record VerticalDrawerBank {

    public required IEnumerable<Dimension> FaceHeights { get; init; }
    public required CabinetDrawerBoxMaterial BoxMaterial { get; init; }
    public required DrawerSlideType SlideType { get; init; }

}