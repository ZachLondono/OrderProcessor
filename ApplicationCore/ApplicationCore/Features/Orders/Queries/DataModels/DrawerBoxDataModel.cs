using ApplicationCore.Features.Orders.Domain;
using ApplicationCore.Features.Orders.Domain.ValueObjects;

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

    public Dictionary<string, string> LabelFields { get; set; } = new();

    public bool PostFinish { get; set; }

    public bool ScoopFront { get; set; }

    public bool Logo { get; set; }

    public bool FaceMountingHoles { get; set; }

    public bool Assembled { get; set; }

    public UBoxDimensions? UBoxDimensions { get; set; } = default;

    public FixedDivdersCounts? FixedDividers { get; set; } = default;

    public Guid BoxMaterialId { get; set; }

    public string BoxMaterialName { get; set; } = string.Empty;

    public Dimension BoxMaterialThickness { get; set; } = Dimension.FromMillimeters(0);

    public Guid BottomMaterialId { get; set; }

    public string BottomMaterialName { get; set; } = string.Empty;

    public Dimension BottomMaterialThickness { get; set; } = Dimension.FromMillimeters(0);

    public string Clips { get; set; } = string.Empty;

	public string ClipsName { get; set; } = string.Empty;

    public string Notches { get; set; } = string.Empty;

	public string NotchesName { get; set; } = string.Empty;

    public string Accessory { get; set; } = string.Empty;

    public string AccessoryName { get; set; } = string.Empty;

    public DrawerBox AsDomainModel() {

        var boxMaterial = new DrawerBoxMaterial(BoxMaterialId, BoxMaterialName, BoxMaterialThickness);
        var bottomMaterial = new DrawerBoxMaterial(BottomMaterialId, BottomMaterialName, BottomMaterialThickness);
        
        return new DrawerBox(Id, LineInOrder, UnitPrice, Qty, Height, Width, Depth, Note, LabelFields, new(boxMaterial, bottomMaterial, Clips, Notches, Accessory, Logo, PostFinish, ScoopFront, FaceMountingHoles, Assembled, UBoxDimensions, FixedDividers));

    }

}
