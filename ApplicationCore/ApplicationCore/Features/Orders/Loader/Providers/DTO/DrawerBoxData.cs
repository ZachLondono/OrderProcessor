using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.Loader.Providers.DTO;

public record DrawerBoxData {

    public int Line { get; set; }

    public decimal UnitPrice { get; set; }

    public int Qty { get; set; }

    public Dimension Height { get; set; } = Dimension.FromMillimeters(0);

    public Dimension Width { get; set; } = Dimension.FromMillimeters(0);

    public Dimension Depth { get; set; } = Dimension.FromMillimeters(0);

    public string Note { get; set; } = string.Empty;

    public Dictionary<string, string> LabelFields { get; set; } = new();

    public LogoPosition Logo { get; set; }

    public bool PostFinish { get; set; }

    public bool ScoopFront { get; set; }

    public bool FaceMountingHoles { get; set; }

    public bool Assembled { get; set; }

    public bool UBox { get; set; }
    public Dimension UBoxA { get; set; } = Dimension.FromMillimeters(0);
    public Dimension UBoxB { get; set; } = Dimension.FromMillimeters(0);
    public Dimension UBoxC { get; set; } = Dimension.FromMillimeters(0);

    public bool FixedDividers { get; set; }
    public int DividersWide { get; set; }
    public int DividersDeep { get; set; }

    public Guid BoxMaterialOptionId { get; set; }
    public Guid BottomMaterialOptionId { get; set; }

    public string Clips { get; set; } = string.Empty;
    public string Notch { get; set; } = string.Empty;
    public string Accessory { get; set; } = string.Empty;

}
