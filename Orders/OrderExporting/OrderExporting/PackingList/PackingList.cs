namespace OrderExporting.PackingList;

public record PackingList {

    public DateTime Date { get; set; }

    public string OrderName { get; set; } = string.Empty;

    public string OrderNumber { get; set; } = string.Empty;

    public Company Customer { get; set; } = new();

    public Company Vendor { get; set; } = new();

    public List<DovetailDovetailDrawerBoxItem> DovetailDrawerBoxes { get; set; } = [];

    public List<DoweledDrawerBoxItem> DoweledDrawerBoxes { get; set; } = [];

    public List<MDFDoorItem> MDFDoors { get; set; } = [];

    public List<FivePieceDoorItem> FivePieceDoors { get; set; } = [];

    public List<CabinetItem> Cabinets { get; set; } = [];

    public List<CabinetPartItem> CabinetParts { get; set; } = [];

    public List<ClosetPartItem> ClosetParts { get; set; } = [];

    public List<ZargenDrawerItem> ZargenDrawers { get; set; } = [];

    public List<AdditionalItem> AdditionalItems { get; set; } = [];

    public bool IncludeCheckBoxesNextToItems { get; set; } = false;

    public bool IncludeSignatureField { get; set; } = false;

}

