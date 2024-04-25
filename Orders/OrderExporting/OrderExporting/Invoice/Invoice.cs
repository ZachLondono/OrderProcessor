namespace OrderExporting.Invoice;

public record Invoice {

    public DateTime Date { get; set; }

    public string OrderName { get; set; } = string.Empty;

    public string OrderNumber { get; set; } = string.Empty;

    public string Terms { get; set; } = string.Empty;

    public decimal SubTotal { get; set; }

    public decimal Discount { get; set; }

    public decimal SalesTax { get; set; }

    public decimal Shipping { get; set; }

    public decimal Total { get; set; }

    public Company Customer { get; set; } = new();

    public Company Vendor { get; set; } = new();

    public List<DovetailDrawerBoxItem> DovetailDrawerBoxes { get; set; } = new();

    public List<DoweledDrawerBoxItem> DoweledDrawerBoxes { get; set; } = new();

    public List<MDFDoorItem> MDFDoors { get; set; } = new();

    public List<FivePieceDoorItem> FivePieceDoors { get; set; } = new();

    public List<CabinetItem> Cabinets { get; set; } = new();

    public List<CabinetPartItem> CabinetParts { get; set; } = new();

    public List<ClosetPartItem> ClosetParts { get; set; } = new();

    public List<ZargenDrawerItem> ZargenDrawers { get; set; } = new();

    public List<CounterTopItem> CounterTops { get; set; } = new();

    public List<AdditionalItem> AdditionalItems { get; set; } = new();

}
