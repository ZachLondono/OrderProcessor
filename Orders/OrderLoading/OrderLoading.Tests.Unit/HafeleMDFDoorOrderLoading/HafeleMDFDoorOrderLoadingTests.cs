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

        options.Data.HafelePO.Should().Be("HafelePO");
        options.Data.HafeleOrderNumber.Should().Be("HafeleOrderNum");

        options.Data.Material.Should().Be("MDF-3/4\"");
        options.Data.DoorStyle.Should().Be("Shaker");
        options.Data.Stiles.Should().Be("2.75");
        options.Data.Rails.Should().Be("2.75");
        options.Data.AStyleDrawerFrontRails.Should().Be("1.5");
        options.Data.EdgeProfile.Should().Be("Square");
        options.Data.PanelDetail.Should().Be("Flat");
        options.Data.PanelDrop.Should().Be("0.3125");
        options.Data.Finish.Should().Be("None");
        options.Data.HingeDrilling.Should().Be("None");
        options.Data.HingeTab.Should().Be("5");

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

    private string GetFilePath(string fileName)
        => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "HafeleMDFDoorOrderLoading", "TestData", fileName);

}
