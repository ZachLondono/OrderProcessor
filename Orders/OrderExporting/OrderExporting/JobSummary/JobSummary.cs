using Domain.Orders.Entities;

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
    public List<CabinetGroup> Cabinets { get; set; } = [];
    public List<CabinetPartGroup> CabinetParts { get; set; } = [];
    public List<ClosetPartGroup> ClosetParts { get; set; } = [];
    public List<ZargenDrawerGroup> ZargenDrawers { get; set; } = [];
    public List<DovetailDrawerBoxGroup> DovetailDrawerBoxes { get; set; } = [];
    public List<DoweledDrawerBoxGroup> DoweledDrawerBoxes { get; set; } = [];
    public List<MDFDoorGroup> MDFDoors { get; set; } = [];
    public List<FivePieceDoorGroup> FivePieceDoors { get; set; } = [];
    public List<CounterTopItem> CounterTops { get; set; } = [];
    public List<AdditionalItem> AdditionalItems { get; set; } = [];

    public bool ContainsDovetailDBSubComponents { get; set; } = false;
    public bool ContainsMDFDoorSubComponents { get; set; } = false;
    public bool ContainsFivePieceDoorSubComponents { get; set; } = false;

    public bool ShowMaterialTypesInSummary { get; set; } = false;
    public List<string> MaterialTypes { get; set; } = new();

}
