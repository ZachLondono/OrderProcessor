using ApplicationCore.Features.Orders.OrderLoading.LoadClosetProOrderData.Models;
using Microsoft.Extensions.Options;

namespace ApplicationCore.Features.Orders.OrderLoading.LoadClosetProOrderData.LoadClosetProWebOrderData;

internal class ClosetProClientFactory {

    private readonly ClosetProSoftwareCredentials _credentials;

    public ClosetProClientFactory(IOptions<ClosetProSoftwareCredentials> credentials) {
        _credentials = credentials.Value;
    }

    public ClosetProClient CreateClient() {
        return new ClosetProClient(_credentials.Username, _credentials.Password, _credentials.Instance);
    }

}
