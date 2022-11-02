namespace ApplicationCore.Features.Orders.Domain.ValueObjects;

public class UBoxDimensions {

    public Dimension A { get; set; } = Dimension.FromMillimeters(0);
    public Dimension B { get; set; } = Dimension.FromMillimeters(0);
    public Dimension C { get; set; } = Dimension.FromMillimeters(0);

}
