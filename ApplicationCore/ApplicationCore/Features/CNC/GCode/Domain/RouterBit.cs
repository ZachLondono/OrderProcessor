namespace ApplicationCore.Features.CNC.GCode.Domain;

public record RouterBit(string Name, double Diameter, ToolRotation Rotation, double SpindleSpeed, double FeedSpeed, double EntrySpeed, double CornerFeed);
