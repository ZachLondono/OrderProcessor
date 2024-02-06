using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using Domain.ValueObjects;

namespace ApplicationCore.Features.Orders.OrderRelease.Handlers.JobSummary;

internal class CabinetItem {

    public int Line { get; set; }
    public int Qty { get; set; }
    public string Description { get; set; } = string.Empty;
    public Dimension Width { get; set; }
    public Dimension Height { get; set; }
    public Dimension Depth { get; set; }
    public CabinetSideType LeftSide { get; set; }
    public CabinetSideType RightSide { get; set; }
    public string[] Comments { get; set; } = Array.Empty<string>();

}
