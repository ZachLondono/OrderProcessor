using ApplicationCore.Features.Orders.Shared.Domain.Enums;

namespace ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

public record CabinetDrawerBoxOptions(CabinetDrawerBoxMaterial Material, DrawerSlideType SlideType) {

    public DrawerBoxOptions GetDrawerBoxOptions() {

        string materialName = Material switch {
            CabinetDrawerBoxMaterial.FingerJointBirch => DrawerBoxOptions.FINGER_JOINT_BIRCH,
            CabinetDrawerBoxMaterial.SolidBirch => DrawerBoxOptions.SOLID_BIRCH,
            _ => "UNKNOWN"
        };

        return new(materialName, materialName, materialName, "1/4\" Ply", "Blum", GetNotchFromSlideType(SlideType), "None", LogoPosition.None);
    }

    private static string GetNotchFromSlideType(DrawerSlideType slide) => slide switch {
        DrawerSlideType.UnderMount => "Standard Notch",
        DrawerSlideType.SideMount => "No Notch",
        _ => ""
    };

}