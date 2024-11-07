using Domain.Orders.Builders;
using Domain.Orders.Entities.Products.Cabinets;
using Domain.Orders.Enums;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;
using FluentAssertions;

namespace ApplicationCore.Tests.Unit.Orders.Persistence.Cabinets;

public class BaseCabinetPersistenceTest : PersistenceTests {

    [Fact]
    public void InsertOrderWithBaseCabinet() {
        var cabinet = new BaseCabinetBuilder()
            .WithDoors(BaseCabinetDoors.NoDoors())
            .WithInside(BaseCabinetInside.Empty())
            .WithDrawers(HorizontalDrawerBank.None())
            .WithToeType(ToeType.NoToe)
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .Build();
        InsertAndQueryOrderWithProduct(cabinet);
    }

    [Fact]
    public void DeleteOrderWithBaseCabinet() {
        var cabinet = new BaseCabinetBuilder()
            .WithDoors(BaseCabinetDoors.NoDoors())
            .WithInside(BaseCabinetInside.Empty())
            .WithDrawers(HorizontalDrawerBank.None())
            .WithToeType(ToeType.NoToe)
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .Build();
        InsertAndDeleteOrderWithProduct(cabinet);
    }

    [Fact]
    public void InsertOrderWithBaseCabinetAndDrawerBoxesByOthers() {

        var cabinet = new BaseCabinetBuilder()
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
    public void InsertOrderWithBaseCabinetAndDrawerBoxes() {

        var cabinet = new BaseCabinetBuilder()
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
    public void InsertOrderWithBaseCabinetAndDoorsByOthers() {

        var cabinet = new BaseCabinetBuilder()
            .WithBoxOptions(null)
            .WithDoorConfiguration(new DoorsByOthers())
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .Build();

        var cab = InsertAndQueryOrderWithProduct<BaseCabinet>(cabinet);

        cab.DrawerBoxOptions.Should().BeNull();
        cab.DoorConfiguration.Switch(
            slab => Assert.Fail("Door configuration should be Doors By Others, but Slab was found"),
            mdf => Assert.Fail("Door configuration should be Doors By Others, but MDF was found"),
            byothers => { });

    }

    [Fact]
    public void InsertOrderWithGarageBaseCabinet() {
        var cabinet = new BaseCabinetBuilder()
            .WithIsGarage(true)
            .WithDoors(BaseCabinetDoors.NoDoors())
            .WithInside(BaseCabinetInside.Empty())
            .WithDrawers(HorizontalDrawerBank.None())
            .WithToeType(ToeType.NoToe)
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .Build();

        InsertAndQueryOrderWithProduct(cabinet);
    }

    [Fact]
    public void DeleteOrderWithGarageBaseCabinet() {
        var cabinet = new BaseCabinetBuilder()
            .WithIsGarage(true)
            .WithDoors(BaseCabinetDoors.NoDoors())
            .WithInside(BaseCabinetInside.Empty())
            .WithDrawers(HorizontalDrawerBank.None())
            .WithToeType(ToeType.NoToe)
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .Build();

        InsertAndDeleteOrderWithProduct(cabinet);
    }

    [Fact]
    public void InsertOrderWithBaseCabinetWithProductionNotes() {
        var cabinet = new BaseCabinetBuilder()
            .WithDoors(BaseCabinetDoors.NoDoors())
            .WithInside(BaseCabinetInside.Empty())
            .WithDrawers(HorizontalDrawerBank.None())
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
    public void DeleteOrderWithGarageBaseCabinetWithProductionNotes() {
        var cabinet = new BaseCabinetBuilder()
            .WithDoors(BaseCabinetDoors.NoDoors())
            .WithInside(BaseCabinetInside.Empty())
            .WithDrawers(HorizontalDrawerBank.None())
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
