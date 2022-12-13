using ApplicationCore.Features.CNC.GCode.Contracts.Machining;

namespace ApplicationCore.Features.CNC.GCode.Contracts;

public class PartFace {
    public required string FileName { get; set; }
    public required IEnumerable<MachiningOperation> Operations { get; set; }
}