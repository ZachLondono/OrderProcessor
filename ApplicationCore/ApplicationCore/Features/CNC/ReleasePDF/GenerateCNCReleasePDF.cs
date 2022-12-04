using ApplicationCore.Features.CNC.ReleasePDF.Contracts;
using ApplicationCore.Features.CNC.ReleasePDF.Services;
using ApplicationCore.Infrastructure;

namespace ApplicationCore.Features.CNC.ReleasePDF;

public class GenerateCNCReleasePDF { 

    public record Command(ReleasedJob Job, string ReportOutputDirectory) : ICommand<PDFGenerationResult>;

    public class Handler : CommandHandler<Command, PDFGenerationResult> {

        private readonly IReleasePDFWriter _pdfService;

        public Handler(IReleasePDFWriter pdfService) {
            _pdfService = pdfService;
        }

        public override Task<Response<PDFGenerationResult>> Handle(Command command) {

            try {
                
                var filePaths = _pdfService.GeneratePDFs(command.Job, command.ReportOutputDirectory);
                
                return Task.FromResult(
                    new Response<PDFGenerationResult>(new PDFGenerationResult() {
                        FilePaths = filePaths
					})
                );

			} catch (Exception e) {

                return Task.FromResult(
                    new Response<PDFGenerationResult>(new Error() {
                        Title = "Exception thrown while generating PDF",
                        Details = e.ToString()
                    })
                );

            }


		}

    }

}