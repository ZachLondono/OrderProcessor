using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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

    protected override Project CreateProject(Solution solution, DocumentCreationArgs args, CompilationOptions compilationOptions, Project? previousProject = null) {

        var name = args.Name ?? "New";
        var path = Path.Combine(args.WorkingDirectory, name);
        var id = ProjectId.CreateNewId(name);

        var parseOptions = ParseOptions.WithKind(args.SourceCodeKind);
        var isScript = args.SourceCodeKind == SourceCodeKind.Script;

        if (isScript) {
            compilationOptions = compilationOptions.WithScriptClassName(name);
        }

        var analyzerConfigDocuments = AnalyzerConfigFiles.Where(File.Exists).Select(file => DocumentInfo.Create(
            DocumentId.CreateNewId(id, debugName: file),
            name: file,
            loader: new FileTextLoader(file, defaultEncoding: null),
            filePath: file));

        solution = solution.AddProject(ProjectInfo.Create(
            id,
            VersionStamp.Create(),
            name,
            name,
            LanguageNames.CSharp,
            filePath: path,
            isSubmission: isScript,
            parseOptions: parseOptions,
            hostObjectType: _projectHostObjectType, // Set the project host object type, which is the type of the 'global' variable available to scripts
            compilationOptions: compilationOptions,
            metadataReferences: previousProject != null ? [] : DefaultReferences,
            projectReferences: previousProject != null ? new[] { new ProjectReference(previousProject.Id) } : null)
            .WithAnalyzerConfigDocuments(analyzerConfigDocuments));

        var project = solution.GetProject(id)!;

        if (!isScript && GetUsings(project) is { Length: > 0 } usings) {
            project = project.AddDocument("RoslynPadGeneratedUsings", usings).Project;
        }

        return project;

        static string GetUsings(Project project) {
            if (project.CompilationOptions is CSharpCompilationOptions options) {
                return string.Join(" ", options.Usings.Select(i => $"global using {i};"));
            }

            return string.Empty;
        }
    }

    // TODO: workaround for GetSolutionAnalyzerReferences bug (should be added once per Solution)
    protected override IEnumerable<AnalyzerReference> GetSolutionAnalyzerReferences() {
        if (!_addedAnalyzers) {
            _addedAnalyzers = true;
            return base.GetSolutionAnalyzerReferences();
        }

        return Enumerable.Empty<AnalyzerReference>();
    }

}

