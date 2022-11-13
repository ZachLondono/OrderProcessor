using ApplicationCore.Features.CADCode.Contracts;
using ApplicationCore.Features.CADCode.Services.Services.CADCodeGCode.PDF;
using ApplicationCore.Infrastructure;

namespace ApplicationCore.Features.CADCode.Handlers;

public class GenerateCNCReleasePDFHandler : CommandHandler<GenerateCNCReleasePDFRequest, IEnumerable<string>> {

    private readonly IReleasePDFService _pdfService;

    public GenerateCNCReleasePDFHandler(IReleasePDFService pdfService) {
        _pdfService = pdfService;
    }

    public override Task<Response<IEnumerable<string>>> Handle(GenerateCNCReleasePDFRequest command) {

		var filePaths = _pdfService.GeneratePDFs(command.Job, command.ReportOutputDirectory);
		// TODO: warn when no data was released
		return Task.FromResult(
            new Response<IEnumerable<string>>(filePaths)
        );

	}

}
