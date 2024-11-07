using Domain.ValueObjects;

namespace ApplicationCore.Tests.Unit.Orders.Persistence.ClosetParts;

public class CustomVerticalPanelPersistenceTests : PersistenceTests {

    [Fact]
    public async Task InsertOrderWithCustomVerticalPanel() {
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

        await InsertAndQueryOrderWithProduct(part);
    }

    [Fact]
    public async Task DeleteOrderWithCustomVerticalPanel() {
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

        await InsertAndDeleteOrderWithProduct(part);
    }

    [Fact]
    public async Task InsertOrderWithCustomVerticalPanelWithProductionNotes() {
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

        await InsertAndQueryOrderWithProduct(part);
    }

    [Fact]
    public async Task DeleteOrderWithCustomVerticalPanelWithProductionNotes() {
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

        await InsertAndDeleteOrderWithProduct(part);
    }

}
