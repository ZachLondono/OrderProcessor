using Domain.Orders.Enums;
using Domain.Orders.ValueObjects;
using Domain.Orders.Entities.Products.Closets;
using Domain.ValueObjects;

namespace Domain.Tests.Unit.Orders.Products.CustomVP;

public class CustomVPBuilder {

    public Guid Id { get; set; } = Guid.NewGuid();
    public int Qty { get; set; } = 0;
    public decimal UnitPrice { get; set; } = decimal.Zero;
    public int ProductNumber { get; set; } = 0;
    public string Room { get; set; } = string.Empty;
    public Dimension Width { get; set; } = Dimension.FromMillimeters(304.8);
    public Dimension Length { get; set; } = Dimension.FromMillimeters(2000);
    public ClosetMaterial Material { get; set; } = new("White", ClosetMaterialCore.ParticleBoard);
    public ClosetPaint? Paint { get; set; } = null;
    public string EdgeBandingColor { get; set; } = "White";
    public string Comment { get; set; } = string.Empty;
    public ClosetVerticalDrillingType DrillingType { get; set; } = ClosetVerticalDrillingType.DrilledThrough;
    public Dimension ExtendBack { get; set; } = Dimension.Zero;
    public Dimension ExtendFront { get; set; } = Dimension.Zero;
    public Dimension HoleDimensionFromBottom { get; set; } = Dimension.Zero;
    public Dimension HoleDimensionFromTop { get; set; } = Dimension.Zero;
    public Dimension TransitionHoleDimensionFromBottom { get; set; } = Dimension.Zero;
    public Dimension TransitionHoleDimensionFromTop { get; set; } = Dimension.Zero;
    public Dimension BottomNotchDepth { get; set; } = Dimension.Zero;
    public Dimension BottomNotchHeight { get; set; } = Dimension.Zero;
    public Dimension LEDChannelOffFront { get; set; } = Dimension.Zero;
    public Dimension LEDChannelWidth { get; set; } = Dimension.Zero;
    public Dimension LEDChannelDepth { get; set; } = Dimension.Zero;

    public CustomDrilledVerticalPanel Build() {

        return new(Id, Qty, UnitPrice, ProductNumber, Room, Width, Length, Material, Paint, EdgeBandingColor, Comment, DrillingType, ExtendBack, ExtendFront, HoleDimensionFromBottom, HoleDimensionFromTop, TransitionHoleDimensionFromBottom, TransitionHoleDimensionFromTop, BottomNotchDepth, BottomNotchHeight, LEDChannelOffFront, LEDChannelWidth, LEDChannelDepth);

    }

}
