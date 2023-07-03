using ApplicationCore.Features.Orders.OrderLoading.LoadClosetProOrderData.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ApplicationCore.Features.Orders.OrderLoading.LoadClosetProOrderData.LoadClosetProWebOrderData;

internal class ClosetProClientFactory {

    private readonly ClosetProSoftwareCredentials _credentials;
    private readonly ILoggerFactory _loggerFactory;

    public ClosetProClientFactory(IOptions<ClosetProSoftwareCredentials> credentials, ILoggerFactory loggerFactory) {
        _credentials = credentials.Value;
        _loggerFactory = loggerFactory;
    }

    public ClosetProClient CreateClient() {
        var logger = _loggerFactory.CreateLogger<ClosetProClient>();
        return new ClosetProClient(_credentials.Username, _credentials.Password, _credentials.Instance, logger);
    }

}
