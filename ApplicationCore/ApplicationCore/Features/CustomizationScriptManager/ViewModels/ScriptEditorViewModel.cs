using System.ComponentModel;
using System.Runtime.CompilerServices;
using ApplicationCore.Shared.CustomizationScripts.Models;
using Microsoft.CodeAnalysis;

namespace ApplicationCore.Features.CustomizationScriptManager.ViewModels;

public class ScriptEditorViewModel<TInput, TOutput> : ScriptEditorViewModel, INotifyPropertyChanged {

    private string? _result;

    public string? Result {
        get { return _result; }
        private set {
            _result = value;
            OnPropertyChanged();
        }
    }

    public override void CompileScript(string text) {

        Result = null;

        var script = ScriptService<TInput, TOutput>.FromCode(text);
        var diagnostics = script.Diagnostics;

        if (diagnostics.Any()) {
            Result = string.Join(Environment.NewLine, diagnostics.Select(d => $"{d.Severity}\t{d.Location.GetLineSpan().StartLinePosition}\t{d.GetMessage()}"));
            return;
        }

        Result = "Compiled Successfully";

    }

    public override Task SaveToFile(string filePath, string text) => File.WriteAllTextAsync(filePath, text);

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

}

public abstract class ScriptEditorViewModel {

    public abstract void CompileScript(string text);

    public abstract Task SaveToFile(string filePath, string text);

}