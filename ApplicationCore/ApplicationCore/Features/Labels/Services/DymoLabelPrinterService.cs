using ApplicationCore.Features.Labels.Contracts;
using ApplicationCore.Features.Labels.Domain;

namespace ApplicationCore.Features.Labels.Services;

internal class DymoLabelPrinterService : ILabelPrinterService {

    private readonly ILabelTemplateReader _reader;

    public DymoLabelPrinterService(ILabelTemplateReader reader) {
        _reader = reader;
    }

    public Task<bool> PrintLabelAsync(Label label, LabelPrinterConfiguration configuration) {

        var template = _reader.GetTemplateFromFile(configuration.LabelTempalteFilePath);

        // TODO: print labels to dymo printer

        return Task.FromResult(true);
    }

}
