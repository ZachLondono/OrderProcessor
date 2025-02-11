using ApplicationCore.Features.MDFDoorOrders.HafeleMDFDoorOrders.ReadOrderFile;
using ClosedXML.Excel;
using FluentAssertions;
using System.Reflection;

namespace ApplicationCore.Tests.Unit.HafeleMDFDoorOrderLoading;

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

        options.Should().NotBeNull();

        options.Date.Should().Be(new DateTime(2025, 1, 17));
        options.ProductionTime.Should().Be("Standard 10 day");
        options.PurchaseOrder.Should().Be("PO123");
        options.JobName.Should().Be("JOBNAME");

        options.HafelePO.Should().Be("HafelePO");
        options.HafeleOrderNumber.Should().Be("HafeleOrderNum");

        options.Material.Should().Be("MDF-3/4\"");
        options.DoorStyle.Should().Be("Shaker");
        options.Stiles.Should().Be("2.75");
        options.Rails.Should().Be("2.75");
        options.AStyleDrawerFrontRails.Should().Be("1.5");
        options.EdgeProfile.Should().Be("Square");
        options.PanelDetail.Should().Be("Flat");
        options.PanelDrop.Should().Be("0.3125");
        options.Finish.Should().Be("None");
        options.HingeDrilling.Should().Be("None");
        options.HingeTab.Should().Be("5");

        options.Contact.Should().Be("Contact");
        options.Company.Should().Be("Company");
        options.AccountNumber.Should().Be("Account #");
        options.AddressLine1.Should().Be("Address 1");
        options.AddressLine2.Should().Be("Address 2");
        options.City.Should().Be("City");
        options.State.Should().Be("State");
        options.Zip.Should().Be("Zip");
        options.Phone.Should().Be("Phone");
        options.Email.Should().Be("Email");
        options.Delivery.Should().Be("Standard Pallet");
        options.TrackingNumber.Should().Be("Tracking Number");

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
