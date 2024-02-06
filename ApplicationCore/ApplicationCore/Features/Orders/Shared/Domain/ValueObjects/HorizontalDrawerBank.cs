using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using Domain.ValueObjects;

namespace ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

public record HorizontalDrawerBank {

    public required Dimension FaceHeight { get; init; }
    public required int Quantity { get; init; }

    public bool Any() => Quantity > 0;

    public static HorizontalDrawerBank None() => new() {
        FaceHeight = Dimension.Zero,
        Quantity = 0,
    };

}