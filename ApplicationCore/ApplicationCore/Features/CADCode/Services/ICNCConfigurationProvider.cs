using ApplicationCore.Features.CADCode.Services.Domain;

namespace ApplicationCore.Features.CADCode.Services;

public interface ICNCConfigurationProvider {

    public IEnumerable<CNCMachineConfiguration> GetConfigurations();

}

public class MockConfigurationProvider : ICNCConfigurationProvider {
    
    public IEnumerable<CNCMachineConfiguration> GetConfigurations() {

        var andiToolMap = new ToolMap(10);
        andiToolMap.AddTool(1, new("3-8comp", 9.5, ToolRotation.Auto, 0, 0, 0, 0));
		andiToolMap.AddTool(6, new("pocket9", 9, ToolRotation.Auto, 0, 0, 0, 0));
		andiToolMap.AddTool(8, new("pocket3", 3, ToolRotation.Auto, 0, 0, 0, 0));
		andiToolMap.AddTool(3, new("pocket44", 44, ToolRotation.Auto, 0, 0, 0, 0));
		andiToolMap.DrillBlock.Add(new("", 5, 0, 0));

        var omniToolMap = new ToolMap(8);
		omniToolMap.AddTool(1, new("3-8comp", 9.5, ToolRotation.Auto, 0, 0, 0, 0));
		omniToolMap.AddTool(6, new("pocket9", 9, ToolRotation.Auto, 0, 0, 0, 0));
		omniToolMap.AddTool(8, new("pocket3", 3, ToolRotation.Auto, 0, 0, 0, 0));
		omniToolMap.AddTool(3, new("pocket44", 44, ToolRotation.Auto, 0, 0, 0, 0));
		omniToolMap.DrillBlock.Add(new("", 5, 0, 0));

        return new List<CNCMachineConfiguration>() {
            new() {
                MachineName = "Andi Stratos",
                ToolMap = andiToolMap,
                Orientation = TableOrientation.Standard
            },
            new() {
                MachineName = "Omnitech",
                ToolMap = omniToolMap,
                Orientation = TableOrientation.Rotated
            }
        };
    }

}