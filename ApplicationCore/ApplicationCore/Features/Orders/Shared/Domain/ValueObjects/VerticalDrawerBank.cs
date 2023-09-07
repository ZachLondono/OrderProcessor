using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

public record VerticalDrawerBank {

    public required Dimension[] FaceHeights { get; init; }

    public int Qty => FaceHeights.Count();

    public bool Any() => FaceHeights.Any();

    public static VerticalDrawerBank None() => new() {
        FaceHeights = Array.Empty<Dimension>(),
    };

}