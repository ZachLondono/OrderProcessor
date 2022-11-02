using ClosedXML.Report;

namespace ApplicationCore.Features.ExcelTemplates.Domain;

public interface IExcelTemplate : IDisposable {

    void AddVariable(object value);

    XLGenerateResult Generate();

    void SaveAs(string filename);

}
