using ApplicationCore.Shared.Domain;
using FluentAssertions;

namespace ApplicationCore.Tests.Unit.Orders.Products.CustomVP;

public class PanelCustomDrilling {

    [Fact]
    public void Should_NotContain_CNCParts_WhenNoCustomizations() {

        // Arrange
        var sut = new CustomVPBuilder() {
            ExtendBack = Dimension.Zero,
            ExtendFront = Dimension.Zero,
            HoleDimensionFromBottom = Dimension.Zero,
            HoleDimensionFromTop = Dimension.Zero,
            TransitionHoleDimensionFromBottom = Dimension.Zero,
            TransitionHoleDimensionFromTop = Dimension.Zero,
            BottomNotchDepth = Dimension.Zero,
            BottomNotchHeight = Dimension.Zero,
        }.Build();

        // Act
        var containsCNCParts = sut.ContainsCNCParts();
        var containsPPProducts = sut.ContainsPPProducts();

        // Assert
        containsCNCParts.Should().BeFalse();
        containsPPProducts.Should().BeTrue();

    }

    [Fact]
    public void Should_NotContain_CNCParts_WhenOnlyTransitionHolesSet() {

        // Arrange
        var sut = new CustomVPBuilder() {
            ExtendBack = Dimension.Zero,
            ExtendFront = Dimension.Zero,
            HoleDimensionFromBottom = Dimension.Zero,
            HoleDimensionFromTop = Dimension.Zero,
            TransitionHoleDimensionFromBottom = Dimension.FromInches(10),
            TransitionHoleDimensionFromTop = Dimension.FromInches(10),
            BottomNotchDepth = Dimension.Zero,
            BottomNotchHeight = Dimension.Zero,
        }.Build();

        // Act
        var containsCNCParts = sut.ContainsCNCParts();
        var containsPPProducts = sut.ContainsPPProducts();

        // Assert
        containsCNCParts.Should().BeFalse();
        containsPPProducts.Should().BeTrue();

    }

    [Fact]
    public void Should_NotContain_CNCParts_WhenOnlyStandardHolesSet() {

        // Arrange
        var sut = new CustomVPBuilder() {
            ExtendBack = Dimension.Zero,
            ExtendFront = Dimension.Zero,
            TransitionHoleDimensionFromBottom = Dimension.Zero,
            TransitionHoleDimensionFromTop = Dimension.Zero,
            HoleDimensionFromBottom = Dimension.FromInches(10),
            HoleDimensionFromTop = Dimension.FromInches(10),
            BottomNotchDepth = Dimension.Zero,
            BottomNotchHeight = Dimension.Zero,
        }.Build();

        // Act
        var containsCNCParts = sut.ContainsCNCParts();
        var containsPPProducts = sut.ContainsPPProducts();

        // Assert
        containsCNCParts.Should().BeFalse();
        containsPPProducts.Should().BeTrue();

    }

    [Fact]
    public void Should_NotContain_CNCParts_WhenOnlyExtendFrontBackAreSet() {

        // Arrange
        var sut = new CustomVPBuilder() {
            ExtendBack = Dimension.FromInches(2),
            ExtendFront = Dimension.FromInches(2),
            TransitionHoleDimensionFromBottom = Dimension.Zero,
            TransitionHoleDimensionFromTop = Dimension.Zero,
            HoleDimensionFromBottom = Dimension.Zero,
            HoleDimensionFromTop = Dimension.Zero,
            BottomNotchDepth = Dimension.Zero,
            BottomNotchHeight = Dimension.Zero,
        }.Build();

        // Act
        var containsCNCParts = sut.ContainsCNCParts();
        var containsPPProducts = sut.ContainsPPProducts();

        // Assert
        containsCNCParts.Should().BeFalse();
        containsPPProducts.Should().BeTrue();

    }

    [Fact]
    public void Should_Contain_CNCParts_WhenStandardHolesAndBottomNotchAreSet() {

        // Arrange
        var sut = new CustomVPBuilder() {
            ExtendBack = Dimension.Zero,
            ExtendFront = Dimension.Zero,
            TransitionHoleDimensionFromBottom = Dimension.Zero,
            TransitionHoleDimensionFromTop = Dimension.Zero,
            HoleDimensionFromBottom = Dimension.FromInches(10),
            HoleDimensionFromTop = Dimension.FromInches(10),
            BottomNotchDepth = Dimension.FromInches(3),
            BottomNotchHeight = Dimension.FromInches(3),
        }.Build();

        // Act
        var containsCNCParts = sut.ContainsCNCParts();
        var containsPPProducts = sut.ContainsPPProducts();

        // Assert
        containsCNCParts.Should().BeTrue();
        containsPPProducts.Should().BeFalse();

    }

    [Fact]
    public void Should_Contain_CNCParts_WhenStandardHolesAndTransitionHolesAreSet() {

        // Arrange
        var sut = new CustomVPBuilder() {
            ExtendBack = Dimension.Zero,
            ExtendFront = Dimension.Zero,
            TransitionHoleDimensionFromBottom = Dimension.FromInches(5),
            TransitionHoleDimensionFromTop = Dimension.FromInches(5),
            HoleDimensionFromBottom = Dimension.FromInches(10),
            HoleDimensionFromTop = Dimension.FromInches(10),
            BottomNotchDepth = Dimension.Zero,
            BottomNotchHeight = Dimension.Zero,
        }.Build();

        // Act
        var containsCNCParts = sut.ContainsCNCParts();
        var containsPPProducts = sut.ContainsPPProducts();

        // Assert
        containsCNCParts.Should().BeTrue();
        containsPPProducts.Should().BeFalse();

    }

}
