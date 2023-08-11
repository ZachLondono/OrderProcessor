using ApplicationCore.Features.Orders.Shared.Domain.Enums;

namespace ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

public class DovetailDrawerBoxConfig {

    public const string FINGER_JOINT_BIRCH = "Birch FJ";
    public const string SOLID_BIRCH = "Birch CL";

    public string FrontMaterial { get; }
    public string BackMaterial { get; }
    public string SideMaterial { get; }
    public string BottomMaterial { get; }
    public string Clips { get; }
    public string Notches { get; }
    public string Accessory { get; }
    public LogoPosition Logo { get; }
    public bool PostFinish { get; }
    public bool ScoopFront { get; }
    public bool FaceMountingHoles { get; }
    public bool Assembled { get; }
    public UBoxDimensions? UBoxDimensions { get; }
    public FixedDivdersCounts? FixedDividersCounts { get; }

    public DovetailDrawerBoxConfig(string frontMaterial, string backMaterial, string sideMaterial, string bottomMaterial, string clips, string notches, string accessory, LogoPosition logo, bool postFinish = false, bool scoopFront = false, bool facemountingHoles = false, bool assembled = true, UBoxDimensions? uBoxDimensions = null, FixedDivdersCounts? fixedDivdersCounts = null) {
        FrontMaterial = frontMaterial;
        BackMaterial = backMaterial;
        SideMaterial = sideMaterial;
        BottomMaterial = bottomMaterial;
        Clips = clips;
        Notches = notches;
        Accessory = accessory;
        Logo = logo;
        PostFinish = postFinish;
        ScoopFront = scoopFront;
        FaceMountingHoles = facemountingHoles;
        Assembled = assembled;
        UBoxDimensions = uBoxDimensions;
        FixedDividersCounts = fixedDivdersCounts;
    }

    public string GetMaterialFriendlyName() {

        if (FrontMaterial == BackMaterial
            && SideMaterial == SideMaterial) {
            return FrontMaterial;
        }

        if (FrontMaterial == BackMaterial
            && FrontMaterial == FINGER_JOINT_BIRCH
            && SideMaterial == SOLID_BIRCH) {
            return "Hybrid Birch";
        }

        return $"{FrontMaterial} / {BackMaterial} / {SideMaterial}";

    }

}