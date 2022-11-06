using ApplicationCore.Features.CADCode.Contracts;
using ApplicationCore.Features.CADCode.Services;

namespace ApplicationCore.Features.CADCode.Handlers;

internal class CNCReleaseRequestHandler /* : AsyncRequestHandler<OrderReleaseTriggerHandler> */ {

    private readonly ICNCService _cncService;
    private readonly ICNCConfigurationProvider _configurationProvider;

    public CNCReleaseRequestHandler(ICNCService cncService, ICNCConfigurationProvider configurationProvider) {
        _cncService = cncService;
        _configurationProvider = configurationProvider;
    }

    public void Handle(CNCReleaseRequest notification) {
        var machineConfigurations = _configurationProvider.GetConfigurations();
        _cncService.ExportToCNC(notification.Batch, machineConfigurations);
    }

}
