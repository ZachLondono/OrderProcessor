using ApplicationCore.Features.CADCode.Contracts;
using ApplicationCore.Features.CADCode.Services;
using ApplicationCore.Features.CADCode.Services.Services.CADCodeGCode.PDF;
using ApplicationCore.Infrastructure;

namespace ApplicationCore.Features.CADCode.Handlers;

public class CNCReleaseRequestHandler : CommandHandler<CNCReleaseRequest, IEnumerable<string>> {

    private readonly ICNCService _cncService;
    private readonly IReleasePDFService _pdfService;
    private readonly ICNCConfigurationProvider _configurationProvider;

    public CNCReleaseRequestHandler(ICNCService cncService, IReleasePDFService pdfService, ICNCConfigurationProvider configurationProvider) {
        _cncService = cncService;
        _pdfService = pdfService;
        _configurationProvider = configurationProvider;
    }

    public override async Task<Response<IEnumerable<string>>> Handle(CNCReleaseRequest command) {
        try {

            return await Task.Run(() => {

                var machineConfigurations = _configurationProvider.GetConfigurations();
                // TODO: do this in a task, otherwise application hangs while waiting for CADCode
                var job = _cncService.ExportToCNC(command.Batch, machineConfigurations);
                var filePaths = _pdfService.GeneratePDFs(job, command.ReportOutputDirectory);

                // TODO send emails to shop manager
                return new Response<IEnumerable<string>>(filePaths);

            });

        } catch (Exception e) {
            return new Response<IEnumerable<string>>(new Error() {
                Title = "Exception thrown while releasing cnc program",
                Details = e.ToString()
            });
        }
    }
}
