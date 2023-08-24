namespace ApplicationCore.Features.Orders.OrderRelease.Handlers.PackingList;

internal record PackingList {

    public DateTime Date { get; set; }

    public string OrderName { get; set; } = string.Empty;

    public string OrderNumber { get; set; } = string.Empty;

    public Company Customer { get; set; } = new();

    public Company Vendor { get; set; } = new();

    public List<DovetailDovetailDrawerBoxItem> DovetailDrawerBoxes { get; set; } = new();

    public List<DoweledDrawerBoxItem> DoweledDrawerBoxes { get; set; } = new();

    public List<MDFDoorItem> Doors { get; set; } = new();

    public List<CabinetItem> Cabinets { get; set; } = new();

    public List<ClosetPartItem> ClosetParts { get; set; } = new();

    public List<AdditionalItem> AdditionalItems { get; set; } = new();

}

