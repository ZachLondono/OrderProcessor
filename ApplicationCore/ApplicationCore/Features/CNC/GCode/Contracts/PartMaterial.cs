namespace ApplicationCore.Features.CNC.GCode.Contracts;

public record PartMaterial {

    public string Name { get; init; } = string.Empty;
    public double Thickness { get; init; }

}