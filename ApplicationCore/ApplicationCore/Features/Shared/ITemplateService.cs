namespace ApplicationCore.Features.Shared;

public interface ITemplateService {

    Task<string> FillTemplate(string template, object model);

}
