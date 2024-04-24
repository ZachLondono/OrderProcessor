using ApplicationCore.Features.AllmoxyOrderExport.Attributes;
using ApplicationCore.Features.AllmoxyOrderExport.Products;
using ApplicationCore.Features.AllmoxyOrderExport.Products.FloorMountedVerticals;
using OrderLoading.ClosetProCSVCutList.Products.Verticals;

namespace ApplicationCore.Features.ClosetProToAllmoxyOrder;

public partial class ClosetProToAllmoxyMapper {

    public static IAllmoxyProduct MapVerticalPanelToAllmoxyProduct(VerticalPanel panel, bool doNotUseWallMount) {

        string bandingColor;
        if (panel.Color == panel.EdgeBandingColor) {
            bandingColor = ClosetEdgeBandingMaterial.MATCH;
        } else {
            bandingColor = ClosetEdgeBandingMaterial.GetMatchingMaterialName(panel.EdgeBandingColor);
        }

        string drilling = panel.Drilling switch {
            VerticalPanelDrilling.FinishedLeft => FinishedSide.LEFT,
            VerticalPanelDrilling.FinishedRight => FinishedSide.RIGHT,
            VerticalPanelDrilling.DrilledThrough => FinishedSide.NONE,
            _ => throw new InvalidOperationException("Unexpected vertical panel drilling type")
        };

        if (panel.WallHung && !doNotUseWallMount) {

            return new WallMountedPanels() {
                Folder = panel.Room,
                ClosetMaterial = ClosetEdgeBandingMaterial.GetMatchingMaterialName(panel.Color),
                BandingColor = bandingColor,
                RadiusedBottom = panel.HasBottomRadius,
                EdgeBandTop = false,
                ExtendForBackPanel = panel.ExtendBack,
                PanelFinish = PanelFinish.NONE,
                Qty = panel.Qty,
                PanelHeight = VerticalPanelHeight.GetNearestCompliantHeight(panel.Height).AsInches().ToString("#.000"),
                Depth = panel.Depth.AsInches(),
                FinishedSide = drilling,
                PartComment = "",
            };

        } else {

            return new FloorMountedPanels() {
                Folder = panel.Room,
                ClosetMaterial = ClosetEdgeBandingMaterial.GetMatchingMaterialName(panel.Color),
                BandingColor = bandingColor,
                BaseNotchHeight = panel.BaseNotch.Height.AsInches(),
                BaseNotchWidth = panel.BaseNotch.Depth.AsInches(),
                EdgeBandTop = false,
                ExtendForBackPanel = panel.ExtendBack,
                PanelFinish = PanelFinish.NONE,
                Qty = panel.Qty,
                PanelHeight = VerticalPanelHeight.GetNearestCompliantHeight(panel.Height).AsInches().ToString("#.000"),
                Depth = panel.Depth.AsInches(),
                FinishedSide = drilling,
                PartComment = "",
                AddLEDChannel = false,
                LEDOffFront = 0,
                LEDWidth = 0,
                LEDDepth = 0,
            };

        }

    }

    public static IAllmoxyProduct MapVerticalTransitionPanelToAllmoxyProduct(TransitionVerticalPanel panel, bool doNotUseWallMount) {

        string bandingColor;
        if (panel.Color == panel.EdgeBandingColor) {
            bandingColor = ClosetEdgeBandingMaterial.MATCH;
        } else {
            bandingColor = ClosetEdgeBandingMaterial.GetMatchingMaterialName(panel.EdgeBandingColor);
        }

        string drilling = panel.Drilling switch {
            VerticalPanelDrilling.FinishedLeft => FinishedSide.LEFT,
            VerticalPanelDrilling.FinishedRight => FinishedSide.RIGHT,
            VerticalPanelDrilling.DrilledThrough => FinishedSide.NONE,
            _ => throw new InvalidOperationException("Unexpected vertical panel drilling type")
        };

        if (panel.WallHung && !doNotUseWallMount) {

            return new WallMountedTransitionPanels() {
                Folder = panel.Room,
                ClosetMaterial = ClosetEdgeBandingMaterial.GetMatchingMaterialName(panel.Color),
                BandingColor = bandingColor,
                RadiusedBottom = panel.HasBottomRadius,
                EdgeBandTop = false,
                ExtendForBackPanel = panel.ExtendBack,
                PanelFinish = PanelFinish.NONE,
                Qty = panel.Qty,
                PanelHeight = VerticalPanelHeight.GetNearestCompliantHeight(panel.Height).AsInches().ToString("#.000"),
                Depth = panel.Depth.AsInches(),
                TransitionDepth = panel.TransitionDepth.AsInches(),
                FinishedSide = drilling,
                PartComment = "",
            };

        } else {

            return new FloorMountedTransitionPanels() {
                Folder = panel.Room,
                ClosetMaterial = ClosetEdgeBandingMaterial.GetMatchingMaterialName(panel.Color),
                BandingColor = bandingColor,
                BaseNotchHeight = panel.BaseNotch.Height.AsInches(),
                BaseNotchWidth = panel.BaseNotch.Depth.AsInches(),
                EdgeBandTop = false,
                ExtendForBackPanel = panel.ExtendBack,
                PanelFinish = PanelFinish.NONE,
                Qty = panel.Qty,
                PanelHeight = VerticalPanelHeight.GetNearestCompliantHeight(panel.Height).AsInches().ToString("#.000"),
                Depth = panel.Depth.AsInches(),
                TransitionDepth = panel.TransitionDepth.AsInches(),
                FinishedSide = drilling,
                PartComment = "",
            };

        }

    }

    public static IAllmoxyProduct MapVerticalHutchPanelToAllmoxyProduct(HutchVerticalPanel panel, bool doNotUseWallMount) {

        string bandingColor;
        if (panel.Color == panel.EdgeBandingColor) {
            bandingColor = ClosetEdgeBandingMaterial.MATCH;
        } else {
            bandingColor = ClosetEdgeBandingMaterial.GetMatchingMaterialName(panel.EdgeBandingColor);
        }

        string drilling = panel.Drilling switch {
            VerticalPanelDrilling.FinishedLeft => FinishedSide.LEFT,
            VerticalPanelDrilling.FinishedRight => FinishedSide.RIGHT,
            VerticalPanelDrilling.DrilledThrough => FinishedSide.NONE,
            _ => throw new InvalidOperationException("Unexpected vertical panel drilling type")
        };

        if (panel.WallHung && !doNotUseWallMount) {

            return new WallMountedHutchPanels() {
                Folder = panel.Room,
                ClosetMaterial = ClosetEdgeBandingMaterial.GetMatchingMaterialName(panel.Color),
                BandingColor = bandingColor,
                RadiusedBottom = panel.HasBottomRadius,
                EdgeBandTop = false,
                ExtendForBackPanel = panel.ExtendBack,
                PanelFinish = PanelFinish.NONE,
                Qty = panel.Qty,
                PanelHeight = VerticalPanelHeight.GetNearestCompliantHeight(panel.PanelHeight).AsInches().ToString("#.000"),
                Depth = panel.BottomDepth.AsInches(),
                DrawerPanelHeight = VerticalPanelHeight.GetNearestCompliantHeight(panel.BottomHeight).AsInches().ToString("#.000"),
                TopDepth = panel.TopDepth.AsInches(),
                FinishedSide = drilling,
                PartComment = "",
            };

        } else {

            return new FloorMountedHutchPanels() {
                Folder = panel.Room,
                ClosetMaterial = ClosetEdgeBandingMaterial.GetMatchingMaterialName(panel.Color),
                BandingColor = bandingColor,
                BaseNotchHeight = panel.BaseNotch.Height.AsInches(),
                BaseNotchWidth = panel.BaseNotch.Depth.AsInches(),
                EdgeBandTop = false,
                ExtendForBackPanel = panel.ExtendBack,
                PanelFinish = PanelFinish.NONE,
                Qty = panel.Qty,
                PanelHeight = VerticalPanelHeight.GetNearestCompliantHeight(panel.PanelHeight).AsInches().ToString("#.000"),
                Depth = panel.BottomDepth.AsInches(),
                DrawerPanelHeight = VerticalPanelHeight.GetNearestCompliantHeight(panel.BottomHeight).AsInches().ToString("#.000"),
                TopDepth = panel.TopDepth.AsInches(),
                FinishedSide = drilling,
                PartComment = "",
            };

        }

    }

    public static IAllmoxyProduct MapDividerPanelToAllmoxyProduct(DividerVerticalPanel panel) {

        string bandingColor;
        if (panel.Color == panel.EdgeBandingColor) {
            bandingColor = ClosetEdgeBandingMaterial.MATCH;
        } else {
            bandingColor = ClosetEdgeBandingMaterial.GetMatchingMaterialName(panel.EdgeBandingColor);
        }

        string drilling = panel.Drilling switch {
            VerticalPanelDrilling.FinishedLeft => FinishedSide.LEFT,
            VerticalPanelDrilling.FinishedRight => FinishedSide.RIGHT,
            VerticalPanelDrilling.DrilledThrough => FinishedSide.NONE,
            _ => throw new InvalidOperationException("Unexpected vertical panel drilling type")
        };

        return new DividerPanels() {
            Folder = panel.Room,
            ClosetMaterial = ClosetEdgeBandingMaterial.GetMatchingMaterialName(panel.Color),
            BandingColor = bandingColor,
            EdgeBandTop = false,
            ExtendForBackPanel = false,
            Cams = Cams.SINGLE_CAM,
            PanelFinish = PanelFinish.NONE,
            Qty = panel.Qty,
            Height = VerticalPanelHeight.GetNearestDividerCompliantHeight(panel.Height).AsInches().ToString("#.000"),
            Depth = panel.Depth.AsInches(),
            FinishedSide = drilling,
            PartComment = ""
        };

    }

}
