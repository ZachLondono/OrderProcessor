using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Tests.Unit.Orders.Persistence.ClosetParts;

public class CustomVerticalPanelPersistenceTests : PersistenceTests {

    [Fact]
    public void InsertOrderWithCustomVerticalPanel() {
        var part = new CustomVPBuilder() {
            Width = Dimension.FromMillimeters(400),
            Length = Dimension.FromMillimeters(2000),
            HoleDimensionFromBottom = Dimension.FromMillimeters(750),
            HoleDimensionFromTop = Dimension.FromMillimeters(750),
            TransitionHoleDimensionFromBottom = Dimension.FromMillimeters(250),
            TransitionHoleDimensionFromTop = Dimension.FromMillimeters(250),
            BottomNotchDepth = Dimension.FromMillimeters(50),
            BottomNotchHeight = Dimension.FromMillimeters(50),
            LEDChannelOffFront = Dimension.FromMillimeters(50),
            LEDChannelWidth = Dimension.FromMillimeters(19),
            LEDChannelDepth = Dimension.FromMillimeters(8),
        }.Build();

        InsertAndQueryOrderWithProduct(part);
    }

    [Fact]
    public void DeleteOrderWithCustomVerticalPanel() {
        var part = new CustomVPBuilder() {
            Width = Dimension.FromMillimeters(400),
            Length = Dimension.FromMillimeters(2000),
            HoleDimensionFromBottom = Dimension.FromMillimeters(750),
            HoleDimensionFromTop = Dimension.FromMillimeters(750),
            TransitionHoleDimensionFromBottom = Dimension.FromMillimeters(250),
            TransitionHoleDimensionFromTop = Dimension.FromMillimeters(250),
            BottomNotchDepth = Dimension.FromMillimeters(50),
            BottomNotchHeight = Dimension.FromMillimeters(50),
            LEDChannelOffFront = Dimension.FromMillimeters(50),
            LEDChannelWidth = Dimension.FromMillimeters(19),
            LEDChannelDepth = Dimension.FromMillimeters(8),
        }.Build();

        InsertAndDeleteOrderWithProduct(part);
    }

    [Fact]
    public void InsertOrderWithCustomVerticalPanelWithProductionNotes() {
        var part = new CustomVPBuilder() {
            Width = Dimension.FromMillimeters(400),
            Length = Dimension.FromMillimeters(2000),
            HoleDimensionFromBottom = Dimension.FromMillimeters(750),
            HoleDimensionFromTop = Dimension.FromMillimeters(750),
            TransitionHoleDimensionFromBottom = Dimension.FromMillimeters(250),
            TransitionHoleDimensionFromTop = Dimension.FromMillimeters(250),
            BottomNotchDepth = Dimension.FromMillimeters(50),
            BottomNotchHeight = Dimension.FromMillimeters(50),
            LEDChannelOffFront = Dimension.FromMillimeters(50),
            LEDChannelWidth = Dimension.FromMillimeters(19),
            LEDChannelDepth = Dimension.FromMillimeters(8),
        }.Build();

        part.ProductionNotes = new() { "A", "B", "C" };

        InsertAndQueryOrderWithProduct(part);
    }

    [Fact]
    public void DeleteOrderWithCustomVerticalPanelWithProductionNotes() {
        var part = new CustomVPBuilder() {
            Width = Dimension.FromMillimeters(400),
            Length = Dimension.FromMillimeters(2000),
            HoleDimensionFromBottom = Dimension.FromMillimeters(750),
            HoleDimensionFromTop = Dimension.FromMillimeters(750),
            TransitionHoleDimensionFromBottom = Dimension.FromMillimeters(250),
            TransitionHoleDimensionFromTop = Dimension.FromMillimeters(250),
            BottomNotchDepth = Dimension.FromMillimeters(50),
            BottomNotchHeight = Dimension.FromMillimeters(50),
            LEDChannelOffFront = Dimension.FromMillimeters(50),
            LEDChannelWidth = Dimension.FromMillimeters(19),
            LEDChannelDepth = Dimension.FromMillimeters(8),
        }.Build();

        part.ProductionNotes = new() { "A", "B", "C" };

        InsertAndDeleteOrderWithProduct(part);
    }
}
