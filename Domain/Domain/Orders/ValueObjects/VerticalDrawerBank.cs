using Domain.ValueObjects;

namespace Domain.Orders.ValueObjects;

public record VerticalDrawerBank {

    public required Dimension[] FaceHeights { get; init; }

    public int Qty => FaceHeights.Count();

    public bool Any() => FaceHeights.Any();

    public static VerticalDrawerBank None() => new() {
        FaceHeights = Array.Empty<Dimension>(),
    };

}