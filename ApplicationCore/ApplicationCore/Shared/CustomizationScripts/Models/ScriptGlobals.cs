namespace ApplicationCore.Shared.CustomizationScripts.Models;

public class ScriptGlobals<TInput> {

    public required TInput Input { get; init; } = default;

}
