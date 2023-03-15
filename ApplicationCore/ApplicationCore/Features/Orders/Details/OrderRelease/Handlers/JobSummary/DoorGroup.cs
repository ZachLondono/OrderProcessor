namespace ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.JobSummary;

internal class DoorGroup {

    public string Room { get; set; } = string.Empty;
    public string Material { get; set; } = string.Empty;
    public string Style { get; set; } = string.Empty;
    public string Finish { get; set; } = string.Empty;

    public List<DoorItem> Items { get; set; } = new();

}
