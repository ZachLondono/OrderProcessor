using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.AllmoxyOrderExport.Attributes;

public static class VerticalPanelHeight {

    public static Dimension GetNearestCompliantHeight(Dimension height) {

        var mult = Math.Round((height - Dimension.FromMillimeters(19)).AsMillimeters() / 32);

        return Dimension.FromMillimeters(19 + 32 * mult);

    }

    public static Dimension GetNearestDividerCompliantHeight(Dimension height) {

        var mult = Math.Round((height - Dimension.FromMillimeters(45)).AsMillimeters() / 32);

        return Dimension.FromMillimeters(45 + 32 * mult);

    }

}
