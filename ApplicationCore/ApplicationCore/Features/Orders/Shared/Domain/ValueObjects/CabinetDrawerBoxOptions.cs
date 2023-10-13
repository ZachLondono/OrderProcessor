using ApplicationCore.Features.Orders.Shared.Domain.Enums;

namespace ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

public record CabinetDrawerBoxOptions(CabinetDrawerBoxMaterial Material, DrawerSlideType SlideType) {

    public DovetailDrawerBoxConfig GetDrawerBoxOptions() {

        string materialName = Material switch {
            CabinetDrawerBoxMaterial.FingerJointBirch => DovetailDrawerBoxConfig.FINGER_JOINT_BIRCH,
            CabinetDrawerBoxMaterial.SolidBirch => DovetailDrawerBoxConfig.SOLID_BIRCH,
            _ => "UNKNOWN"
        };

        return new(materialName, materialName, materialName, "1/4\" Ply", "Blum", GetNotchFromSlideType(SlideType), "None", LogoPosition.None);
    }

    private static string GetNotchFromSlideType(DrawerSlideType slide) => slide switch {
        DrawerSlideType.UnderMount => "Std_Notch",
        DrawerSlideType.SideMount => "No_Notch",
        _ => ""
    };

}