using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

public record HorizontalDrawerBank {

    public required Dimension FaceHeight { get; init; }
    public required int Quantity { get; init; }

    public bool Any() => Quantity > 0;

}