namespace ApplicationCore.Features.CNC.GCode.Domain;

// TODO: remove references to this in other features
// TODO: Instead of "standard" or "rotated" specify which axis (x or y) is the long axis of the table
public enum TableOrientation {
    Standard,
    Rotated
}