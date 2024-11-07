using Domain.Orders.Builders;
using Domain.Orders.Enums;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;
using FluentAssertions;

namespace ApplicationCore.Tests.Unit.Orders.Persistence.Cabinets;

public class BlindBaseCabinetPersistenceTest : PersistenceTests {

    [Fact]
    public async Task InsertOrderWithBlindBaseCabinet() {
        var cabinet = new BlindBaseCabinetBuilder()
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .Build();
        await InsertAndQueryOrderWithProduct(cabinet);
    }

    [Fact]
    public async Task DeleteOrderWithBlindBaseCabinet() {
        var cabinet = new BlindBaseCabinetBuilder()
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .Build();
        await InsertAndDeleteOrderWithProduct(cabinet);
    }

    [Fact]
    public async Task InsertOrderWithBlindBaseCabinetAndDrawerBoxesByOthers() {

        var cabinet = new BlindBaseCabinetBuilder()
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

        var cabinet = new BlindBaseCabinetBuilder()
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
    public async Task InsertOrderWithBlindBaseCabinetAndSlab() {

        var cabinet = new BlindBaseCabinetBuilder()
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
    public async Task InsertOrderWithBlindBaseCabinetWithProductionNotes() {
        var cabinet = new BlindBaseCabinetBuilder()
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .WithProductionNotes(new() { "A", "B", "C" })
            .Build();

        await InsertAndQueryOrderWithProduct(cabinet);
    }

    [Fact]
    public async Task DeleteOrderWithBlindBaseCabinetWithProductionNotes() {
        var cabinet = new BlindBaseCabinetBuilder()
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .WithProductionNotes(new() { "A", "B", "C" })
            .Build();

        await InsertAndDeleteOrderWithProduct(cabinet);
    }

}
