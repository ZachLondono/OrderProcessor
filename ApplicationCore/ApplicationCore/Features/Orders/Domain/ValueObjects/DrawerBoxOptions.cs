namespace ApplicationCore.Features.Orders.Domain.ValueObjects;

public class DrawerBoxOptions {

    public DrawerBoxMaterial BoxMaterial { get; init; }
    public DrawerBoxMaterial BottomMaterial { get; }
    public DrawerBoxOption Clips { get; }
    public DrawerBoxOption Notches { get; }
    public DrawerBoxOption Accessory { get; }
    public bool Logo { get; }
    public bool PostFinish { get; }
    public bool ScoopFront { get; }
    public bool FaceMountingHoles { get; }
    public UBoxDimensions? UBoxDimensions { get; }
    public FixedDivdersCounts? FixedDivdersCounts { get; }

    public DrawerBoxOptions(DrawerBoxMaterial boxMaterial, DrawerBoxMaterial bottomMaterial, DrawerBoxOption clips, DrawerBoxOption notches, DrawerBoxOption accessory, bool logo = false, bool postFinish = false, bool scoopFront = false, bool facemountingholes = false, UBoxDimensions? uBoxDimensions = null, FixedDivdersCounts? fixedDivdersCounts = null) {
        BoxMaterial = boxMaterial;
        BottomMaterial = bottomMaterial;
        Clips = clips;
        Notches = notches;
        Accessory = accessory;
        Logo = logo;
        PostFinish = postFinish;
        ScoopFront = scoopFront;
        FaceMountingHoles = facemountingholes;
        UBoxDimensions = uBoxDimensions;
        FixedDivdersCounts = fixedDivdersCounts;
    }

}
