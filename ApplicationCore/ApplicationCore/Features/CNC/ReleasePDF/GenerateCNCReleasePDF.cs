using ApplicationCore.Features.CNC.GCode.Contracts.ProgramRelease;
using ApplicationCore.Features.CNC.ReleasePDF.Services;
using ApplicationCore.Infrastructure;

namespace ApplicationCore.Features.CNC.ReleasePDF;

public class GenerateCNCReleasePDF { 

    public record Command(ReleasedJob Job, string ReportOutputDirectory) : ICommand<IEnumerable<string>>;

    public class Handler : CommandHandler<Command, IEnumerable<string>> {

        private readonly IReleasePDFWriter _pdfService;

        public Handler(IReleasePDFWriter pdfService) {
            _pdfService = pdfService;
        }

        public override Task<Response<IEnumerable<string>>> Handle(Command command) {

            try {
                
                var filePaths = _pdfService.GeneratePDFs(command.Job, command.ReportOutputDirectory);
                
                return Task.FromResult(
                    new Response<IEnumerable<string>>(filePaths)
                );

			} catch (Exception e) {

                return Task.FromResult(
                    new Response<IEnumerable<string>>(new Error() {
                        Title = "Exception thrown while generating PDF",
                        Details = e.ToString()
                    })
                );

            }


		}

    }

}