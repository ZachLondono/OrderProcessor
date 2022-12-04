using ApplicationCore.Features.CNC.GCode.Contracts;
using ApplicationCore.Features.CNC.GCode.Domain;
using ApplicationCore.Features.CNC.ReleasePDF.Contracts;
using ApplicationCore.Infrastructure;

namespace ApplicationCore.Features.CNC.GCode;

public class CNCReleaseRequestHandler : CommandHandler<CNCReleaseRequest, ReleasedJob> {

    private readonly ICNCService _cncService;
    private readonly ICNCConfigurationProvider _configurationProvider;

    public CNCReleaseRequestHandler(ICNCService cncService, ICNCConfigurationProvider configurationProvider) {
        _cncService = cncService;
        _configurationProvider = configurationProvider;
    }

    public override async Task<Response<ReleasedJob>> Handle(CNCReleaseRequest command) {
        try {

            var machineConfigurations = _configurationProvider.GetConfigurations();
            ReleasedJob job = await _cncService.ExportToCNC(command.Batch, machineConfigurations);
            return new(job);

        } catch (CADCodeFailedToInitilizeException) {
            return new Response<ReleasedJob>(new Error() {
                Title = "Could not start CADCode",
                Details = "Missing CADCode license key or CADCode failed while trying to initilize"
            });
        } catch (Exception e) {
            return new Response<ReleasedJob>(new Error() {
                Title = "Exception thrown while releasing cnc program",
                Details = e.ToString()
            });
        }
    }
}
