using Domain.Orders.Entities;
using Domain.Orders.Entities.Hardware;

namespace OrderExporting.JobSummary;

public class JobSummary {

    public string Number { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public byte[] VendorLogo { get; set; } = Array.Empty<byte>();
    public string Comment { get; set; } = string.Empty;
    public DateTime ReleaseDate { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? DueDate { get; set; }

    public string SpecialRequirements { get; set; } = string.Empty;

    public bool ShowItemsInSummary { get; set; } = false;
    public List<CabinetGroup> Cabinets { get; set; } = new();
    public List<CabinetPartGroup> CabinetParts { get; set; } = new();
    public List<ClosetPartGroup> ClosetParts { get; set; } = new();
    public List<ZargenDrawerGroup> ZargenDrawers { get; set; } = new();
    public List<DovetailDrawerBoxGroup> DovetailDrawerBoxes { get; set; } = new();
    public List<DoweledDrawerBoxGroup> DoweledDrawerBoxes { get; set; } = new();
    public List<MDFDoorGroup> MDFDoors { get; set; } = new();
    public List<FivePieceDoorGroup> FivePieceDoors { get; set; } = new();
    public List<AdditionalItem> AdditionalItems { get; set; } = new();

    public bool ContainsDovetailDBSubComponents { get; set; } = false;
    public bool ContainsMDFDoorSubComponents { get; set; } = false;
    public bool ContainsFivePieceDoorSubComponents { get; set; } = false;

    public bool ShowMaterialTypesInSummary { get; set; } = false;
    public List<string> MaterialTypes { get; set; } = new();

    public List<Supply> Supplies { get; set; } = new();

}
