namespace ApplicationCore.Features.ExcelTemplates.Domain;

public class ExcelTemplateFactory : IExcelTemplateFactory {

    public IExcelTemplate CreateTemplate(Stream stream) {
        // TODO: this can be cached
        return new ClosedXMLTemplate(stream);
    }

}
