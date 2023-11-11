using Microsoft.CodeAnalysis.Diagnostics;
using RoslynPad.Roslyn;
using System.Reflection;

namespace ApplicationCore.Features.CustomizationScriptManager;

public class CustomRoslynHost : RoslynHost {

    private bool _addedAnalyzers;
    private Type? _projectHostObjectType;

    public CustomRoslynHost(Type? projectHostObjectType, IEnumerable<Assembly>? additionalAssemblies, RoslynHostReferences? references)
        : base(additionalAssemblies, references, null) {
        _projectHostObjectType = projectHostObjectType;
    }

    protected override Type? GetProjectHostObjectType() => _projectHostObjectType;

    // TODO: workaround for GetSolutionAnalyzerReferences bug (should be added once per Solution)
    protected override IEnumerable<AnalyzerReference> GetSolutionAnalyzerReferences() {
        if (!_addedAnalyzers) {
            _addedAnalyzers = true;
            return base.GetSolutionAnalyzerReferences();
        }

        return Enumerable.Empty<AnalyzerReference>();
    }

}

