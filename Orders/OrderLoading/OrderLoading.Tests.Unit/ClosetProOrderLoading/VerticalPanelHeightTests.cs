using Domain.ValueObjects;
using FluentAssertions;
using OrderLoading.ClosetProCSVCutList.Products.Verticals;

namespace OrderLoading.Tests.Unit.ClosetProOrderLoading;

public class VerticalPanelHeightTests {

    [Theory]
    [InlineData(2929, 2, 2931)]
    [InlineData(2930, 2, 2931)]
    [InlineData(2931, 2, 2931)]
    [InlineData(2932, 2, 2931)]
    [InlineData(2933, 2, 2931)]
    public void ShouldConvertHeightToCompliantHeight_WhenErrorIsWithinTolerances(double inputMM, double errorMM, double expectedMM) {

        var didWork = VerticalPanel.TryGetNearest32MMComplientHeight(Dimension.FromMillimeters(inputMM), out Dimension output, errorMM);

        didWork.Should().BeTrue();
        output.AsMillimeters().Should().Be(expectedMM);

    }


    [Theory]
    [InlineData(2929, 1)]
    [InlineData(2933, 1)]
    public void ShouldNotConvertHeightToCompliantHeight_WhenErrorIsNotWithinTolerances(double inputMM, double errorMM) {

        var didWork = VerticalPanel.TryGetNearest32MMComplientHeight(Dimension.FromMillimeters(inputMM), out _, errorMM);

        didWork.Should().BeFalse();

    }

}
