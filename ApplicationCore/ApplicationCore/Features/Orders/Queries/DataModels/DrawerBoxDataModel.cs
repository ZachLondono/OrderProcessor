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

    public bool PostFinish { get; set; }

    public bool ScoopFront { get; set; }

    public bool Logo { get; set; }

    public bool FaceMountingHoles { get; set; }

    public UBoxDimensions? UBoxDimensions { get; set; } = default;

    public FixedDivdersCounts? FixedDividers { get; set; } = default;

    public Guid BoxMaterialId { get; set; }

    public string BoxMaterialName { get; set; } = string.Empty;

    public Dimension BoxMaterialThickness { get; set; } = Dimension.FromMillimeters(0);

    public Guid BottomMaterialId { get; set; }

    public string BottomMaterialName { get; set; } = string.Empty;

    public Dimension BottomMaterialThickness { get; set; } = Dimension.FromMillimeters(0);

    public Guid ClipsId { get; set; }

    public string ClipsName { get; set; } = string.Empty;

    public Guid NotchesId { get; set; }

    public string NotchesName { get; set; } = string.Empty;

    public Guid AccessoryId { get; set; }

    public string AccessoryName { get; set; } = string.Empty;

    public DrawerBox AsDomainModel() {

        var boxMaterial = new DrawerBoxMaterial(BoxMaterialId, BoxMaterialName, BoxMaterialThickness);
        var bottomMaterial = new DrawerBoxMaterial(BottomMaterialId, BottomMaterialName, BottomMaterialThickness);
        var clips = new DrawerBoxOption(ClipsId, ClipsName);
        var notches = new DrawerBoxOption(NotchesId, NotchesName);
        var accessory = new DrawerBoxOption(AccessoryId, AccessoryName);

        return new DrawerBox(Id, LineInOrder, UnitPrice, Qty, Height, Width, Depth, Note, new(boxMaterial, bottomMaterial, clips, notches, accessory, Logo, PostFinish, ScoopFront, FaceMountingHoles, UBoxDimensions, FixedDividers));

    }

}
