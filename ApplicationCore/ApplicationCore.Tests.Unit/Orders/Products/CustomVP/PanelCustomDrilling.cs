using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
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

        public CustomDrilledVerticalPanel Build() {

            return new(Id, Qty, UnitPrice, ProductNumber, Room, Width, Length, Material, Paint, EdgeBandingColor, Comment, DrillingType, ExtendBack, ExtendFront, HoleDimensionFromBottom, HoleDimensionFromTop, TransitionHoleDimensionFromBottom, TransitionHoleDimensionFromTop, BottomNotchDepth, BottomNotchHeight);

        }

    }

}
