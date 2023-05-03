namespace ApplicationCore.Features.CNC.Tools.Contracts;

public class ToolCarousel {

    public required string MachineName { get; init; }

    public required int PositionCount { get; init; }

    public required Tool[] Tools { get; init; }

}
