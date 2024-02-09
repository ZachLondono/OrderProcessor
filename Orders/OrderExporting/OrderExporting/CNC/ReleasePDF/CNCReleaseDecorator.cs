using QuestPDF.Infrastructure;
using OrderExporting.CNC.ReleasePDF.Services;
using OrderExporting.CNC.Programs.Job;

namespace OrderExporting.CNC.ReleasePDF;

public class CNCReleaseDecorator : ICNCReleaseDecorator {

    private ReleasedJob? _jobData = null;

    private readonly ReleasePDFDecoratorFactory _pdfService;

    public CNCReleaseDecorator(ReleasePDFDecoratorFactory pdfService) {
        _pdfService = pdfService;
    }

    public void AddData(ReleasedJob job) {
        _jobData = job;
    }

    public void Decorate(IDocumentContainer container) {

        if (_jobData is null) {
            return;
        }

        var decorators = _pdfService.GenerateDecorators(_jobData);

        foreach (var decorator in decorators) {

            decorator.Decorate(container);

        }

        return;

    }

}