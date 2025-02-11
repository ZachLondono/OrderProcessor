using ApplicationCore.Features.MDFDoorOrders.OpenDoorOrders;
using Microsoft.Extensions.DependencyInjection;

namespace ApplicationCore.Features.MDFDoorOrders.DoorOrderRelease;

public class DoorOrderReleaseActionRunnerFactory {

    private readonly IServiceProvider _serviceProvider;

    public DoorOrderReleaseActionRunnerFactory(IServiceProvider serviceProvider) {
        _serviceProvider = serviceProvider;
    }

    public DoorOrderReleaseActionRunner CreateActionRunner(DoorOrder order, DoorOrderReleaseOptions options) {
        var actionRunner = _serviceProvider.GetRequiredService<DoorOrderReleaseActionRunner>();
        actionRunner.DoorOrder = order;
        actionRunner.Options = options;
        return actionRunner;
    }

}
