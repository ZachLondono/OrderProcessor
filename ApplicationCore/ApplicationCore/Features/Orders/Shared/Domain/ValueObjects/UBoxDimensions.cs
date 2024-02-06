using Domain.ValueObjects;

namespace ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

public class UBoxDimensions {

    public Dimension A { get; set; } = Dimension.FromMillimeters(0);
    public Dimension B { get; set; } = Dimension.FromMillimeters(0);
    public Dimension C { get; set; } = Dimension.FromMillimeters(0);

    public UBoxDimensions() { }

    public UBoxDimensions(Dimension a, Dimension b, Dimension c) {
        A = a;
        B = b;
        C = c;
    }

}
