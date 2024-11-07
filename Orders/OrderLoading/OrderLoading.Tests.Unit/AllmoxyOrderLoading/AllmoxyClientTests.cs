using OrderLoading.LoadAllmoxyOrderData.LoadAllmoxyWebOrderData;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using RestSharp;
using IRestClient = OrderLoading.LoadAllmoxyOrderData.LoadAllmoxyWebOrderData.Rest.IRestClient;

namespace OrderLoading.Tests.Unit.AllmoxyOrderLoading;

public class AllmoxyClientTests {

    private readonly AllmoxyClient _sut;
    private readonly IRestClient _client = Substitute.For<IRestClient>();
    private const string USERNAME = "username";
    private const string PASSWORD = "password";

    public AllmoxyClientTests() {
        _sut = new(USERNAME, PASSWORD, _client);
    }

    [Fact]
    public async Task ShouldReturnExportData_WhenOrderAndIndexAreValid() {

        string orderNumber = "";
        int index = 0;

        string expectedContent = "EXPECTED CONTENT";

        var response = new RestResponse() {
            ContentType = AllmoxyClient.EXPORT_CONTENT_TYPE,
            Content = expectedContent
        };

        _client.ExecuteAsync(Arg.Any<RestRequest>()).ReturnsForAnyArgs(Task.FromResult(response));

        var export = await _sut.GetExportAsync(orderNumber, index);

        export.Should().BeEquivalentTo(expectedContent);

    }

    [Fact]
    public async Task ShouldThrowException_WhenOrderIsNotValid() {

        string orderNumber = "";
        int index = 0;

        string expectedContent = AllmoxyClient.ORDER_NOT_FOUND_CONTENT;

        var response = new RestResponse() {
            ContentType = AllmoxyClient.NOT_LOGGED_IN_CONTENT_TYPE,
            Content = expectedContent
        };

        _client.ExecuteAsync(Arg.Any<RestRequest>()).ReturnsForAnyArgs(Task.FromResult(response));

        await _sut.Awaiting(s => s.GetExportAsync(orderNumber, index))
                    .Should()
                    .ThrowAsync<InvalidOperationException>()
                    .WithMessage("Order could not be found");

    }

    [Fact]
    public async Task ShouldThrowException_WhenIndexIsNotValid() {

        string orderNumber = "";
        int index = 0;

        string expectedContent = AllmoxyClient.EXPORT_NOT_FOUND_CONTENT;

        var response = new RestResponse() {
            ContentType = AllmoxyClient.NOT_LOGGED_IN_CONTENT_TYPE,
            Content = expectedContent
        };

        _client.ExecuteAsync(Arg.Any<RestRequest>()).ReturnsForAnyArgs(Task.FromResult(response));

        await _sut.Awaiting(s => s.GetExportAsync(orderNumber, index))
                    .Should()
                    .ThrowAsync<InvalidOperationException>()
                    .WithMessage("Export could not be found");

    }

    [Fact]
    public async Task ShouldThrowException_WhenCredentialsAreInvalid() {

        string orderNumber = "";
        int index = 0;

        var response = new RestResponse() {
            ContentType = AllmoxyClient.NOT_LOGGED_IN_CONTENT_TYPE
        };

        _client.ExecuteAsync(Arg.Any<RestRequest>()).ReturnsForAnyArgs(Task.FromResult(response));

        await _sut.Awaiting(s => s.GetExportAsync(orderNumber, index))
                    .Should()
                    .ThrowAsync<InvalidOperationException>()
                    .WithMessage("Could not log in to Allmoxy");

    }

    [Fact]
    public async Task ShouldThrowException_WhenUnexpectedResponseReturned() {

        string orderNumber = "";
        int index = 0;

        var response = new RestResponse() {
            ContentType = "UNEXPECTED/TYPE",
            StatusCode = System.Net.HttpStatusCode.OK
        };

        _client.ExecuteAsync(Arg.Any<RestRequest>()).ReturnsForAnyArgs(Task.FromResult(response));

        await _sut.Awaiting(s => s.GetExportAsync(orderNumber, index))
                    .Should()
                    .ThrowAsync<InvalidOperationException>()
                    .WithMessage($"Unexpected response from server {System.Net.HttpStatusCode.OK}");

    }

    [Fact]
    public void ShouldThrowException_WhenContentIsNull() {

        string orderNumber = "";
        int index = 0;

        var response = new RestResponse() {
            ContentType = AllmoxyClient.EXPORT_CONTENT_TYPE,
            Content = null
        };

        _client.ExecuteAsync(Arg.Any<RestRequest>()).ReturnsForAnyArgs(Task.FromResult(response));

        _sut.Awaiting(s => s.GetExportAsync(orderNumber, index))
                    .Should()
                    .ThrowAsync<InvalidOperationException>()
                    .WithMessage("No data returned");

    }

    [Fact]
    public void ShouldCallClientDispose_WhenDisposeCalled() {

        _sut.Dispose();

        _client.Received().Dispose();

    }

}
