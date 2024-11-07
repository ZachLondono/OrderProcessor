using Domain.Orders.Builders;
using Domain.Orders.Enums;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;
using FluentAssertions;

namespace ApplicationCore.Tests.Unit.Orders.Persistence.Cabinets;

public class DrawerBaseCabinetPersistenceTest : PersistenceTests {

    [Fact]
    public async Task InsertOrderWithDrawerBaseCabinet() {
        var cabinet = new DrawerBaseCabinetBuilder()
            .WithDrawers(VerticalDrawerBank.None())
            .WithToeType(ToeType.NoToe)
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .Build();
        await InsertAndQueryOrderWithProduct(cabinet);
    }

    [Fact]
    public async Task DeleteOrderWithDrawerBaseCabinet() {
        var cabinet = new DrawerBaseCabinetBuilder()
            .WithDrawers(VerticalDrawerBank.None())
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

        var cabinet = new DrawerBaseCabinetBuilder()
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

        var cabinet = new DrawerBaseCabinetBuilder()
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

        var cabinet = new DrawerBaseCabinetBuilder()
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
    public async Task InsertOrderWithGarageDrawerBaseCabinet() {
        var cabinet = new DrawerBaseCabinetBuilder()
            .WithIsGarage(true)
            .WithDrawers(VerticalDrawerBank.None())
            .WithToeType(ToeType.NoToe)
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .Build();

        await InsertAndQueryOrderWithProduct(cabinet);
    }

    [Fact]
    public async Task DeleteOrderWithGarageDrawerBaseCabinet() {
        var cabinet = new DrawerBaseCabinetBuilder()
            .WithIsGarage(true)
            .WithDrawers(VerticalDrawerBank.None())
            .WithToeType(ToeType.NoToe)
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .Build();

        await InsertAndDeleteOrderWithProduct(cabinet);
    }

    [Fact]
    public async Task InsertOrderWithDrawerBaseCabinetWithProductionNotes() {
        var cabinet = new DrawerBaseCabinetBuilder()
            .WithDrawers(VerticalDrawerBank.None())
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
    public async Task DeleteOrderWithGarageDrawerBaseCabinetWithProductionNotes() {
        var cabinet = new DrawerBaseCabinetBuilder()
            .WithDrawers(VerticalDrawerBank.None())
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
