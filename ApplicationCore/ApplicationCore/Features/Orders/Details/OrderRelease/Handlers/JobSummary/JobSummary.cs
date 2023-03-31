using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

namespace ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.JobSummary;

internal class JobSummary {

    public string Number { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public byte[] VendorLogo { get; set; } = Array.Empty<byte>();
    public string Comment { get; set; } = string.Empty;
    public DateTime ReleaseDate { get; set; }
    public List<Supply> Supplies { get; set; } = new();

    public string SpecialRequirements { get; set; } = string.Empty;

    public decimal SubTotal { get; set; }
    public decimal Shipping { get; set; }
    public decimal SalesTax { get; set; }
    public decimal Total { get; set; }

    public bool ShowItemsInSummary { get; set; } = false;
    public List<CabinetGroup> Cabients { get; set; } = new();
    public List<ClosetPartGroup> ClosetParts { get; set; } = new();
    public List<DrawerBoxGroup> DrawerBoxes { get; set; } = new();
    public List<DoorGroup> Doors { get; set; } = new();

}
