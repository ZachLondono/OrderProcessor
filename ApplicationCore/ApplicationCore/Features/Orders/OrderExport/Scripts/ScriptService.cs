using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace ApplicationCore.Features.Orders.OrderExport.Scripts;

public class ScriptService<TInput, TResult> {

    private ScriptRunner<TResult>? _runner;
    private readonly string _scriptFilePath;

    public ScriptService(string scriptFilePath) {
        _scriptFilePath = scriptFilePath;
    }

    public void LoadScript(Type[] referenceTypes) {

        var inputMetadata = MetadataReference.CreateFromFile(typeof(TInput).Assembly.Location);
        var resultMetadata = MetadataReference.CreateFromFile(typeof(TResult).Assembly.Location);

        var references = new List<MetadataReference>() {
            inputMetadata,
            resultMetadata
        };

        foreach (var type in referenceTypes) {
            references.Add(MetadataReference.CreateFromFile(type.Assembly.Location));
        }

        using var code = File.OpenRead(_scriptFilePath);

        var script = CSharpScript.Create<TResult>(
                                code: code,
                                options: ScriptOptions.Default.WithReferences(references),
                                globalsType: typeof(ScriptGlobals<TInput>));

        script.Compile();
        _runner = script.CreateDelegate();

    }


    public async Task<TResult> RunScript(TInput input) {
    
        if (_runner is null) throw new InvalidOperationException();
    
        return await _runner(new ScriptGlobals<TInput>() {
            Input = input
        });
    
    }

}