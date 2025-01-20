using ApplicationCore.Features.HafeleMDFDoorOrders.ReadOrderFile;
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
