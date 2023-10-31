using ApplicationCore.Features.CustomizationScriptManager;

namespace DesktopHost.ScriptEditor;

internal class ScriptEditorOpener : IScriptEditorOpener {

    public void OpenScriptEditor() {
        new ScriptEditor().Show();
    }

}
