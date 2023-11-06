namespace ApplicationCore.Features.CustomizationScriptManager;

public interface IScriptEditorOpener {

    public void OpenScriptEditor(string filePath, Type inputType, Type outputType);

}
