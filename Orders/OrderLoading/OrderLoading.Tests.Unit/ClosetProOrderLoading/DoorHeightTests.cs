using Domain.ValueObjects;
using FluentAssertions;
using OrderLoading.ClosetProCSVCutList.Products.Fronts;

namespace OrderLoading.Tests.Unit.ClosetProOrderLoading;

public class DoorHeightTests {

    [Theory]
    [InlineData(3.78, 3.1, 93)]
    [InlineData(5.04, 3.1, 125)]
    [InlineData(6.3, 3.1, 157)]
    [InlineData(7.56, 3.1, 189)]
    [InlineData(8.82, 3.1, 221)]
    [InlineData(10.08, 3.1, 253)]
    [InlineData(11.34, 3.1, 285)]
    [InlineData(12.6, 3.1, 317)]
    public void ShouldConvertHeightToCompliantHeight_WhenErrorIsWithinTolerances(double inputIN, double errorMM, double expectedMM) {

        var didWork = MelamineSlabFront.TryGetNearest32MMComplientHalfOverlayHeight(Dimension.FromInches(inputIN), out Dimension output, errorMM);

        didWork.Should().BeTrue();
        output.AsMillimeters().Should().Be(expectedMM);

    }

}
