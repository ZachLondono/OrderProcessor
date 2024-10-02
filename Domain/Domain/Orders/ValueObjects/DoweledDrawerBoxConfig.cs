using Domain.ValueObjects;

namespace Domain.Orders.ValueObjects;

public class DoweledDrawerBoxConfig {

    public const string STANDARD_NOTCH = "Standard Notch";
    public const string NO_NOTCH = "No Notch";

    public DoweledDrawerBoxMaterial FrontMaterial { get; }
    public DoweledDrawerBoxMaterial BackMaterial { get; }
    public DoweledDrawerBoxMaterial SideMaterial { get; }
    public DoweledDrawerBoxMaterial BottomMaterial { get; }
    public bool MachineThicknessForUMSlides { get; }
    public Dimension FrontBackHeightAdjustment { get; }
    public string UMNotch { get; }

    public DoweledDrawerBoxConfig(DoweledDrawerBoxMaterial frontMaterial,
                                  DoweledDrawerBoxMaterial backMaterial,
                                  DoweledDrawerBoxMaterial sideMaterial,
                                  DoweledDrawerBoxMaterial bottomMaterial,
                                  bool machineThicknessForUMSlides,
                                  Dimension frontBackHeightAdjustment,
                                  string umNotch) {
        FrontMaterial = frontMaterial;
        BackMaterial = backMaterial;
        SideMaterial = sideMaterial;
        BottomMaterial = bottomMaterial;
        MachineThicknessForUMSlides = machineThicknessForUMSlides;
        FrontBackHeightAdjustment = frontBackHeightAdjustment;
        UMNotch = umNotch;
    }

}
