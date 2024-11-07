using Domain.Orders.Builders;
using Domain.Orders.Enums;
using Domain.Orders.ValueObjects;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;
using FluentAssertions;

namespace ApplicationCore.Tests.Unit.Orders.Persistence.Cabinets;

public class TallCabinetPersistenceTest : PersistenceTests {

    [Fact]
    public async Task InsertOrderWithTallCabinet() {
        var cabinet = new TallCabinetBuilder()
            .WithDoors(TallCabinetDoors.NoDoors())
            .WithInside(TallCabinetInside.Empty())
            .WithToeType(ToeType.NoToe)
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .Build();
        await InsertAndQueryOrderWithProduct(cabinet);
    }

    [Fact]
    public async Task DeleteOrderWithTallCabinet() {
        var cabinet = new TallCabinetBuilder()
            .WithDoors(TallCabinetDoors.NoDoors())
            .WithToeType(ToeType.NoToe)
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .Build();
        await InsertAndDeleteOrderWithProduct(cabinet);
    }

    [Fact]
    public async Task InsertOrderWithBlindBaseCabinetAndDrawerBoxesByOthers() {

        var cabinet = new TallCabinetBuilder()
            .WithBoxOptions(null)
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .Build();

        var cab = await InsertAndQueryOrderWithProduct(cabinet);

        cab.DrawerBoxOptions.Should().BeNull();

    }

    [Fact]
    public async Task InsertOrderWithBlindBaseCabinetAndDrawerBoxes() {

        var cabinet = new TallCabinetBuilder()
            .WithBoxOptions(new CabinetDrawerBoxOptions(CabinetDrawerBoxMaterial.SolidBirch, DrawerSlideType.SideMount))
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .Build();

        var cab = await InsertAndQueryOrderWithProduct(cabinet);

        cab.DrawerBoxOptions.Should().NotBeNull();
        cab.DrawerBoxOptions!.Material.Should().Be(CabinetDrawerBoxMaterial.SolidBirch);
        cab.DrawerBoxOptions!.SlideType.Should().Be(DrawerSlideType.SideMount);

    }

    [Fact]
    public async Task InsertOrderWithBlindBaseCabinetAndNoMDFDoors() {

        var cabinet = new TallCabinetBuilder()
            .WithBoxOptions(null)
            .WithDoorConfiguration(new CabinetSlabDoorMaterial("Finish", CabinetMaterialFinishType.Melamine, CabinetMaterialCore.ParticleBoard))
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .Build();

        var cab = await InsertAndQueryOrderWithProduct(cabinet);

        cab.DrawerBoxOptions.Should().BeNull();
        cab.DoorConfiguration.Switch(
            slab => { },
            mdf => Assert.Fail("Door configuration should have been Slab, but was MDF"),
            byothers => Assert.Fail("Door configuration should have been Slab, but was Doors By Others"));

    }

    [Fact]
    public async Task InsertOrderWithGarageTallCabinet() {
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

        await InsertAndQueryOrderWithProduct(cabinet);
    }

    [Fact]
    public async Task DeleteOrderWithGarageTallCabinet() {
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

        await InsertAndDeleteOrderWithProduct(cabinet);
    }

    [Fact]
    public async Task InsertOrderWithTallCabinetWithProductionNotes() {
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

        await InsertAndQueryOrderWithProduct(cabinet);
    }

    [Fact]
    public async Task DeleteOrderWithTallCabinetWithProductionNotes() {
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

        await InsertAndDeleteOrderWithProduct(cabinet);
    }

}
