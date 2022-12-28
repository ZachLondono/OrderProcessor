namespace ApplicationCore.Features.CNC.GCode.Configuration;

/// <summary>
/// Settings required to correctly generate the gcode for specific machines
/// </summary>
public class GCodeGenerationConfiguration {
	public string InventoryFilePath { get; set; } = string.Empty;
	public GCodeMachineConfiguration DefaultMachineConfiguration { get; set; } = new();
	public Dictionary<string, GCodeMachineConfiguration> Machines { get; set; } = new();
}
