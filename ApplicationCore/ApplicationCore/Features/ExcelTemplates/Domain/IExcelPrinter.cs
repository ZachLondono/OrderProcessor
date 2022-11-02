namespace ApplicationCore.Features.ExcelTemplates.Domain;

public interface IExcelPrinter {

    // TODO: maybe add option to set print area, not neccessary now 
    public Task PrintFile(string filePath);

}
