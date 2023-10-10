using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

namespace ApplicationCore.Features.Orders.OrderRelease.Handlers.JobSummary;

internal class JobSummary {

    public string Number { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public byte[] VendorLogo { get; set; } = Array.Empty<byte>();
    public string Comment { get; set; } = string.Empty;
    public DateTime ReleaseDate { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? DueDate { get; set; }

    public string SpecialRequirements { get; set; } = string.Empty;

    public bool ShowInvoiceSummary { get; set; } = false;
    public decimal SubTotal { get; set; }
    public decimal Shipping { get; set; }
    public decimal SalesTax { get; set; }
    public decimal Total { get; set; }

    public bool ShowItemsInSummary { get; set; } = false;
    public List<CabinetGroup> Cabinets { get; set; } = new();
    public List<CabinetPartGroup> CabinetParts { get; set; } = new();
    public List<ClosetPartGroup> ClosetParts { get; set; } = new();
    public List<ZargenDrawerGroup> ZargenDrawers { get; set; } = new();
    public List<DovetailDrawerBoxGroup> DovetailDrawerBoxes { get; set; } = new();
    public List<DoweledDrawerBoxGroup> DoweledDrawerBoxes { get; set; } = new();
    public List<MDFDoorGroup> Doors { get; set; } = new();
    public int AdditionalItems { get; set; } = new();

    public List<Supply> Supplies { get; set; } = new();

}
