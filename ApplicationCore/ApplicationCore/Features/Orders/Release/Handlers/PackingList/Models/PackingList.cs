namespace ApplicationCore.Features.Orders.Release.Handlers.PackingList.Models;

internal class PackingList {

    public DateTime Date { get; set; }

    public string OrderName { get; set; } = string.Empty;

    public string OrderNumber { get; set; } = string.Empty;

    public Company Customer { get; set; } = new();

    public Company Vendor { get; set; } = new();

    public List<DrawerBoxItem> DrawerBoxes { get; set; } = new();

    public List<DoorItem> Doors { get; set; } = new();

    public List<CabinetItem> Cabinets { get; set; } = new();

}
