using ApplicationCore.Features.CustomizationScriptManager;
using System;

namespace DesktopHost.ScriptEditor;

internal class ScriptEditorOpener : IScriptEditorOpener {

    public void OpenScriptEditor(string filePath, Type inputType, Type outputType) {
        new ScriptEditor(filePath, inputType, outputType).Show();
    }

}
