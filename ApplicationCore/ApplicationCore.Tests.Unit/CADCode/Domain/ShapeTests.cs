using ApplicationCore.Features.CNC.Shared;
using FluentAssertions;

namespace ApplicationCore.Tests.Unit.CADCode.Domain;

public class ShapeTests {

    private Shape _sut = new();

    [Fact]
    public void AddFillet_ShouldThrow_WhenStartingWithFillet() {

        Action execute = () => _sut.AddFillet(2);

        execute.Should().Throw<InvalidOperationException>();

    }

    [Fact]
    public void AddFillet_ShouldNotThrow_WhenPrecededByLine() {

        var sut = new Shape();
        sut.AddLine(new(0, 0), new(0, 50));

        Action execute = () => sut.AddFillet(2);

        execute.Should().NotThrow<InvalidOperationException>();

    }

    [Fact]
    public void AddLine_ShouldNotThrow_WhenSegmentIsValid() {
        var sut = new Shape();

        Action execute = () => sut.AddLine(new(1, 50), new(50, 50));

        execute.Should().NotThrow<InvalidOperationException>();
    }

    [Fact]
    public void AddLine_ShouldThrow_WhenSegmentsDontConnect() {

        var sut = new Shape();
        sut.AddLine(new(0, 0), new(0, 50));

        Action execute = () => sut.AddLine(new(1, 50), new(50, 50));

        execute.Should().Throw<InvalidOperationException>();

    }

    [Fact]
    public void AddLine_ShouldThrow_WhenSegmentLengthIsZero() {

        var sut = new Shape();

        Action execute = () => sut.AddLine(new(50, 50), new(50, 50));

        execute.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GetSegments_ShouldThrow_WhenShapeIsNotClosed() {

        var sut = new Shape();
        sut.AddLine(new(0, 0), new(0, 50));
        sut.AddLine(new(0, 50), new(50, 50));

        Action execute = () => sut.GetSegments();

        sut.IsClosed.Should().BeFalse();
        execute.Should().Throw<InvalidOperationException>();

    }

    [Fact]
    public void GetSegments_ShouldThrow_WhenShapeEndsInFillet() {

        var sut = new Shape();
        sut.AddLine(new(0, 0), new(0, 50));
        sut.AddLine(new(0, 50), new(50, 50));
        sut.AddLine(new(50, 50), new(50, 0));
        sut.AddLine(new(50, 0), new(0, 0));
        sut.AddFillet(2);

        Action execute = () => sut.GetSegments();

        sut.IsClosed.Should().BeFalse();
        execute.Should().Throw<InvalidOperationException>();

    }

}
