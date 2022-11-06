using ApplicationCore.Features.CADCode.Contracts;
using ApplicationCore.Features.CADCode.Services;
using ApplicationCore.Infrastructure;

namespace ApplicationCore.Features.CADCode.Handlers;

internal class CNCReleaseRequestHandler : CommandHandler<CNCReleaseRequest> {

    private readonly ICNCService _cncService;
    private readonly ICNCConfigurationProvider _configurationProvider;

    public CNCReleaseRequestHandler(ICNCService cncService, ICNCConfigurationProvider configurationProvider) {
        _cncService = cncService;
        _configurationProvider = configurationProvider;
    }

    public override Task<Response> Handle(CNCReleaseRequest command) {
        try {
            var machineConfigurations = _configurationProvider.GetConfigurations();
            _cncService.ExportToCNC(command.Batch, machineConfigurations);
            return Task.FromResult(new Response());
        } catch (Exception e) {
            return Task.FromResult(new Response(new Error($"Exception thrown while releasing cnc program {e}")));
        }

    }
}
