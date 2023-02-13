namespace ApplicationCore.Features.Shared.Services;

public interface ITemplateService {

    Task<string> FillTemplate(string template, object model);

}
