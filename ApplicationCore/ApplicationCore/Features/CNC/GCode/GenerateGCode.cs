using ApplicationCore.Features.CNC.GCode.Contracts;
using ApplicationCore.Features.CNC.GCode.Domain;
using ApplicationCore.Features.CNC.GCode.Services;
using ApplicationCore.Features.CNC.ReleasePDF.Contracts;
using ApplicationCore.Infrastructure;

namespace ApplicationCore.Features.CNC.GCode;

public class GenerateGCode { 

    public record Command(CNCBatch Batch) : ICommand<ReleasedJob>;

    public class Handler : CommandHandler<Command, ReleasedJob> {

        private readonly ICNCService _cncService;
        private readonly ICNCConfigurationProvider _configurationProvider;

        public Handler(ICNCService cncService, ICNCConfigurationProvider configurationProvider) {
            _cncService = cncService;
            _configurationProvider = configurationProvider;
        }

        public override async Task<Response<ReleasedJob>> Handle(Command command) {
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

}