using ApplicationCore.Features.Orders.Shared.Domain.Enums;

namespace ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

public record CabinetDrawerBoxOptions(CabinetDrawerBoxMaterial Material, DrawerSlideType SlideType) {

    public DrawerBoxOptions DrawerBoxOptions => new("", "", "", "", "Blum", GetNotchFromSlideType(SlideType), "", LogoPosition.None);

    private static string GetNotchFromSlideType(DrawerSlideType slide) => slide switch {
        DrawerSlideType.UnderMount => "Standard Notch",
        DrawerSlideType.SideMount => "",
        _ => ""
    };

}