using Domain.Orders.Builders;
using Domain.Orders.Enums;
using Domain.Orders.ValueObjects;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;
using FluentAssertions;

namespace ApplicationCore.Tests.Unit.Orders.Persistence.Cabinets;

public class TallCabinetPersistenceTest : PersistenceTests {

    [Fact]
    public void InsertOrderWithTallCabinet() {
        var cabinet = new TallCabinetBuilder()
            .WithDoors(TallCabinetDoors.NoDoors())
            .WithInside(TallCabinetInside.Empty())
            .WithToeType(ToeType.NoToe)
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .Build();
        InsertAndQueryOrderWithProduct(cabinet);
    }

    [Fact]
    public void DeleteOrderWithTallCabinet() {
        var cabinet = new TallCabinetBuilder()
            .WithDoors(TallCabinetDoors.NoDoors())
            .WithToeType(ToeType.NoToe)
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .Build();
        InsertAndDeleteOrderWithProduct(cabinet);
    }

    [Fact]
    public void InsertOrderWithBlindBaseCabinetAndDrawerBoxesByOthers() {

        var cabinet = new TallCabinetBuilder()
            .WithBoxOptions(null)
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .Build();

        var cab = InsertAndQueryOrderWithProduct(cabinet);

        cab.DrawerBoxOptions.Should().BeNull();

    }

    [Fact]
    public void InsertOrderWithBlindBaseCabinetAndDrawerBoxes() {

        var cabinet = new TallCabinetBuilder()
            .WithBoxOptions(new CabinetDrawerBoxOptions(CabinetDrawerBoxMaterial.SolidBirch, DrawerSlideType.SideMount))
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .Build();

        var cab = InsertAndQueryOrderWithProduct(cabinet);

        cab.DrawerBoxOptions.Should().NotBeNull();
        cab.DrawerBoxOptions!.Material.Should().Be(CabinetDrawerBoxMaterial.SolidBirch);
        cab.DrawerBoxOptions!.SlideType.Should().Be(DrawerSlideType.SideMount);

    }

    [Fact]
    public void InsertOrderWithBlindBaseCabinetAndNoMDFDoors() {

        var cabinet = new TallCabinetBuilder()
            .WithBoxOptions(null)
            .WithMDFDoorOptions(null)
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .Build();

        var cab = InsertAndQueryOrderWithProduct(cabinet);

        cab.DrawerBoxOptions.Should().BeNull();
        cab.MDFDoorOptions.Should().BeNull();

    }

    [Fact]
    public void InsertOrderWithGarageTallCabinet() {
        var cabinet = new TallCabinetBuilder()
            .WithIsGarage(true)
            .WithDoors(TallCabinetDoors.NoDoors())
            .WithInside(TallCabinetInside.Empty())
            .WithToeType(ToeType.NoToe)
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .Build();

        InsertAndQueryOrderWithProduct(cabinet);
    }

    [Fact]
    public void DeleteOrderWithGarageTallCabinet() {
        var cabinet = new TallCabinetBuilder()
            .WithIsGarage(true)
            .WithDoors(TallCabinetDoors.NoDoors())
            .WithInside(TallCabinetInside.Empty())
            .WithToeType(ToeType.NoToe)
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .Build();

        InsertAndDeleteOrderWithProduct(cabinet);
    }

    [Fact]
    public void InsertOrderWithTallCabinetWithProductionNotes() {
        var cabinet = new TallCabinetBuilder()
            .WithDoors(TallCabinetDoors.NoDoors())
            .WithInside(TallCabinetInside.Empty())
            .WithToeType(ToeType.NoToe)
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .WithProductionNotes(new() { "A", "B", "C" })
            .Build();

        InsertAndQueryOrderWithProduct(cabinet);
    }

    [Fact]
    public void DeleteOrderWithTallCabinetWithProductionNotes() {
        var cabinet = new TallCabinetBuilder()
            .WithDoors(TallCabinetDoors.NoDoors())
            .WithInside(TallCabinetInside.Empty())
            .WithToeType(ToeType.NoToe)
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .WithProductionNotes(new() { "A", "B", "C" })
            .Build();

        InsertAndDeleteOrderWithProduct(cabinet);
    }

}
