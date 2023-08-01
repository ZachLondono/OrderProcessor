using ApplicationCore.Features.Orders.OrderLoading.LoadDoorSpreadsheetOrderData;
using ApplicationCore.Features.Orders.OrderLoading.LoadDoorSpreadsheetOrderData.DoorOrderModels;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Products.Doors;
using FluentAssertions;

namespace ApplicationCore.Tests.Unit.Orders.DoorSpreadsheetOrderLoading;

public class DoorSpreadsheetOrderProviderTests {

    [Theory]
    [InlineData("Door", DoorType.Door)]
    [InlineData("Double Door", DoorType.Door)]
    [InlineData("Base Door LH", DoorType.Door)]
    [InlineData("Drawer Front", DoorType.DrawerFront)]
    [InlineData("Dwr. Front - A", DoorType.DrawerFront)]
    [InlineData("Dwr. Front - B", DoorType.DrawerFront)]
    public void MapLineItem_Should_Create_Product_With_Correct_DoorType(string description, DoorType expected) {

        // Arrange
        var header = CreateOrderHeader();
        var item = CreateLineItem();
        item.Description = description;

        // Act
        var product = DoorSpreadsheetOrderProvider.MapLineItem(item, header, OrderHeader.UnitType.Inches);

        // Assert
        product.Should().BeOfType<MDFDoorProduct>();
        (product as MDFDoorProduct)!.Type.Should().Be(expected);

    }

    [Theory]
    [InlineData("English (dec)", OrderHeader.UnitType.Inches)]
    [InlineData("English (frac)", OrderHeader.UnitType.Inches)]
    [InlineData("Metric (mm)", OrderHeader.UnitType.Millimeters)]
    public void GetUnitType_Should_Return_Correct_Type(string input, OrderHeader.UnitType expected) {

        // Arrange
        var header = CreateOrderHeader();
        header.Units = input;

        // Act
        var type = header.GetUnitType();

        // Assert
        type.Should().Be(expected);

    }

    [Fact]
    public void MapLineItem_Should_Create_Product_With_Correct_Dimensions_When_Units_Are_Inches() {

        // Arrange
        var header = CreateOrderHeader();
        var item = CreateLineItem();
        item.Width = 10;
        item.Height = 10;
        item.TopRail = 10;
        item.BottomRail = 10;
        item.LeftStile = 10;
        item.RightStile = 10;
        item.Thickness = 10;
        header.PanelDrop = 10;

        // Act
        var product = DoorSpreadsheetOrderProvider.MapLineItem(item, header, OrderHeader.UnitType.Inches);

        // Assert
        product.Should().BeOfType<MDFDoorProduct>();
        var door = product as MDFDoorProduct;
        door!.Height.AsInches().Should().Be(item.Height);
        door.Width.AsInches().Should().Be(item.Width);
        door.FrameSize.TopRail.AsInches().Should().Be(item.TopRail);
        door.FrameSize.BottomRail.AsInches().Should().Be(item.BottomRail);
        door.FrameSize.LeftStile.AsInches().Should().Be(item.LeftStile);
        door.FrameSize.RightStile.AsInches().Should().Be(item.RightStile);
        door.Thickness.AsInches().Should().Be(item.Thickness);
        door.PanelDrop.AsInches().Should().Be(header.PanelDrop);

    }

    [Fact]
    public void MapLineItem_Should_Create_Product_With_Correct_Dimensions_When_Units_Are_Millimeters() {

        // Arrange
        var header = CreateOrderHeader();
        var item = CreateLineItem();
        item.Width = 10;
        item.Height = 10;
        item.TopRail = 10;
        item.BottomRail = 10;
        item.LeftStile = 10;
        item.RightStile = 10;
        item.Thickness = 10;
        header.PanelDrop = 10;

        // Act
        var product = DoorSpreadsheetOrderProvider.MapLineItem(item, header, OrderHeader.UnitType.Millimeters);

        // Assert
        product.Should().BeOfType<MDFDoorProduct>();
        var door = product as MDFDoorProduct;
        door!.Height.AsMillimeters().Should().Be(item.Height);
        door.Width.AsMillimeters().Should().Be(item.Width);
        door.FrameSize.TopRail.AsMillimeters().Should().Be(item.TopRail);
        door.FrameSize.BottomRail.AsMillimeters().Should().Be(item.BottomRail);
        door.FrameSize.LeftStile.AsMillimeters().Should().Be(item.LeftStile);
        door.FrameSize.RightStile.AsMillimeters().Should().Be(item.RightStile);
        door.Thickness.AsMillimeters().Should().Be(item.Thickness);
        door.PanelDrop.AsMillimeters().Should().Be(header.PanelDrop);

    }

    [Fact]
    public void MapWorkbookData_Should_Create_Valid_Address() {

        // Arrange
        string line1 = "address line 1";
        string city = "city name";
        string state = "NJ";
        string zip = "08876";

        var header = CreateOrderHeader();
        header.Address1 = line1;
        header.Address2 = $"{city}, {state} {zip}";
        var items = new List<LineItem>();

        // Act
        var order = DoorSpreadsheetOrderProvider.MapWorkbookData(header, "C:\\", items, Guid.Empty, Guid.Empty);

        // Assert
        order.Shipping.Address.Should().BeEquivalentTo(order.Billing.Address);
        var address = order.Shipping.Address;
        address.Line1.Should().Be(line1);
        address.City.Should().Be(city);
        address.State.Should().Be(state);
        address.Zip.Should().Be(zip);

    }

    private static LineItem CreateLineItem() => new() {
        PartNumber = 0,
        Description = "",
        Line = 0,
        Qty = 0,
        Width = 0,
        Height = 0,
        Note = "",
        UnitPrice = 0m,
        LeftStile = 0,
        RightStile = 0,
        TopRail = 0,
        BottomRail = 0,
        Material = "",
        Thickness = 0,
        Opening1 = 0,
        Opening2 = 0,
        Rail3 = 0,
        Rail4 = 0,
        Orientation = "Horizontal"
    };

    private static OrderHeader CreateOrderHeader() => new() {
        VendorName = "",
        CompanyName = "",
        Phone = "",
        InvoiceFirstName = "",
        InvoiceEmail = "",
        ConfirmationFirstName = "",
        ConfirmationEmail = "",
        Address1 = "",
        Address2 = "",
        Units = "English (frac)",
        TrackingNumber = "",
        JobName = "",
        OrderDate = DateTime.Now,
        Freight = 0,
        PanelDrop = 0,
        Finish = "",
        Color = "",
        Style = "",
        EdgeProfile = "",
        PanelDetail = "",
    };

}
