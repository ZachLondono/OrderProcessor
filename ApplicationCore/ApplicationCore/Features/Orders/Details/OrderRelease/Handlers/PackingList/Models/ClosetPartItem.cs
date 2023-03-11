namespace ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.PackingList.Models;

internal class ClosetPartItem {

    public int Line { get; set; }

    public int Qty { get; set; }

    public string Description { get; set; } = string.Empty;

    public string Length { get; set; } = string.Empty;

    public string Width { get; set; } = string.Empty;

}
