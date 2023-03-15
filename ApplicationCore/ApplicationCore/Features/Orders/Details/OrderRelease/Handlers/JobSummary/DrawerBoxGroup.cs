namespace ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.JobSummary;

internal class DrawerBoxGroup {

    public string Room { get; set; } = string.Empty;
    public string Material { get; set; } = string.Empty;
    public string BottomMaterial { get; set; } = string.Empty;
    public string Clips { get; set; } = string.Empty;
    public string Notch { get; set; } = string.Empty;

    public List<DrawerBoxItem> Items { get; set; } = new();

}
