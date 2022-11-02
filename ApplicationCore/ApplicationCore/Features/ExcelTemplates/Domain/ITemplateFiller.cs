namespace ApplicationCore.Features.ExcelTemplates.Domain;

public interface ITemplateFiller {

    public Task<string> FillTemplate(object model, string outputDirectory, string filename, bool print);

}
