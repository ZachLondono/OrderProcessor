using Domain.ValueObjects;

namespace ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

public class DoweledDrawerBoxConfig {

    public DoweledDrawerBoxMaterial FrontMaterial { get; }
    public DoweledDrawerBoxMaterial BackMaterial { get; }
    public DoweledDrawerBoxMaterial SideMaterial { get; }
    public DoweledDrawerBoxMaterial BottomMaterial { get; }
    public bool MachineThicknessForUMSlides { get; }
    public Dimension FrontBackHeightAdjustment { get; }

    public DoweledDrawerBoxConfig(DoweledDrawerBoxMaterial frontMaterial, DoweledDrawerBoxMaterial backMaterial, DoweledDrawerBoxMaterial sideMaterial, DoweledDrawerBoxMaterial bottomMaterial, bool machineThicknessForUMSlides, Dimension frontBackHeightAdjustment) {
        FrontMaterial = frontMaterial;
        BackMaterial = backMaterial;
        SideMaterial = sideMaterial;
        BottomMaterial = bottomMaterial;
        MachineThicknessForUMSlides = machineThicknessForUMSlides;
        FrontBackHeightAdjustment = frontBackHeightAdjustment;
    }

}
