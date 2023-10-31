namespace ApplicationCore.Shared.CustomizationScripts.Models;

public class CustomizationScript {

    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public CustomizationType Type { get; set; }

}
