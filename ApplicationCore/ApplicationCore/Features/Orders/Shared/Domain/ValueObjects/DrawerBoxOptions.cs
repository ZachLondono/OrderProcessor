namespace ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

public class DrawerBoxOptions {

    public Guid BoxMaterialId { get; }
    public Guid BottomMaterialId { get; }
    public string Clips { get; }
    public string Notches { get; }
    public string Accessory { get; }
    public LogoPosition Logo { get; }
    public bool PostFinish { get; }
    public bool ScoopFront { get; }
    public bool FaceMountingHoles { get; }
    public bool Assembled { get; }
    public UBoxDimensions? UBoxDimensions { get; }
    public FixedDivdersCounts? FixedDivdersCounts { get; }

    public DrawerBoxOptions(Guid boxMaterialId, Guid bottomMaterialId, string clips, string notches, string accessory, LogoPosition logo, bool postFinish = false, bool scoopFront = false, bool facemountingholes = false, bool assembled = true, UBoxDimensions? uBoxDimensions = null, FixedDivdersCounts? fixedDivdersCounts = null) {
        BoxMaterialId = boxMaterialId;
        BottomMaterialId = bottomMaterialId;
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
