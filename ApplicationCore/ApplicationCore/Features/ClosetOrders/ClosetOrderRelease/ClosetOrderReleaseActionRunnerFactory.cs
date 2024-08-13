using ApplicationCore.Features.ClosetOrders.ClosetOrderSelector;
using Microsoft.Extensions.DependencyInjection;

namespace ApplicationCore.Features.ClosetOrders.ClosetOrderRelease;

public class ClosetOrderReleaseActionRunnerFactory(IServiceProvider serviceProvider) {

    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public ClosetOrderReleaseActionRunner CreateActionRunner(ClosetOrder order, ClosetOrderReleaseOptions options) {
        var actionRunner = _serviceProvider.GetRequiredService<ClosetOrderReleaseActionRunner>();
        actionRunner.ClosetOrder = order;
        actionRunner.Options = options;
        return actionRunner;
    }

}
