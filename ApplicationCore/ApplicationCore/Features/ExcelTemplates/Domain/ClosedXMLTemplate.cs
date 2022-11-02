using ClosedXML.Report;

namespace ApplicationCore.Features.ExcelTemplates.Domain;

public class ClosedXMLTemplate : IExcelTemplate {

    private readonly XLTemplate _template;

    public ClosedXMLTemplate(Stream stream) {
        _template = new XLTemplate(stream);
    }

    public void AddVariable(object value) => _template.AddVariable(value);

    public XLGenerateResult Generate() => _template.Generate();

    public void SaveAs(string filename) => _template.SaveAs(filename);

    public void Dispose() => _template.Dispose();
}