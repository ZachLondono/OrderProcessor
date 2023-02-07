namespace ApplicationCore.Features.Orders.Release.Handlers.PackingList.Models;

internal class DrawerBoxItem {

    public int Line { get; set; }

    public int Qty { get; set; }

    public string Description { get; set; } = string.Empty;

    public string Height { get; set; } = string.Empty;

    public string Width { get; set; } = string.Empty;

    public string Depth { get; set; } = string.Empty;

}
