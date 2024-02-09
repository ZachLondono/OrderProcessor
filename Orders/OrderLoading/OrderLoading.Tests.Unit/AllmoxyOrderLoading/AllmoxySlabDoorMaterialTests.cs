using OrderLoading.LoadAllmoxyOrderData.AllmoxyXMLModels;
using Domain.Orders.Enums;
using Domain.Orders.ValueObjects;
using FluentAssertions;

namespace OrderLoading.Tests.Unit.AllmoxyOrderLoading;

public class AllmoxySlabDoorMaterialTests {

    [Fact]
    public void WhiteMelamineCabinet_WithMatchingSlabDoors() {

        // Arrange
        var fronts = new CabinetFrontsModel() {
            Style = "Slab",
            Type = "Slab",
            Color = "Match Finish",
            FinishType = "match",
        };
        string melamineColor = "White";
        var boxMaterial = new CabinetMaterial(melamineColor, CabinetMaterialFinishType.Melamine, CabinetMaterialCore.ParticleBoard);
        var finishMaterial = CabinetFinishMaterial.UnPaintedMelaPB(melamineColor);

        // Act
        var result = CabinetModelBase.GetSlabDoorMaterial(fronts, boxMaterial, finishMaterial);

        // Assert
        result.Should().NotBeNull();
        result.Finish.Should().Be(melamineColor);
        result.FinishType.Should().Be(CabinetMaterialFinishType.Melamine);
        result.Core.Should().Be(CabinetMaterialCore.ParticleBoard);
        result.PaintColor.Should().BeNull();

    }

    [Fact]
    public void WhiteMelamineCabinet_WithNoDoors() {

        // Arrange
        var fronts = new CabinetFrontsModel() {
            Style = "None",
            Type = "None",
            Color = "Match Finish",
            FinishType = "match",
        };
        string melamineColor = "White";
        var boxMaterial = new CabinetMaterial(melamineColor, CabinetMaterialFinishType.Melamine, CabinetMaterialCore.ParticleBoard);
        var finishMaterial = CabinetFinishMaterial.UnPaintedMelaPB(melamineColor);

        // Act
        var result = CabinetModelBase.GetSlabDoorMaterial(fronts, boxMaterial, finishMaterial);

        // Assert
        result.Should().BeNull();

    }

    [Fact]
    public void PreFinPlywoodCabinet_WithMatchingSlabDoors() {

        // Arrange
        var fronts = new CabinetFrontsModel() {
            Style = "Slab",
            Type = "Slab",
            Color = "Match Finish",
            FinishType = "match",
        };
        string veneerFinish = "PRE";
        var boxMaterial = new CabinetMaterial(veneerFinish, CabinetMaterialFinishType.Veneer, CabinetMaterialCore.Plywood);
        var finishMaterial = CabinetFinishMaterial.UnPaintedVeneerPly(veneerFinish);

        // Act
        var result = CabinetModelBase.GetSlabDoorMaterial(fronts, boxMaterial, finishMaterial);

        // Assert
        result.Should().NotBeNull();
        result.Finish.Should().Be(veneerFinish);
        result.FinishType.Should().Be(CabinetMaterialFinishType.Veneer);
        result.Core.Should().Be(CabinetMaterialCore.Plywood);
        result.PaintColor.Should().BeNull();

    }

    [Fact]
    public void PreFinPlywoodCabinet_WithNoDoors() {

        // Arrange
        var fronts = new CabinetFrontsModel() {
            Style = "None",
            Type = "None",
            Color = "Match Finish",
            FinishType = "match",
        };
        string veneerFinish = "Pre";
        var boxMaterial = new CabinetMaterial(veneerFinish, CabinetMaterialFinishType.Veneer, CabinetMaterialCore.Plywood);
        var finishMaterial = CabinetFinishMaterial.UnPaintedVeneerPly(veneerFinish);

        // Act
        var result = CabinetModelBase.GetSlabDoorMaterial(fronts, boxMaterial, finishMaterial);

        // Assert
        result.Should().BeNull();

    }

    [Fact]
    public void PBBoxWithUnPaintedPlywoodFinishedSides_WithPaintedSlabDoors() {

        // Arrange
        var fronts = new CabinetFrontsModel() {
            Style = "Slab",
            Type = "Slab",
            Color = "Paint Color",
            FinishType = "paint",
        };
        string veneerFinish = "Pre";
        string melaFinish = "White";
        var boxMaterial = new CabinetMaterial(melaFinish, CabinetMaterialFinishType.Melamine, CabinetMaterialCore.ParticleBoard);
        var finishMaterial = CabinetFinishMaterial.UnPaintedVeneerPly(veneerFinish);

        // Act
        var result = CabinetModelBase.GetSlabDoorMaterial(fronts, boxMaterial, finishMaterial);

        // Assert
        result.Should().NotBeNull();
        result.Finish.Should().Be(veneerFinish);
        result.FinishType.Should().Be(CabinetMaterialFinishType.Paint);
        result.Core.Should().Be(CabinetMaterialCore.Plywood);
        result.PaintColor.Should().Be(fronts.Color);

    }

    [Fact]
    public void PBBoxWithUnPaintedPlywoodFinishedSides_WithMatchingSlabDoors() {
        // Arrange
        var fronts = new CabinetFrontsModel() {
            Style = "Slab",
            Type = "Slab",
            Color = "match",
            FinishType = "match",
        };
        string veneerFinish = "Pre";
        string melaFinish = "White";
        var boxMaterial = new CabinetMaterial(melaFinish, CabinetMaterialFinishType.Melamine, CabinetMaterialCore.ParticleBoard);
        var finishMaterial = CabinetFinishMaterial.UnPaintedVeneerPly(veneerFinish);

        // Act
        var result = CabinetModelBase.GetSlabDoorMaterial(fronts, boxMaterial, finishMaterial);

        // Assert
        result.Should().NotBeNull();
        result.Finish.Should().Be(veneerFinish);
        result.FinishType.Should().Be(CabinetMaterialFinishType.Veneer);
        result.Core.Should().Be(CabinetMaterialCore.Plywood);
        result.PaintColor.Should().BeNull();
    }

    [Fact]
    public void PBBoxWithPaintedPlywoodFinishedSides_WithMatchingSlabDoors() {
        // Arrange
        var fronts = new CabinetFrontsModel() {
            Style = "Slab",
            Type = "Slab",
            Color = "match",
            FinishType = "match",
        };
        string veneerFinish = "Pre";
        string melaFinish = "White";
        string paintFinish = "Paint Color";
        var boxMaterial = new CabinetMaterial(melaFinish, CabinetMaterialFinishType.Melamine, CabinetMaterialCore.ParticleBoard);
        var finishMaterial = CabinetFinishMaterial.PaintedPly(veneerFinish, paintFinish);

        // Act
        var result = CabinetModelBase.GetSlabDoorMaterial(fronts, boxMaterial, finishMaterial);

        // Assert
        result.Should().NotBeNull();
        result.Finish.Should().Be(veneerFinish);
        result.FinishType.Should().Be(CabinetMaterialFinishType.Paint);
        result.Core.Should().Be(CabinetMaterialCore.Plywood);
        result.PaintColor.Should().Be(paintFinish);
    }

}
