namespace ApplicationCore.Features.CNC.GCode.Configuration;

public class GCodeGenerationConfiguration {
	public string InventoryFilePath { get; set; } = string.Empty;
	public GCodeMachineConfiguration DefaultMachineConfiguration { get; set; } = new();
	public Dictionary<string, GCodeMachineConfiguration> Machines { get; set; } = new();
}
