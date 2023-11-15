using ApplicationCore.Features.CustomizationScriptManager;
using ApplicationCore.Features.CustomizationScriptManager.ViewModels;
using ApplicationCore.Shared.CustomizationScripts.Models;
using Microsoft.CodeAnalysis;
using RoslynPad.Editor;
using RoslynPad.Roslyn;
using System;
using System.IO;
using System.Reflection;
using System.Windows;

namespace DesktopHost.ScriptEditor;

public partial class ScriptEditor : Window {

    public bool WasFileLoaded { get; private set; } = false;

    private readonly RoslynHost _host;
    private RoslynCodeEditor? _currentEditor;
    private string _filePath;

    public ScriptEditor(string filePath, Type inputType, Type outputType) {
        _filePath = filePath;
        InitializeComponent();

        Assembly[] additionalAssemblies = new[] {
            Assembly.Load("RoslynPad.Roslyn.Windows"),
            Assembly.Load("RoslynPad.Editor.Windows")
        };

        Type hostType = typeof(CustomRoslynHost<>).MakeGenericType(inputType);
        var hostInstance = Activator.CreateInstance(hostType, additionalAssemblies, RoslynHostReferences.NamespaceDefault.With(assemblyReferences: ScriptService.References, imports: ScriptService.Imports));
        if (hostInstance is not RoslynHost host) {
            throw new InvalidOperationException();
        }

        _host = host;

        Type vmType = typeof(ScriptEditorViewModel<,>).MakeGenericType(inputType, outputType);
        var vmInstance = Activator.CreateInstance(vmType);
        if (vmInstance is null) {
            throw new InvalidOperationException();
        }

        Editor.DataContext = vmInstance;
        Results.DataContext = vmInstance;

    }

    private async void OnItemLoaded(object sender, EventArgs e) {

        var editor = (RoslynCodeEditor)sender;
        editor.Loaded -= OnItemLoaded;
        editor.Focus();

        var workingDirectory = Directory.GetCurrentDirectory();

        string contents;

        try {
            contents = await File.ReadAllTextAsync(_filePath);
            WasFileLoaded = true;
        } catch (Exception ex) {
            WasFileLoaded = false;
            contents = $"Script could not be read - '{ex.Message}'";
        }

        var documentId = await editor.InitializeAsync(_host,
                                                      new ClassificationHighlightColors(),
                                                      workingDirectory,
                                                      contents,
                                                      SourceCodeKind.Script).ConfigureAwait(true);

        _currentEditor = editor;

    }

    private void Compile(object sender, RoutedEventArgs e) {
        if (_currentEditor is null || !WasFileLoaded) return;
        var viewModel = (ScriptEditorViewModel)_currentEditor.DataContext;
        viewModel.CompileScript(_currentEditor.Text);
    }

    private async void SaveChanges(object sender, RoutedEventArgs e) {
        if (_currentEditor is null || !WasFileLoaded) return;
        var viewModel = (ScriptEditorViewModel)_currentEditor.DataContext;
        await viewModel.SaveToFile(_filePath, _currentEditor.Text);
    }

}
