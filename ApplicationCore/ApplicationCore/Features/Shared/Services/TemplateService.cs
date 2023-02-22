using RazorEngineCore;
using RazorEngine = RazorEngineCore.RazorEngine;

namespace ApplicationCore.Features.Shared.Services;

internal class TemplateService : ITemplateService {

    private readonly IDictionary<string, IRazorEngineCompiledTemplate> _templates;
    private readonly IRazorEngine _engine;

    public TemplateService() {
        _engine = new RazorEngine();
        _templates = new Dictionary<string, IRazorEngineCompiledTemplate>();
    }

    public async Task<string> FillTemplate(string template, object model) {

        IRazorEngineCompiledTemplate compiledTemplate;
        if (_templates.ContainsKey(template)) {
            compiledTemplate = _templates[template];
        } else {
            compiledTemplate = _engine.Compile(template);
            _templates.Add(template, compiledTemplate);
        }

        return await compiledTemplate.RunAsync(model);

    }

}