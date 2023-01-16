using ApplicationCore.Features.Orders.Loader;
using ApplicationCore.Features.Orders.Loader.Rest;
using FluentAssertions;
using NSubstitute;
using RestSharp;

namespace ApplicationCore.Tests.Unit.Orders;

public class AllmoxyClientTests {

    private readonly AllmoxyClient _sut;
    private readonly IRestClient _client = Substitute.For<IRestClient>();
    private const string USERNAME = "username";
    private const string PASSWORD = "password";

    public AllmoxyClientTests() {
        _sut = new(USERNAME, PASSWORD, _client);
    }

    [Fact]
    public void ShouldReturnExportData_WhenOrderAndIndexAreValid() {
        
        string orderNumber = "";
        int index = 0;

        string expectedContent = "EXPECTED CONTENT";

        var response = new RestResponse() {
            ContentType = AllmoxyClient.EXPORT_CONTENT_TYPE,
            Content = expectedContent
        };

        _client.Execute(Arg.Any<RestRequest>()).ReturnsForAnyArgs(response);

        var export = _sut.GetExport(orderNumber, index);

        export.Should().BeEquivalentTo(expectedContent);

    }

    [Fact]
    public void ShouldThrowException_WhenOrderIsNotValid() {
        
        string orderNumber = "";
        int index = 0;

        string expectedContent = AllmoxyClient.ORDER_NOT_FOUND_CONTENT;

        var response = new RestResponse() {
            ContentType = AllmoxyClient.NOT_LOGGED_IN_CONTENT_TYPE,
            Content = expectedContent
        };

        _client.Execute(Arg.Any<RestRequest>()).ReturnsForAnyArgs(response);

        var action = () => _sut.GetExport(orderNumber, index);

        action.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Order could not be found");

    }
    
    [Fact]
    public void ShouldThrowException_WhenIndexIsNotValid() {
    
        string orderNumber = "";
        int index = 0;

        string expectedContent = AllmoxyClient.EXPORT_NOT_FOUND_CONTENT;

        var response = new RestResponse() {
            ContentType = AllmoxyClient.NOT_LOGGED_IN_CONTENT_TYPE,
            Content = expectedContent
        };

        _client.Execute(Arg.Any<RestRequest>()).ReturnsForAnyArgs(response);

        var action = () => _sut.GetExport(orderNumber, index);

        action.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Export could not be found");

    }

    [Fact]
    public void ShouldThrowException_WhenCredentialsAreInvalid() {

        string orderNumber = "";
        int index = 0;

        var response = new RestResponse() {
            ContentType = AllmoxyClient.NOT_LOGGED_IN_CONTENT_TYPE
        };

        _client.Execute(Arg.Any<RestRequest>()).ReturnsForAnyArgs(response);

        var action = () => _sut.GetExport(orderNumber, index);

        action.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Could not log in to Allmoxy");
        
    }

}
