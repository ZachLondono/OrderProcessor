using QuestPDF.Infrastructure;
using ApplicationCore.Features.Tools.Contracts;
using ApplicationCore.Features.CNC.ReleasePDF.Services;
using ApplicationCore.Features.CNC.ReleasePDF.WSXML;
using ApplicationCore.Features.CNC.Contracts;

namespace ApplicationCore.Features.CNC.ReleasePDF;

internal class CNCReleaseDecorator : ICNCReleaseDecorator {

    private ReleasedJob? _jobData = null;

    private readonly IReleasePDFWriter _pdfService;
    private readonly CNCToolBox.GetToolCarousels _getToolCarousels;

    public CNCReleaseDecorator(IReleasePDFWriter pdfService, CNCToolBox.GetToolCarousels getToolCarousels) {
        _pdfService = pdfService;
        _getToolCarousels = getToolCarousels;
    }

    public async Task LoadDataFromFile(string reportFilePath, DateTime orderDate, string customerName, string vendorName) {

        var toolCarousels = await _getToolCarousels();
        // TODO: the ParseWSXMLReport method should only read the data from the file, it should not need any additional data passed to it. Another method in this class should then create the ReleasedJob instance from that data
        _jobData = WSXMLParser.ParseWSXMLReport(reportFilePath, orderDate, customerName, vendorName, toolCarousels);

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