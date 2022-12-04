namespace ApplicationCore.Features.CNC.Services.Domain;

public record RouterBit(string Name, double Diameter, ToolRotation Rotation, double SpindleSpeed, double FeedSpeed, double EntrySpeed, double CornerFeed);
