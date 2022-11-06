namespace ApplicationCore.Features.CADCode.Services.Domain;

internal record RouterBit(string Name, double Diameter, ToolRotation Rotation, double SpindleSpeed, double FeedSpeed, double EntrySpeed, double CornerFeed);
