using ApplicationCore.Shared.CustomizationScripts.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using RoslynPad.Roslyn;
using System.Reflection;

namespace ApplicationCore.Features.CustomizationScriptManager;

// TODO: workaround for GetSolutionAnalyzerReferences bug (should be added once per Solution)
public class CustomRoslynHost<TInput> : RoslynHost {

    private bool _addedAnalyzers;

    public CustomRoslynHost(IEnumerable<Assembly>? additionalAssemblies, RoslynHostReferences? references)
        : base(additionalAssemblies, references, null) {
    }

    protected override Project CreateProject(Solution solution, DocumentCreationArgs args, CompilationOptions compilationOptions, Project? previousProject = null) {

        var name = args.Name ?? "New";
        var path = Path.Combine(args.WorkingDirectory, name);
        var id = ProjectId.CreateNewId(name);

        var parseOptions = ParseOptions.WithKind(args.SourceCodeKind);
        compilationOptions = compilationOptions.WithScriptClassName(name);

        List<MetadataReference> references = new(DefaultReferences);
        var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);
        references.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "mscorlib.dll")));
        references.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.dll")));
        references.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Core.dll")));
        references.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll")));

        var projInfo = ProjectInfo.Create(
            id,
            VersionStamp.Create(),
            name,
            name,
            LanguageNames.CSharp,
            filePath: path,
            isSubmission: true,
            parseOptions: parseOptions,
            hostObjectType: typeof(ScriptGlobals<TInput>),
            compilationOptions: compilationOptions,
            metadataReferences: references,
            projectReferences: null);

        solution = solution.AddProject(projInfo);

        return solution.GetProject(id)!;

    }

    protected override IEnumerable<AnalyzerReference> GetSolutionAnalyzerReferences() {
        if (!_addedAnalyzers) {
            _addedAnalyzers = true;
            return base.GetSolutionAnalyzerReferences();
        }

        return Enumerable.Empty<AnalyzerReference>();
    }
}

