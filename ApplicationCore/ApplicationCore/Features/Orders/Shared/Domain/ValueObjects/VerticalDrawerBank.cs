using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

public record VerticalDrawerBank {

    public required IEnumerable<Dimension> FaceHeights { get; init; }
    public required CabinetDrawerBoxMaterial BoxMaterial { get; init; }
    public required DrawerSlideType SlideType { get; init; }

    public int DrawerQty => FaceHeights.Count();

    public bool Any() => FaceHeights.Any();

}