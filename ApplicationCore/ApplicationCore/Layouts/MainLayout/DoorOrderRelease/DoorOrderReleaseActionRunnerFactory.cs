using ApplicationCore.Features.OpenDoorOrders;
using Microsoft.Extensions.DependencyInjection;

namespace ApplicationCore.Layouts.MainLayout.DoorOrderRelease;

public class DoorOrderReleaseActionRunnerFactory {

	private readonly IServiceProvider _serviceProvider;

	public DoorOrderReleaseActionRunnerFactory(IServiceProvider serviceProvider) {
		_serviceProvider = serviceProvider;
	}

	public DoorOrderReleaseActionRunner CreateActionRunner(DoorOrder doorOrder) {
		var actionRunner = _serviceProvider.GetRequiredService<DoorOrderReleaseActionRunner>();
		actionRunner.DoorOrder = doorOrder;
		return actionRunner;
	}

}
