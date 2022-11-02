using ApplicationCore.Features.ExcelTemplates.Contracts;
using ApplicationCore.Shared;

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
            var filepath = GetAvailableFileName(outputDirectory, filename);
            template.SaveAs(filepath);

            return filepath;

        });

        if (print) {
            await _printer.PrintFile(filepath);
        }

        return filepath;

    }

    private string GetAvailableFileName(string direcotry, string filename) {

        int index = 1;

        string filepath = Path.Combine(direcotry, $"{filename}.xlsx");

        while (_fileReader.DoesFileExist(filepath)) {

            filepath = Path.Combine(direcotry, $"{filename} ({index++}).xlsx");

        }

        return filepath;

    }

}
