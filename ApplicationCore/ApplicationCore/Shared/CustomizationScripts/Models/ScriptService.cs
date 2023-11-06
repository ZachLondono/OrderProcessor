using CADCodeProxy.Machining;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.Collections.Immutable;
using System.Reflection;

namespace ApplicationCore.Shared.CustomizationScripts.Models;

public class ScriptService<TInput, TResult> : ScriptService {

    private ScriptRunner<TResult>? _runner;
    public ImmutableArray<Diagnostic> Diagnostics { get; private set; }

    public static Type GlobalObjectType => typeof(ScriptGlobals<TInput>);

    private ScriptService() { }

    public static async Task<ScriptService<TInput, TResult>> FromFile(string scriptFilePath) {

        var service = new ScriptService<TInput, TResult>();

        var code = await File.ReadAllTextAsync(scriptFilePath);
        service.InitializeScript(code);

        return service;

    }

    public static ScriptService<TInput, TResult> FromCode(string code) {

        var service = new ScriptService<TInput, TResult>();

        service.InitializeScript(code);

        return service;

    }

    private void InitializeScript(string code) {

        var script = CSharpScript.Create<TResult>(
                                code: code,
                                options: ScriptOptions.Default.WithReferences(References).WithImports(Imports),
                                globalsType: typeof(ScriptGlobals<TInput>));

        Diagnostics = script.Compile();

        if (Diagnostics.Any()) return;

        _runner = script.CreateDelegate();

    }

    public async Task<TResult> RunScript(TInput input) {

        if (_runner is null) throw new InvalidOperationException();

        return await _runner(new ScriptGlobals<TInput>() {
            Input = input
        });

    }

}

public class ScriptService {

    public static Assembly[] References => new[] {
        typeof(object).Assembly,
        typeof(System.Text.RegularExpressions.Regex).Assembly,
        typeof(Enumerable).Assembly,
        typeof(ScriptService).Assembly,
        typeof(Part).Assembly
    };

    public static string[] Imports => new[] {
        "System",
        "System.Threading",
        "System.Threading.Tasks",
        "System.Collections",
        "System.Collections.Generic",
        "System.Text",
        "System.Text.RegularExpressions",
        "System.Linq",
        "System.IO",
        "System.Reflection"
    };

}
