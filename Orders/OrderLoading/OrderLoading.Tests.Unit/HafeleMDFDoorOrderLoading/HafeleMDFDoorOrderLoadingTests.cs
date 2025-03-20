using ClosedXML.Excel;
using FluentAssertions;
using OrderLoading.LoadHafeleMDFDoorSpreadsheetOrderData.ReadOrderFile;
using System.Reflection;

namespace OrderLoading.Tests.Unit.HafeleMDFDoorOrderLoading;

public class HafeleMDFDoorOrderLoadingTests {

    [Fact]
    public void LoadFile() {

        var path = GetFilePath("Test1.3.xlsx");

        var order = HafeleMDFDoorOrder.Load(path);

        order.Should().NotBeNull();

    }

    [Fact]
    public void LoadOptionsOnly() {

        var path = GetFilePath("Test1.3.xlsx");

        var wb = new XLWorkbook(path);

        var options = Options.LoadFromWorkbook(wb);

        options.Data.Should().NotBeNull();

        options.Data.Date.Should().Be(new DateTime(2025, 1, 17));
        options.Data.ProductionTime.Should().Be("Standard 10 day");
        options.Data.PurchaseOrder.Should().Be("PO123");
        options.Data.JobName.Should().Be("JOBNAME");
        options.Data.OrderComments.Should().Be("This is a comment");

        options.Data.HafelePO.Should().Be("HafelePO");
        options.Data.HafeleOrderNumber.Should().Be("HafeleOrderNum");

        options.Data.Material.Should().Be("MDF-3/4\"");
        options.Data.DoorStyle.Should().Be("Shaker");
        options.Data.Stiles.Should().Be(2.75);
        options.Data.Rails.Should().Be(2.75);
        options.Data.AStyleDrawerFrontRails.Should().Be(1.5);
        options.Data.EdgeProfile.Should().Be("Square");
        options.Data.PanelDetail.Should().Be("Flat");
        options.Data.PanelDrop.Should().Be(0.3125);
        options.Data.Finish.Should().Be("None");
        options.Data.HingeDrilling.Should().Be("None");
        options.Data.HingeTab.Should().Be(5);

        options.Data.Contact.Should().Be("Contact");
        options.Data.Company.Should().Be("Company");
        options.Data.AccountNumber.Should().Be("Account #");
        options.Data.AddressLine1.Should().Be("Address 1");
        options.Data.AddressLine2.Should().Be("Address 2");
        options.Data.City.Should().Be("City");
        options.Data.State.Should().Be("State");
        options.Data.Zip.Should().Be("Zip");
        options.Data.Phone.Should().Be("Phone");
        options.Data.Email.Should().Be("Email");
        options.Data.Delivery.Should().Be("Standard Pallet");
        options.Data.TrackingNumber.Should().Be("Tracking Number");

    }

    [Fact]
    public void LoadSizesOnly() {

        var path = GetFilePath("Test1.3.xlsx");
        var wb = new XLWorkbook(path);

        var sizes = Size.LoadFromWorkbook(wb);

        sizes.Should().NotBeNull();
        sizes.Should().HaveCount(6);
        sizes[0].Rail3.Should().Be(0);
        sizes[4].Rail3.Should().Be(2.75);

    }

    [Fact]
    public void LoadDataOnly() {

        var path = GetFilePath("Test1.3.xlsx");
        var wb = new XLWorkbook(path);

        var data = Data.LoadFromWorkbook(wb);

        data.Should().NotBeNull();

        data.HafeleMarkUpToCustomers.Should().Be(0.3);
        data.DiscountToHafele.Should().Be(0.2);

        data.MaterialThicknessesByName.Should().Contain(new KeyValuePair<string, double>("MDF-3/4\"", 0.75));
        data.MaterialThicknessesByName.Should().Contain(new KeyValuePair<string, double>("MDF-1\"", 1));
        data.MaterialThicknessesByName.Should().Contain(new KeyValuePair<string, double>("White Mela MDF-3/4\"", 0.75));
        data.MaterialThicknessesByName.Should().Contain(new KeyValuePair<string, double>("White Mela MDF-1\"", 1));

        data.PanelCountByDoorType.Should().Contain(new KeyValuePair<string, int>("Single Panel", 1));
        data.PanelCountByDoorType.Should().Contain(new KeyValuePair<string, int>("Single Open Panel", 1));
        data.PanelCountByDoorType.Should().Contain(new KeyValuePair<string, int>("Single Open Panel, w/ Gasket Route", 1));
        data.PanelCountByDoorType.Should().Contain(new KeyValuePair<string, int>("Double Panel, SS", 2));
        data.PanelCountByDoorType.Should().Contain(new KeyValuePair<string, int>("Double Panel, OS", 2));
        data.PanelCountByDoorType.Should().Contain(new KeyValuePair<string, int>("Double Panel, SO", 2));
        data.PanelCountByDoorType.Should().Contain(new KeyValuePair<string, int>("Double Panel, OO", 2));
        data.PanelCountByDoorType.Should().Contain(new KeyValuePair<string, int>("Double Panel, OS, w/ Gasket Route", 2));
        data.PanelCountByDoorType.Should().Contain(new KeyValuePair<string, int>("Double Panel, OO, w/ Gasket Route", 2));
        data.PanelCountByDoorType.Should().Contain(new KeyValuePair<string, int>("Triple Panel", 3));
        data.PanelCountByDoorType.Should().Contain(new KeyValuePair<string, int>("Quadruple Panel", 4));
        data.PanelCountByDoorType.Should().Contain(new KeyValuePair<string, int>("Slab", 0));
        data.PanelCountByDoorType.Should().Contain(new KeyValuePair<string, int>("Drawer Front - B", 1));
        data.PanelCountByDoorType.Should().Contain(new KeyValuePair<string, int>("Drawer Front - A", 1));

    }

    private string GetFilePath(string fileName)
        => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "HafeleMDFDoorOrderLoading", "TestData", fileName);

}
