namespace ApplicationCore.Shared;

public interface ITemplateService {

    Task<string> FillTemplate(string template, object model);

}
