using ApplicationCore.Features.CNC.GCode.Domain;

namespace ApplicationCore.Features.CNC.GCode.Configuration;

public class GCodeMachineConfiguration {
    public string ToolFilePath { get; set; } = string.Empty;
    public string NestOutputDirectory { get; set; } = string.Empty;
    public string SinglePartOutputDirectory { get; set; } = string.Empty;
    public TableOrientation TableOrientation { get; set; }
}