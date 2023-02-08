using ApplicationCore.Features.Shared;

namespace ApplicationCore.Features.ExcelTemplates.Domain;

internal class ClosedXMLTemplateFiller : ITemplateFiller {

    private readonly ClosedXMLTemplateConfiguration _configuration;
    private readonly IFileReader _fileReader;
    private readonly IExcelTemplateFactory _factory;
    private readonly IExcelPrinter _printer;

    public ClosedXMLTemplateFiller(ClosedXMLTemplateConfiguration configuration, IFileReader fileReader, IExcelTemplateFactory factory, IExcelPrinter printer) {
        _configuration = configuration;
        _fileReader = fileReader;
        _factory = factory;
        _printer = printer;
    }


    public async Task<string> FillTemplate(object model, string outputDirectory, string filename, bool print) {

        var filepath = await Task.Run(() => {

            using var stream = _fileReader.OpenReadFileStream(_configuration.TemplateFilePath);
            using var template = _factory.CreateTemplate(stream);
            template.AddVariable(model);
            _ = template.Generate();
            var filepath = _fileReader.GetAvailableFileName(outputDirectory, filename, ".xlsx");
            template.SaveAs(filepath);

            return filepath;

        });

        if (print) {
            await _printer.PrintFile(filepath);
        }

        return filepath;

    }

}
