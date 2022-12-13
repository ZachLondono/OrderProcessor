namespace ApplicationCore.Features.CNC.GCode.Domain.CADCode;

public class UsedInventory {
    public string Name { get; init; } = string.Empty;
    public double Width { get; init; }
    public double Length { get; init; }
    public double Thickness { get; init; }
    public bool IsGrained { get; init; }
    public int SheetsUsed { get; init; }
}
