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

    public void LoadScript() {

        var script = BuildScriptFromFile(_scriptFilePath);
        script.Compile();
        _runner = script.CreateDelegate();

    }


    public async Task<TResult> RunScript(TInput input) {
    
        if (_runner is null) throw new InvalidOperationException();
    
        return await _runner(new ScriptGlobals<TInput>() {
            Input = input
        });
    
    }

    public static Script<TResult> BuildScriptFromFile(string filePath) {

        var references = new List<MetadataReference>() {
            MetadataReference.CreateFromFile(typeof(TInput).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(TResult).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(CADCodeProxy.Machining.Part).Assembly.Location),
        };

        using var code = File.OpenRead(filePath);

        return CSharpScript.Create<TResult>(
                                code: code,
                                options: ScriptOptions.Default.WithReferences(references),
                                globalsType: typeof(ScriptGlobals<TInput>));
    }


}