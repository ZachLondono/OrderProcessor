using ApplicationCore.Features.AllmoxyOrderExport.Attributes;
using ApplicationCore.Features.AllmoxyOrderExport.Products;
using ApplicationCore.Features.AllmoxyOrderExport.Products.Miscellaneous;
using OrderLoading.ClosetProCSVCutList.Products;

namespace ApplicationCore.Features.ClosetProToAllmoxyOrder;

public partial class ClosetProToAllmoxyMapper {

    public static IAllmoxyProduct MapMiscellaneousToAllmoxyProduct(MiscellaneousClosetPart part) {

        return part.Type switch {
            MiscellaneousType.Backing => MapPartBack(part),
            MiscellaneousType.Top => MapPartTop(part),
            MiscellaneousType.ToeKick => MapPartToeKick(part),
            MiscellaneousType.Filler or
            MiscellaneousType.ExtraPanel or
            MiscellaneousType.Cleat => MapPartNailer(part),
            _ => throw new NotImplementedException("Unexpected miscellaneous closet part")
        };

    }

    public static IAllmoxyProduct MapPartToeKick(MiscellaneousClosetPart part) {

        string bandingColor;
        if (part.Color == part.EdgeBandingColor) {
            bandingColor = ClosetEdgeBandingMaterial.MATCH;
        } else {
            bandingColor = ClosetEdgeBandingMaterial.GetMatchingMaterialName(part.EdgeBandingColor);
        }

        return new ToeKick() {
            Folder = part.Room,
            ClosetMaterial = ClosetEdgeBandingMaterial.GetMatchingMaterialName(part.Color),
            BandingColor = bandingColor,
            Cams = Cams.SINGLE_CAM,
            PanelFinish = PanelFinish.NONE,
            Qty = part.Qty,
            Width = part.Width.AsInches(),
            Length = part.Length.AsInches(),
            PartComment = string.Empty
        };

    }

    public static IAllmoxyProduct MapPartTop(MiscellaneousClosetPart part) {

        string bandingColor;
        if (part.Color == part.EdgeBandingColor) {
            bandingColor = ClosetEdgeBandingMaterial.MATCH;
        } else {
            bandingColor = ClosetEdgeBandingMaterial.GetMatchingMaterialName(part.EdgeBandingColor);
        }

        return new Top() {
            Folder = part.Room,
            ClosetMaterial = ClosetEdgeBandingMaterial.GetMatchingMaterialName(part.Color),
            BandingColor = bandingColor,
            PanelFinish = PanelFinish.NONE,
            Qty = part.Qty,
            Width = part.Width.AsInches(),
            Depth = part.Length.AsInches(),
            PartComment = string.Empty,
            RadiusedCorners = RadiusedCorners.NONE
        };

    }

    public static IAllmoxyProduct MapPartBack(MiscellaneousClosetPart part) {

        string bandingColor;
        if (part.Color == part.EdgeBandingColor) {
            bandingColor = ClosetEdgeBandingMaterial.MATCH;
        } else {
            bandingColor = ClosetEdgeBandingMaterial.GetMatchingMaterialName(part.EdgeBandingColor);
        }

        return new Back() {
            Folder = part.Room,
            ClosetMaterial = ClosetEdgeBandingMaterial.GetMatchingMaterialName(part.Color),
            PanelFinish = PanelFinish.NONE,
            Qty = part.Qty,
            Width = part.Width.AsInches(),
            Height = part.Length.AsInches(),
            PartComment = string.Empty,
        };

    }

    public static IAllmoxyProduct MapPartNailer(MiscellaneousClosetPart part) {

        string bandingColor;
        if (part.Color == part.EdgeBandingColor) {
            bandingColor = ClosetEdgeBandingMaterial.MATCH;
        } else {
            bandingColor = ClosetEdgeBandingMaterial.GetMatchingMaterialName(part.EdgeBandingColor);
        }

        return new Nailer() {
            Folder = part.Room,
            ClosetMaterial = ClosetEdgeBandingMaterial.GetMatchingMaterialName(part.Color),
            BandingColor = bandingColor,
            Banding = EdgeBanding.ALL_EDGES,
            PanelFinish = PanelFinish.NONE,
            Qty = part.Qty,
            Width = part.Width.AsInches(),
            Length = part.Length.AsInches(),
            PartComment = string.Empty,
        };

    }

}
