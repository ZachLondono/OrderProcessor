using ApplicationCore.Features.Orders.Shared.Domain.Enums;

namespace ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

public class DrawerBoxOptions {

    public string FrontMaterial { get; }
    public string BackMaterial { get; }
    public string SideMaterial { get; }
    public string BottomMaterial { get; }
    public string Clips { get; }
    public string Notches { get; }
    public string Accessory { get; }
    public DrawerSlideType SlideType { get; }
    public LogoPosition Logo { get; }
    public bool PostFinish { get; }
    public bool ScoopFront { get; }
    public bool FaceMountingHoles { get; }
    public bool Assembled { get; }
    public UBoxDimensions? UBoxDimensions { get; }
    public FixedDivdersCounts? FixedDivdersCounts { get; }

    public DrawerBoxOptions(string frontMaterial, string backMaterial, string sideMaterial, string bottomMaterial, string clips, string notches, DrawerSlideType slideType, string accessory, LogoPosition logo, bool postFinish = false, bool scoopFront = false, bool facemountingholes = false, bool assembled = true, UBoxDimensions? uBoxDimensions = null, FixedDivdersCounts? fixedDivdersCounts = null) {
        FrontMaterial = frontMaterial;
        BackMaterial = backMaterial;
        SideMaterial = sideMaterial;
        BottomMaterial = bottomMaterial;
        Clips = clips;
        Notches = notches;
        SlideType = slideType;
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
