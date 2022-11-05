using ApplicationCore.Features.Orders.Domain.ValueObjects;

namespace ApplicationCore.Features.Orders.Loader.Providers.DTO;

public record DrawerBoxData {

    public int Line { get; set; }

    public decimal UnitPrice { get; set; }

    public int Qty { get; set; }

    public Dimension Height { get; set; } = Dimension.FromMillimeters(0);

    public Dimension Width { get; set; } = Dimension.FromMillimeters(0);

    public Dimension Depth { get; set; } = Dimension.FromMillimeters(0);

    public bool Logo { get; set; }

    public bool PostFinish { get; set; }

    public bool ScoopFront { get; set; }

    public bool FaceMountingHoles { get; set; }

    public bool UBox { get; set; }
    public Dimension UBoxA { get; set; } = Dimension.FromMillimeters(0);
    public Dimension UBoxB { get; set; } = Dimension.FromMillimeters(0);
    public Dimension UBoxC { get; set; } = Dimension.FromMillimeters(0);

    public bool FixedDividers { get; set; }
    public int DividersWide { get; set; }
    public int DividersDeep { get; set; }

    public Guid BoxMaterialOptionId { get; set; }
    public Guid BottomMaterialOptionId { get; set; }
    public Guid ClipsOptionId { get; set; }
    public Guid NotchOptionId { get; set; }
    public Guid InsertOptionId { get; set; }

}
