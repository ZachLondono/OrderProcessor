using Domain.Orders.Entities.Products.Closets;
using Domain.ValueObjects;
using FluentAssertions;
using OrderLoading.ClosetProCSVCutList.Products.Verticals;

namespace OrderLoading.Tests.Unit.ClosetProOrderLoading;

public class VerticalPanelSKUTests {

    [Fact]
    public void NoLEDCenterPanel() {

        var vp = CreateVP(VerticalPanelDrilling.DrilledThrough, VerticalPanelLEDChannel.None);

        var product = vp.ToProduct(Dimension.Zero, Dimension.Zero) as ClosetPart;

        product!.SKU.Should().Be("PC");

    }

    [Fact]
    public void NoLEDFinishedLeft() {

        var vp = CreateVP(VerticalPanelDrilling.FinishedLeft, VerticalPanelLEDChannel.None);

        var product = vp.ToProduct(Dimension.Zero, Dimension.Zero) as ClosetPart;

        product!.SKU.Should().Be("PE");

    }

    [Fact]
    public void NoLEDFinishedRight() {

        var vp = CreateVP(VerticalPanelDrilling.FinishedRight, VerticalPanelLEDChannel.None);

        var product = vp.ToProduct(Dimension.Zero, Dimension.Zero) as ClosetPart;

        product!.SKU.Should().Be("PE");

    }

    [Fact]
    public void RightLEDFinishedLeft() {

        var vp = CreateVP(VerticalPanelDrilling.FinishedLeft, VerticalPanelLEDChannel.Right);

        var product = vp.ToProduct(Dimension.Zero, Dimension.Zero) as ClosetPart;

        product!.SKU.Should().Be("PE-LED");

    }

    [Fact]
    public void LeftLEDFinishedRight() {

        var vp = CreateVP(VerticalPanelDrilling.FinishedRight, VerticalPanelLEDChannel.Left);

        var product = vp.ToProduct(Dimension.Zero, Dimension.Zero) as ClosetPart;

        product!.SKU.Should().Be("PE-LED");

    }

    [Fact]
    public void RightLEDFinishedRight() {

        var vp = CreateVP(VerticalPanelDrilling.FinishedRight, VerticalPanelLEDChannel.Right);

        var action = () => vp.ToProduct(Dimension.Zero, Dimension.Zero);

        action.Should().Throw<NotSupportedException>();

    }

    [Fact]
    public void LeftLEDFinishedLeft() {

        var vp = CreateVP(VerticalPanelDrilling.FinishedLeft, VerticalPanelLEDChannel.Left);

        var action = () => vp.ToProduct(Dimension.Zero, Dimension.Zero);

        action.Should().Throw<NotSupportedException>();

    }

    [Fact]
    public void BothLEDFinishedLeft() {

        var vp = CreateVP(VerticalPanelDrilling.FinishedLeft, VerticalPanelLEDChannel.Both);

        var action = () => vp.ToProduct(Dimension.Zero, Dimension.Zero);

        action.Should().Throw<NotSupportedException>();

    }

    [Fact]
    public void BothLEDFinishedRight() {

        var vp = CreateVP(VerticalPanelDrilling.FinishedRight, VerticalPanelLEDChannel.Both);

        var action = () => vp.ToProduct(Dimension.Zero, Dimension.Zero);

        action.Should().Throw<NotSupportedException>();

    }

    [Fact]
    public void BothLEDCenter() {

        var vp = CreateVP(VerticalPanelDrilling.DrilledThrough, VerticalPanelLEDChannel.Both);

        var action = () => vp.ToProduct(Dimension.Zero, Dimension.Zero);

        action.Should().Throw<NotSupportedException>();

    }

    [Fact]
    public void LeftLEDCenter() {

        var vp = CreateVP(VerticalPanelDrilling.DrilledThrough, VerticalPanelLEDChannel.Left);

        var action = () => vp.ToProduct(Dimension.Zero, Dimension.Zero);

        action.Should().Throw<NotSupportedException>();

    }

    [Fact]
    public void RightLEDCenter() {

        var vp = CreateVP(VerticalPanelDrilling.DrilledThrough, VerticalPanelLEDChannel.Right);

        var action = () => vp.ToProduct(Dimension.Zero, Dimension.Zero);

        action.Should().Throw<NotSupportedException>();

    }

    private VerticalPanel CreateVP(VerticalPanelDrilling drilling, VerticalPanelLEDChannel led) {

        return new VerticalPanel() {
            Qty = 1,
            Color = "White",
            EdgeBandingColor = "White",
            Room = "",
            UnitPrice = 0,
            PartNumber = 1,
            Height = Dimension.FromInches(83.898),
            Depth = Dimension.FromInches(12),
            WallHung = false,
            ExtendBack = false,
            HasBottomRadius = false,
            BaseNotch = new(Dimension.Zero, Dimension.Zero),

            Drilling = drilling,
            LEDChannel = led
        };

    }

}
