namespace ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.ReleasePDF.Contracts;

public class ProgramMaterial {

    public string Name { get; init; } = string.Empty;
    public double Width { get; init; }
    public double Length { get; init; }
    public double Thickness { get; init; }
    public bool IsGrained { get; init; }
    public double Yield { get; init; }

}