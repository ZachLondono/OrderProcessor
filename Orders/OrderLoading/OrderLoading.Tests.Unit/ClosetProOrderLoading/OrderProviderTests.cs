using FluentAssertions;
using OrderLoading.ClosetProCSVCutList.Header;

namespace OrderLoading.Tests.Unit.ClosetProOrderLoading;

public class OrderProviderTests {

    [Fact]
    public void GetOrderName_Should_RemoveNumberPrefix() {

        string input = "1-Order Name";
        string expectedOutput = "Order Name";

        string output = OrderHeaderProcessor.GetOrderName(input);

        output.Should().Be(expectedOutput);

    }

}
