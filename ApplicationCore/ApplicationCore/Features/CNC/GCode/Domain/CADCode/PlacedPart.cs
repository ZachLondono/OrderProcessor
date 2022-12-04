using ApplicationCore.Features.CNC.GCode.Contracts.Machining;

namespace ApplicationCore.Features.CNC.GCode.Domain.CADCode;

/// <summary>
/// Represents a single part that is placed in a CNC program
/// </summary>
public class PlacedPart
{

    public string FileName { get; init; } = string.Empty;

    /// <summary>
    /// The index of the inventory in the optimization result
    /// </summary>
    public int UsedInventoryIndex { get; init; }

    public int ProgramIndex { get; init; }

    /// <summary>
    /// The point at which the part was inserted onto the sheet
    /// </summary>
    public Point InsertionPoint { get; init; } = new(0, 0);

    /// <summary>
    /// True if the part had to be rotated to fit on the optimized sheet
    /// </summary>
    public bool IsRotated { get; init; } = false;

    public double Area { get; init; }

}