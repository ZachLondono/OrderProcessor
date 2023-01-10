using ApplicationCore.Features.Orders.Domain.Products;
using ApplicationCore.Features.Orders.Domain.ValueObjects;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.Queries.DataModels;

public class DrawerBoxDataModel {

    public Guid Id { get; set; }

    public int LineInOrder { get; set; }

    public decimal UnitPrice { get; set; }

    public int Qty { get; set; }

    public string Note { get; set; } = string.Empty;

    public Dimension Height { get; set; } = Dimension.FromMillimeters(0);

    public Dimension Width { get; set; } = Dimension.FromMillimeters(0);

    public Dimension Depth { get; set; } = Dimension.FromMillimeters(0);

    public IDictionary<string, string> LabelFields { get; set; } = new Dictionary<string, string>();

    public bool PostFinish { get; set; }

    public bool ScoopFront { get; set; }

    public LogoPosition Logo { get; set; }

    public bool FaceMountingHoles { get; set; }

    public bool Assembled { get; set; }

    public UBoxDimensions? UBoxDimensions { get; set; } = default;

    public FixedDivdersCounts? FixedDividers { get; set; } = default;

    public Guid BoxMaterialId { get; set; }

    public Guid BottomMaterialId { get; set; }

    public string Clips { get; set; } = string.Empty;

	public string ClipsName { get; set; } = string.Empty;

    public string Notches { get; set; } = string.Empty;

	public string NotchesName { get; set; } = string.Empty;

    public string Accessory { get; set; } = string.Empty;

    public string AccessoryName { get; set; } = string.Empty;

    public DovetailDrawerBoxProduct AsDomainModel() {

        return new DovetailDrawerBoxProduct(Id, LineInOrder, UnitPrice, Qty, Height, Width, Depth, Note, LabelFields.AsReadOnly(), new(BoxMaterialId, BottomMaterialId, Clips, Notches, Accessory, Logo, PostFinish, ScoopFront, FaceMountingHoles, Assembled, UBoxDimensions, FixedDividers));

    }

}
