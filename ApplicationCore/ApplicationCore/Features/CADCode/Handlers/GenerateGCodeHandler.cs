using ApplicationCore.Features.CADCode.Contracts;
using ApplicationCore.Features.CADCode.Contracts.ProgramRelease;
using ApplicationCore.Features.CADCode.Services;
using ApplicationCore.Infrastructure;

namespace ApplicationCore.Features.CADCode.Handlers;

public class CNCReleaseRequestHandler : CommandHandler<CNCReleaseRequest, ReleasedJob> {

    private readonly ICNCService _cncService;
    private readonly ICNCConfigurationProvider _configurationProvider;

    public CNCReleaseRequestHandler(ICNCService cncService, ICNCConfigurationProvider configurationProvider) {
        _cncService = cncService;
        _configurationProvider = configurationProvider;
    }

    public override async Task<Response<ReleasedJob>> Handle(CNCReleaseRequest command) {
        try {

            return await Task.Run<Response<ReleasedJob>>(() => {

                var machineConfigurations = _configurationProvider.GetConfigurations();
                ReleasedJob job = _cncService.ExportToCNC(command.Batch, machineConfigurations);
                return new(job);

            });

        } catch (Exception e) {
            return new Response<ReleasedJob>(new Error() {
                Title = "Exception thrown while releasing cnc program",
                Details = e.ToString()
            });
        }
    }
}
