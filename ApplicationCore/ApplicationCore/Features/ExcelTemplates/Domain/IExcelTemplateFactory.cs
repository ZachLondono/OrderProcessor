namespace ApplicationCore.Features.ExcelTemplates.Domain;

public interface IExcelTemplateFactory {

    public IExcelTemplate CreateTemplate(Stream stream);

}
