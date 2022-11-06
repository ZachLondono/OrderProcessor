namespace ApplicationCore.Features.CADCode.Services.Domain;

public class CNCMachineConfiguration {

    public string MachineName { get; init; } = string.Empty;
    public ToolMap ToolMap { get; init; } = new(0);
    public TableOrientation Orientation { get; init; } = TableOrientation.Standard;

}
