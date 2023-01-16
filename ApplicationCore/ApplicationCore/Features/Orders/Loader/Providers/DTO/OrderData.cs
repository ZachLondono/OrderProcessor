namespace ApplicationCore.Features.Orders.Loader.Providers.DTO;

public record OrderData {

    public string Number { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Comment { get; set; } = string.Empty;

    public decimal Tax { get; set; }

    public decimal Shipping { get; set; }

    public decimal PriceAdjustment { get; set; }

    public DateTime OrderDate { get; set; }

    public Guid CustomerId { get; set; }

    public Guid VendorId { get; set; }

    public bool Rush { get; set; }

    public Dictionary<string, string> Info { get; set; } = new();

    public List<DrawerBoxData> DrawerBoxes { get; set; } = new();

    public List<BaseCabinetData> BaseCabinets { get; set; } = new();

    public List<WallCabinetData> WallCabinets { get; set; } = new();

    public List<DrawerBaseCabinetData> DrawerBaseCabinets { get; set; } = new();

    public List<TallCabinetData> TallCabinets { get; set; } = new();

    public List<PieCutCornerCabinetData> PieCutCornerCabinets { get; set; } = new();

    public List<DiagonalCornerCabinetData> DiagonalCornerCabinets { get; set; } = new();

    public List<SinkCabinetData> SinkCabinets { get; set; } = new();

    public List<AdditionalItemData> AdditionalItems { get; set; } = new();

}
