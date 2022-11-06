using ApplicationCore.Features.CADCode.Services.Domain;

namespace ApplicationCore.Features.CADCode.Services;

internal interface ICNCConfigurationProvider {

    public IEnumerable<CNCMachineConfiguration> GetConfigurations();

}

internal class MockConfigurationProvider : ICNCConfigurationProvider {
    
    public IEnumerable<CNCMachineConfiguration> GetConfigurations() {

        var andiToolMap = new ToolMap(10);
        andiToolMap.AddTool(1, new("3-8Comp", 9.5, ToolRotation.Auto, 0, 0, 0, 0));
        andiToolMap.AddTool(6, new("Pocket9", 9, ToolRotation.Auto, 0, 0, 0, 0));
        andiToolMap.DrillBlock.Add(new("", 5, 0, 0));

        var omniToolMap = new ToolMap(8);
        omniToolMap.AddTool(1, new("3-8Comp", 9.5, ToolRotation.Auto, 0, 0, 0, 0));
        omniToolMap.AddTool(6, new("Pocket9", 9, ToolRotation.Auto, 0, 0, 0, 0));
        omniToolMap.DrillBlock.Add(new("", 5, 0, 0));

        return new List<CNCMachineConfiguration>() {
            new() {
                MachineName = "Andi Stratos",
                ToolMap = andiToolMap,
                Orientation = TableOrientation.Standard
            },
            /*new() {
                MachineName = "Omnitech",
                ToolMap = omniToolMap,
                Orientation = TableOrientation.Rotated
            }*/
        };
    }

}