using ApplicationCore.Features.ClosetProCSVCutList;
using ApplicationCore.Features.ClosetProCSVCutList.CSVModels;
using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.Components;
using ApplicationCore.Shared.Domain;
using FluentAssertions;

namespace ApplicationCore.Tests.Unit.Orders.ClosetProOrderLoading;

public class ProductMappingTests {

    [Fact]
    public void FloorMountedDrillThroughPart() {

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
            InfoRecords = [
                new() {
                    PartName = "Edge Banding",
                    Color = "RED"
                }
            ]
        };
        var helper = new PartHelper() {
            Part = part
        };

        // Act
        var vp = ClosetProPartMapper.CreateVerticalPanel(part, false, RoomNamingStrategy.ByWallAndSection);
        var product = vp.ToProduct(Dimension.Zero);

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
    public void FloorMountedFinRightPart() {

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
        var vp = ClosetProPartMapper.CreateVerticalPanel(part, false, RoomNamingStrategy.ByWallAndSection);
        var product = vp.ToProduct(Dimension.Zero);

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
    public void FloorMountedFinLeftPart() {

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
        var vp = ClosetProPartMapper.CreateVerticalPanel(part, false, RoomNamingStrategy.ByWallAndSection);
        var product = vp.ToProduct(Dimension.Zero);

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
    public void FloorMountedBaseNotchPart() {

        // Arrange
        string expectedSKU = "PC";
        Dimension panelHeight = Dimension.FromInches(89);
        Dimension panelDepth = Dimension.FromInches(12);
        Dimension leftDrilling = Dimension.FromInches(12);
        Dimension rightDrilling = Dimension.FromInches(12);
        Dimension expectedBaseNotchDepth = Dimension.FromInches(1.5);
        Dimension expectedBaseNotchHeight = Dimension.FromInches(10);

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
            BBDepth = expectedBaseNotchDepth.AsInches(),
            BBHeight = expectedBaseNotchHeight.AsInches(),
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
        var vp = ClosetProPartMapper.CreateVerticalPanel(part, false, RoomNamingStrategy.ByWallAndSection);
        var product = vp.ToProduct(Dimension.Zero);

        // Assert
        var closetPart = helper.CompareToProduct(product);
        closetPart.SKU.Should().Be(expectedSKU);
        closetPart.Width.Should().Be(panelDepth);
        closetPart.Length.Should().Be(panelHeight);
        closetPart.EdgeBandingColor.Should().Be("RED");
        closetPart.Parameters.Should().Contain(new KeyValuePair<string, string>("FINLEFT", "0"));
        closetPart.Parameters.Should().Contain(new KeyValuePair<string, string>("FINRIGHT", "0"));
        closetPart.Parameters.Should().Contain(new KeyValuePair<string, string>("WallMount", "0"));
        closetPart.Parameters.Should().Contain(new KeyValuePair<string, string>("BottomNotchH", expectedBaseNotchHeight.AsMillimeters().ToString()));
        closetPart.Parameters.Should().Contain(new KeyValuePair<string, string>("BottomNotchD", expectedBaseNotchDepth.AsMillimeters().ToString()));

    }

    [Fact]
    public void FloorMountedTransitionPart() {

        // Arrange
        string expectedSKU = "PCDT";
        Dimension panelHeight = Dimension.FromInches(89);
        Dimension panelDepth = Dimension.FromInches(14);
        Dimension leftDrilling = Dimension.FromInches(14);
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
        var vp = ClosetProPartMapper.CreateTransitionVerticalPanel(part, false, RoomNamingStrategy.ByWallAndSection);
        var product = vp.ToProduct(Dimension.Zero);

        // Assert
        var closetPart = helper.CompareToProduct(product);
        closetPart.SKU.Should().Be(expectedSKU);
        closetPart.Width.Should().Be(panelDepth);
        closetPart.Length.Should().Be(panelHeight);
        closetPart.EdgeBandingColor.Should().Be("RED");
        closetPart.Parameters.Should().Contain(new KeyValuePair<string, string>("FINLEFT", "0"));
        closetPart.Parameters.Should().Contain(new KeyValuePair<string, string>("FINRIGHT", "1"));
        closetPart.Parameters.Should().Contain(new KeyValuePair<string, string>("WallMount", "0"));
        closetPart.Parameters.Should().Contain(new KeyValuePair<string, string>("MiddleHoles", (rightDrilling - Dimension.FromMillimeters(37)).AsMillimeters().ToString()));

    }

    [Fact]
    public void FloorMountedHutchPart() {

        // Arrange
        string expectedSKU = "PEH";
        Dimension panelHeight = Dimension.FromInches(89);
        Dimension panelDepth = Dimension.FromInches(14);
        Dimension leftDrilling = Dimension.FromInches(14);
        Dimension rightDrilling = Dimension.FromInches(0);
        Dimension expectedBaseNotchDepth = Dimension.FromInches(1.5);
        Dimension expectedBaseNotchHeight = Dimension.FromInches(10);
        Dimension expectedHutchTopDepth = Dimension.FromInches(12);
        Dimension expectedHutchDwrPanelHeight = Dimension.FromInches(45);

        var part = new Part() {
            Depth = panelDepth.AsInches(),
            Height = panelHeight.AsInches(),
            Color = "White",
            PartCost = "123.45",
            Quantity = 123,
            VertDrillL = leftDrilling.AsInches(),
            VertDrillR = rightDrilling.AsInches(),
            PartName = "Vertical Panel - Hutch",
            ExportName = "VP-Hutch",
            VertHand = "Right",
            BBDepth = expectedBaseNotchDepth.AsInches(),
            BBHeight = expectedBaseNotchHeight.AsInches(),
            DrillLeft1 = $"0|{expectedHutchTopDepth.AsInches()}",
            DrillLeft2 = $"0|{panelHeight.AsInches()}",
            UnitL = "",
            UnitR = $"{panelDepth.AsInches()}|{expectedHutchDwrPanelHeight.AsInches()}|{expectedHutchTopDepth.AsInches()}|{(panelHeight - expectedHutchDwrPanelHeight).AsInches()}",
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
        var vp = ClosetProPartMapper.CreateHutchVerticalPanel(part, false, RoomNamingStrategy.ByWallAndSection);
        var product = vp.ToProduct(Dimension.Zero);

        // Assert
        var closetPart = helper.CompareToProduct(product);
        closetPart.SKU.Should().Be(expectedSKU);
        closetPart.Width.Should().Be(panelDepth);
        closetPart.Length.Should().Be(panelHeight);
        closetPart.EdgeBandingColor.Should().Be("RED");
        closetPart.Parameters.Should().Contain(new KeyValuePair<string, string>("FINLEFT", "0"));
        closetPart.Parameters.Should().Contain(new KeyValuePair<string, string>("FINRIGHT", "1"));
        closetPart.Parameters.Should().Contain(new KeyValuePair<string, string>("BottomNotchH", expectedBaseNotchHeight.AsMillimeters().ToString()));
        closetPart.Parameters.Should().Contain(new KeyValuePair<string, string>("BottomNotchD", expectedBaseNotchDepth.AsMillimeters().ToString()));
        closetPart.Parameters.Should().Contain(new KeyValuePair<string, string>("TopDepth", expectedHutchTopDepth.AsMillimeters().ToString()));
        closetPart.Parameters.Should().Contain(new KeyValuePair<string, string>("DwrPanelH", expectedHutchDwrPanelHeight.AsMillimeters().ToString()));

    }

    [Fact]
    public void WallMountedPart() {

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
        var vp = ClosetProPartMapper.CreateVerticalPanel(part, false, RoomNamingStrategy.ByWallAndSection);
        var product = vp.ToProduct(Dimension.Zero);

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

    [Fact]
    public void SlabDoorInsert() {

        // Arrange
        var part = new Part() {
            Width = 10,
            Height = 15,
            Color = "White",
            PartCost = "123.45",
            Quantity = 1,
            PartName = "Cab Door Insert",
            ExportName = "Slab",
            InfoRecords = new()
        };
        var helper = new PartHelper() {
            Part = part
        };

        // Act
        var vp = ClosetProPartMapper.CreateSlabFront(part, Dimension.Zero, RoomNamingStrategy.ByWallAndSection);
        var product = vp.ToProduct();

        // Assert
        var slabDoor = helper.CompareToProduct(product);
        slabDoor.SKU.Should().Be("DOOR");
        slabDoor.Length.Should().Be(Dimension.FromInches(part.Height));
        slabDoor.Width.Should().Be(Dimension.FromInches(part.Width));

    }

    [Fact]
    public void SlabDrawerInsert() {

        // Arrange
        var part = new Part() {
            Width = 10,
            Height = 15,
            Color = "White",
            PartCost = "123.45",
            Quantity = 1,
            PartName = "Drawer XX Small Insert",
            ExportName = "Slab",
            InfoRecords = new()
        };
        var helper = new PartHelper() {
            Part = part
        };

        // Act
        var vp = ClosetProPartMapper.CreateSlabFront(part, Dimension.Zero, RoomNamingStrategy.ByWallAndSection);
        var product = vp.ToProduct();

        // Assert
        var slabDoor = helper.CompareToProduct(product);
        slabDoor.SKU.Should().Be("DF-XX");
        slabDoor.Length.Should().Be(Dimension.FromInches(part.Width));
        slabDoor.Width.Should().Be(Dimension.FromInches(part.Height));

    }

    [Fact]
    public void IslandVerticalPanel() {

        // Arrange
        string expectedSKU = "PIE";
        Dimension panelHeight = Dimension.FromInches(24);
        Dimension panelDepth = Dimension.FromInches(34);
        Dimension leftDrilling = Dimension.FromInches(0);
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
            PartName = "Vertical Panel - Island",
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
        var vp = ClosetProPartMapper.CreateIslandVerticalPanel(part, RoomNamingStrategy.ByWallAndSection);
        var product = vp.ToProduct();

        // Assert
        var closetPart = helper.CompareToProduct(product);
        closetPart.SKU.Should().Be(expectedSKU);
        closetPart.Width.Should().Be(panelDepth);
        closetPart.Length.Should().Be(panelHeight);
        closetPart.EdgeBandingColor.Should().Be("RED");
        closetPart.Parameters.Should().Contain(new KeyValuePair<string, string>("FINLEFT", "1"));
        closetPart.Parameters.Should().Contain(new KeyValuePair<string, string>("FINRIGHT", "0"));
        closetPart.Parameters.Should().Contain(new KeyValuePair<string, string>("Row3Holes", "0"));
        double.Parse(closetPart.Parameters["Row1Holes"]).Should().BeApproximately(267.8, 0.15);

    }

}
