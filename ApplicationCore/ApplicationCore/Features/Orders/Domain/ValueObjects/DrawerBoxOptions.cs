namespace ApplicationCore.Features.Orders.Domain.ValueObjects;

public class DrawerBoxOptions {

    public DrawerBoxMaterial BoxMaterial { get; init; }
    public DrawerBoxMaterial BottomMaterial { get; }
    public string Clips { get; }
    public string Notches { get; }
    public string Accessory { get; }
    public bool Logo { get; }
    public bool PostFinish { get; }
    public bool ScoopFront { get; }
    public bool FaceMountingHoles { get; }
    public bool Assembled { get; }
    public UBoxDimensions? UBoxDimensions { get; }
    public FixedDivdersCounts? FixedDivdersCounts { get; }

    public DrawerBoxOptions(DrawerBoxMaterial boxMaterial, DrawerBoxMaterial bottomMaterial, string clips, string notches, string accessory, bool logo = false, bool postFinish = false, bool scoopFront = false, bool facemountingholes = false, bool assembled = true, UBoxDimensions? uBoxDimensions = null, FixedDivdersCounts? fixedDivdersCounts = null) {
        BoxMaterial = boxMaterial;
        BottomMaterial = bottomMaterial;
        Clips = clips;
        Notches = notches;
        Accessory = accessory;
        Logo = logo;
        PostFinish = postFinish;
        ScoopFront = scoopFront;
        FaceMountingHoles = facemountingholes;
		Assembled = assembled;
		UBoxDimensions = uBoxDimensions;
        FixedDivdersCounts = fixedDivdersCounts;
    }

}
