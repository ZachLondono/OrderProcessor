using ApplicationCore.Features.Orders.OrderLoading.LoadClosetProOrderData.Models;
using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.Products.Closets;
using ApplicationCore.Shared.Domain;
using FluentAssertions;

namespace ApplicationCore.Tests.Unit.Orders.ClosetProOrderLoading;

public class ProductMappingTests {

    private readonly ClosetProPartMapper _sut;
    
    public ProductMappingTests() {
        _sut = new(new ComponentBuilderFactory());
    }

    [Fact]
    public void FloorMountedDrillThroughPart_ShouldCreateCorrectProduct() {

        // Arrange
        string expectedSKU = "PC";
        Dimension panelHeight = Dimension.FromInches(89);
        Dimension panelDepth = Dimension.FromInches(12);
        Dimension leftDrilling = Dimension.FromInches(12);
        Dimension rightDrilling = Dimension.FromInches(12);

        var part = new Part() {
            Depth = panelDepth.AsInches(),
            Height = panelHeight.AsInches(),
            Color = "White",
            PartCost = "123.45",
            Quantity = 123,
            VertDrillL = leftDrilling.AsInches(),
            VertDrillR = rightDrilling.AsInches(),
            ExportName = "CPS FM Vert",
            VertHand = "T",
            InfoRecords = new() {
                new() {
                    PartName = "Edge Banding",
                    Color = "RED"
                }
            }
        };
        var helper = new PartHelper() {
            Part = part
        };

        // Act
        var product = _sut.CreateVerticalPanelFromPart(part);

        // Assert
        var closetPart = helper.CompareToProduct(product);
        closetPart.SKU.Should().Be(expectedSKU);
        closetPart.Width.Should().Be(panelDepth);
        closetPart.Length.Should().Be(panelHeight);
        closetPart.EdgeBandingColor.Should().Be("RED");
        closetPart.Parameters.Should().Contain(new KeyValuePair<string, string>("FINLEFT", "0"));
        closetPart.Parameters.Should().Contain(new KeyValuePair<string, string>("FINRIGHT", "0"));
        closetPart.Parameters.Should().Contain(new KeyValuePair<string, string>("WallMount", "0"));

    }

    [Fact]
    public void FloorMountedFinRightPart_ShouldCreateCorrectProduct() {

        // Arrange
        string expectedSKU = "PE";
        Dimension panelHeight = Dimension.FromInches(89);
        Dimension panelDepth = Dimension.FromInches(12);
        Dimension leftDrilling = Dimension.FromInches(12);
        Dimension rightDrilling = Dimension.Zero;

        var part = new Part() {
            Depth = panelDepth.AsInches(),
            Height = panelHeight.AsInches(),
            Color = "White",
            PartCost = "123.45",
            Quantity = 123,
            VertDrillL = leftDrilling.AsInches(),
            VertDrillR = rightDrilling.AsInches(),
            ExportName = "CPS FM Vert",
            VertHand = "R",
            InfoRecords = new() {
                new() {
                    PartName = "Edge Banding",
                    Color = "RED"
                }
            }
        };
        var helper = new PartHelper() {
            Part = part
        };

        // Act
        var product = _sut.CreateVerticalPanelFromPart(part);

        // Assert
        var closetPart = helper.CompareToProduct(product);
        closetPart.SKU.Should().Be(expectedSKU);
        closetPart.Width.Should().Be(panelDepth);
        closetPart.Length.Should().Be(panelHeight);
        closetPart.EdgeBandingColor.Should().Be("RED");
        closetPart.Parameters.Should().Contain(new KeyValuePair<string, string>("FINLEFT", "0"));
        closetPart.Parameters.Should().Contain(new KeyValuePair<string, string>("FINRIGHT", "1"));
        closetPart.Parameters.Should().Contain(new KeyValuePair<string, string>("WallMount", "0"));

    }

    [Fact]
    public void FloorMountedFinLeftPart_ShouldCreateCorrectProduct() {

        // Arrange
        string expectedSKU = "PE";
        Dimension panelHeight = Dimension.FromInches(89);
        Dimension panelDepth = Dimension.FromInches(12);
        Dimension rightDrilling = Dimension.FromInches(12);
        Dimension leftDrilling = Dimension.Zero;

        var part = new Part() {
            Depth = panelDepth.AsInches(),
            Height = panelHeight.AsInches(),
            Color = "White",
            PartCost = "123.45",
            Quantity = 123,
            VertDrillL = leftDrilling.AsInches(),
            VertDrillR = rightDrilling.AsInches(),
            ExportName = "CPS FM Vert",
            VertHand = "L",
            InfoRecords = new() {
                new() {
                    PartName = "Edge Banding",
                    Color = "RED"
                }
            }
        };
        var helper = new PartHelper() {
            Part = part
        };

        // Act
        var product = _sut.CreateVerticalPanelFromPart(part);

        // Assert
        var closetPart = helper.CompareToProduct(product);
        closetPart.SKU.Should().Be(expectedSKU);
        closetPart.Width.Should().Be(panelDepth);
        closetPart.Length.Should().Be(panelHeight);
        closetPart.EdgeBandingColor.Should().Be("RED");
        closetPart.Parameters.Should().Contain(new KeyValuePair<string, string>("FINLEFT", "1"));
        closetPart.Parameters.Should().Contain(new KeyValuePair<string, string>("FINRIGHT", "0"));
        closetPart.Parameters.Should().Contain(new KeyValuePair<string, string>("WallMount", "0"));

    }

    [Fact]
    public void WallMountedPart_ShouldCreateCorrectProduct() {

        // Arrange
        string expectedSKU = "PE";
        Dimension panelHeight = Dimension.FromInches(89);
        Dimension panelDepth = Dimension.FromInches(12);
        Dimension rightDrilling = Dimension.FromInches(12);
        Dimension leftDrilling = Dimension.Zero;

        var part = new Part() {
            Depth = panelDepth.AsInches(),
            Height = panelHeight.AsInches(),
            Color = "White",
            PartCost = "123.45",
            Quantity = 123,
            VertDrillL = leftDrilling.AsInches(),
            VertDrillR = rightDrilling.AsInches(),
            ExportName = "CPS WM Vert",
            VertHand = "L",
            InfoRecords = new() {
                new() {
                    PartName = "Edge Banding",
                    Color = "RED"
                }
            }
        };
        var helper = new PartHelper() {
            Part = part
        };

        // Act
        var product = _sut.CreateVerticalPanelFromPart(part);

        // Assert
        var closetPart = helper.CompareToProduct(product);
        closetPart.SKU.Should().Be(expectedSKU);
        closetPart.Width.Should().Be(panelDepth);
        closetPart.Length.Should().Be(panelHeight);
        closetPart.EdgeBandingColor.Should().Be("RED");
        closetPart.Parameters.Should().Contain(new KeyValuePair<string, string>("FINLEFT", "1"));
        closetPart.Parameters.Should().Contain(new KeyValuePair<string, string>("FINRIGHT", "0"));
        closetPart.Parameters.Should().Contain(new KeyValuePair<string, string>("WallMount", "1"));

    }

    public class PartHelper {

        public Part Part { get; set; } = new();

        public ClosetPart CompareToProduct(IProduct product) {

            product.Should().BeOfType<ClosetPart>();

            var closetPart = product as ClosetPart;
            closetPart.Should().NotBeNull();
            closetPart!.Material.Core.Should().Be(ClosetMaterialCore.ParticleBoard);
            closetPart.Material.Finish.Should().Be(Part.Color);
            closetPart.Qty.Should().Be(Part.Quantity);
            closetPart.ProductNumber.Should().Be(Part.PartNum);
            closetPart.Room.Should().Be(ClosetProPartMapper.GetRoomName(Part));
            if (ClosetProPartMapper.TryParseMoneyString(Part.PartCost, out decimal price)) {
                closetPart.UnitPrice.Should().Be(price);
            }

            return closetPart;

        }


    }

}
