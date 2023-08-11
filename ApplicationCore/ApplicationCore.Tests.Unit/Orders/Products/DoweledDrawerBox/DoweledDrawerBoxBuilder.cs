using ApplicationCore.Features.Orders.Shared.Domain.Components;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Tests.Unit.Orders.Products.DoweledDrawerBoxTests;

public class DoweledDrawerBoxBuilder {

    public int Qty { get; set; } = 1;
    public Dimension Height { get; set; } = Dimension.FromInches(4.125);
    public Dimension Width { get; set; } = Dimension.FromInches(21);
    public Dimension Depth { get; set; } = Dimension.FromInches(21);
    public DoweledDrawerBoxMaterial FrontMaterial { get; set; } = new("White", Dimension.FromInches(0.75), true);
    public DoweledDrawerBoxMaterial BackMaterial { get; set; } = new("White", Dimension.FromInches(0.75), true);
    public DoweledDrawerBoxMaterial SideMaterial { get; set; } = new("White", Dimension.FromInches(0.75), true);
    public DoweledDrawerBoxMaterial BottomMaterial { get; set; } = new("White", Dimension.FromInches(0.25), true);
    public bool MachineThicknessForUMSlides { get; set; } = false;
    public Dimension FrontBackHeightAdjustment { get; set; } = Dimension.Zero;

    public DoweledDrawerBox Build() {
        return new(Qty, Height, Width, Depth, FrontMaterial, BackMaterial, SideMaterial, BottomMaterial, MachineThicknessForUMSlides, FrontBackHeightAdjustment);
    }

}