using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.CNC.GCode.Contracts;

public record BatchMaterial {
    public required string SheetStock { get; set; }
    public required Dimension Thickness { get; set; }
}