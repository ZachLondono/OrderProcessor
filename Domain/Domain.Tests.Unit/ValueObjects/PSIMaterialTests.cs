using Domain.ValueObjects;
using FluentAssertions;

namespace Domain.Tests.Unit.ValueObjects;

public class PSIMaterialTests {

    [Fact]
    public void Should_ParseValid1WordColorMaterial() {

        // Arrange
        string side1Color = "White";
        string side1Finish = "White";
        string side2Color = "White";
        string side2Finish = "White";
        string coreType = "PB";
        double thickness = 19;
        string materialName = CreateMaterialName(side1Color, side1Finish, side2Color, side2Finish, coreType, thickness);

        // Act
        var isValid = PSIMaterial.TryParse(materialName, out PSIMaterial psiMat);

        // Assert
        isValid.Should().BeTrue();
        psiMat.Side1Color.Should().Be(side1Color);
        psiMat.Side1FinishType.Should().Be(side1Finish);
        psiMat.Side2Color.Should().Be(side2Color);
        psiMat.Side2FinishType.Should().Be(side2Finish);
        psiMat.CoreType.Should().Be(coreType);
        psiMat.Thickness.Should().Be(thickness);

    }

    [Fact]
    public void Should_ParseValid2WordColorMaterial() {

        // Arrange
        string side1Color = "Brushed Aluminum";
        string side1Finish = "mela";
        string side2Color = "Brushed Aluminum";
        string side2Finish = "mela";
        string coreType = "PB";
        double thickness = 19;
        string materialName = CreateMaterialName(side1Color, side1Finish, side2Color, side2Finish, coreType, thickness);

        // Act
        var isValid = PSIMaterial.TryParse(materialName, out PSIMaterial psiMat);

        // Assert
        isValid.Should().BeTrue();
        psiMat.Side1Color.Should().Be(side1Color);
        psiMat.Side1FinishType.Should().Be(side1Finish);
        psiMat.Side2Color.Should().Be(side2Color);
        psiMat.Side2FinishType.Should().Be(side2Finish);
        psiMat.CoreType.Should().Be(coreType);
        psiMat.Thickness.Should().Be(thickness);

    }

    [Fact]
    public void Should_ParseValidMultiWordColorMaterial() {

        // Arrange
        string side1Color = "FREE SPIRIT - L580 K";
        string side1Finish = "mela";
        string side2Color = "FREE SPIRIT - L580 K";
        string side2Finish = "mela";
        string coreType = "PB";
        double thickness = 19;
        string materialName = CreateMaterialName(side1Color, side1Finish, side2Color, side2Finish, coreType, thickness);

        // Act
        var isValid = PSIMaterial.TryParse(materialName, out PSIMaterial psiMat);

        // Assert
        isValid.Should().BeTrue();
        psiMat.Side1Color.Should().Be(side1Color);
        psiMat.Side1FinishType.Should().Be(side1Finish);
        psiMat.Side2Color.Should().Be(side2Color);
        psiMat.Side2FinishType.Should().Be(side2Finish);
        psiMat.CoreType.Should().Be(coreType);
        psiMat.Thickness.Should().Be(thickness);

    }

    [Fact]
    public void Should_Parse2DifferentSidedMaterial() {

        // Arrange
        string side1Color = "FREE SPIRIT - L580 K";
        string side1Finish = "mela";
        string side2Color = "Pre-Finished Birch";
        string side2Finish = "Veneer";
        string coreType = "Ply";
        double thickness = 19;
        string materialName = CreateMaterialName(side1Color, side1Finish, side2Color, side2Finish, coreType, thickness);

        // Act
        var isValid = PSIMaterial.TryParse(materialName, out PSIMaterial psiMat);

        // Assert
        isValid.Should().BeTrue();
        psiMat.Side1Color.Should().Be(side1Color);
        psiMat.Side1FinishType.Should().Be(side1Finish);
        psiMat.Side2Color.Should().Be(side2Color);
        psiMat.Side2FinishType.Should().Be(side2Finish);
        psiMat.CoreType.Should().Be(coreType);
        psiMat.Thickness.Should().Be(thickness);

    }

    [Fact]
    public void ShouldNot_ParseInValid2WordCoreMaterial() {

        // Arrange
        string side1Color = "White";
        string side1Finish = "mela";
        string side2Color = "White";
        string side2Finish = "mela";
        string coreType = "Particle Board";
        double thickness = 19;
        string materialName = CreateMaterialName(side1Color, side1Finish, side2Color, side2Finish, coreType, thickness);

        // Act
        var isValid = PSIMaterial.TryParse(materialName, out PSIMaterial psiMat);

        // Assert
        isValid.Should().BeFalse();

    }

    [Theory]
    [InlineData("tops")]
    [InlineData("tops1")]
    public void CleanMaterialName_TopsMaterialNameSuffix_RemoveSuffixFromMaterialName(string keyWord) {

        // Arrange
        string materialNameBase = "Other Material Information";
        string materialName = $"{materialNameBase} ({keyWord})";

        // Act
        var clean = PSIMaterial.CleanMaterialName(materialName);

        // Assert
        clean.Should().Be(materialNameBase);

    }

    [Theory]
    [InlineData("tops")]
    [InlineData("tops1")]
    public void CleanMaterialName_TopsMaterialNamePrefix_RemovePrefixFromMaterialName(string keyWord) {

        // Arrange
        string materialNameBase = "Other Material Information";
        string materialName = $"({keyWord}) {materialNameBase}";

        // Act
        var clean = PSIMaterial.CleanMaterialName(materialName);

        // Assert
        clean.Should().Be(materialNameBase);

    }

    [Theory]
    [InlineData("tops")]
    [InlineData("tops1")]
    public void CleanMaterialName_TopsInMiddleOfMaterialName_RemoveTopsFromMaterialName(string keyWord) {

        // Arrange
        string materialNameBase = "Other Material Information";
        string materialName = materialNameBase.Replace(" ", $" ({keyWord}) ");

        // Act
        var clean = PSIMaterial.CleanMaterialName(materialName);

        // Assert
        clean.Should().Be(materialNameBase);

    }

    [Theory]
    [InlineData("tops")]
    [InlineData("tops1")]
    public void CleanMaterialName_TopsInMiddleOfMaterialName2_RemoveTopsFromMaterialName(string keyWord) {

        // Arrange
        string materialNameBase = "Other Material Information";
        string materialName = $"Other Material({keyWord}) Information";

        // Act
        var clean = PSIMaterial.CleanMaterialName(materialName);

        // Assert
        clean.Should().Be(materialNameBase);

    }

    [Theory]
    [InlineData("tops")]
    [InlineData("tops1")]
    public void CleanMaterialName_TopsInMiddleOfMaterialName3_RemoveTopsFromMaterialName(string keyWord) {

        // Arrange
        string materialNameBase = "Other Material Information";
        string materialName = $"Other Material ({keyWord})Information";

        // Act
        var clean = PSIMaterial.CleanMaterialName(materialName);

        // Assert
        clean.Should().Be(materialNameBase);

    }

    [Fact]
    public void ToLongName_ShouldReturnOriginalName() {

        // Arrange
        string materialName = $"Grained  PRE-FINISHED BIRCH PLY     19.00 PB  WHITE MELA";

        // Act
        var isValid = PSIMaterial.TryParse(materialName, out var psiMaterial);

        // Assert
        isValid.Should().BeTrue();
        psiMaterial.GetLongName().Should().Be(materialName);

    }

    private string CreateMaterialName(string side1Color, string side1Finish, string side2Color, string side2Finish, string coreType, double thickness) {

        return $"Grained  {side1Color} {side1Finish}     {thickness:0.00} {coreType}  {side2Color} {side2Finish}";

    }

}
