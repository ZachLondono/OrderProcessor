using ApplicationCore.Features.AllmoxyOrderExport.Attributes;
using ApplicationCore.Features.AllmoxyOrderExport.Products;
using ApplicationCore.Features.AllmoxyOrderExport.Products.Shelves;
using ApplicationCore.Features.ClosetProCSVCutList.Products;

namespace ApplicationCore.Features.ClosetProToAllmoxyOrder;

public partial class ClosetProToAllmoxyMapper {

    public static IAllmoxyProduct MapShelfToAllmoxyProduct(Shelf shelf) {

        return shelf.Type switch {
            ShelfType.Adjustable => MapPartToAdjustableShelf(shelf),
            ShelfType.Shoe => MapPartToShoeShelf(shelf),
            ShelfType.Fixed => MapPartToFixedShelf(shelf),
            _ => throw new InvalidOperationException("Unexpected shelf product type")
        };

    }

    public static IAllmoxyProduct MapPartToAdjustableShelf(Shelf shelf) {

        string bandingColor;
        if (shelf.Color == shelf.EdgeBandingColor) {
            bandingColor = ClosetEdgeBandingMaterial.MATCH;
        } else {
            bandingColor = ClosetEdgeBandingMaterial.GetMatchingMaterialName(shelf.EdgeBandingColor);
        }

        return new AdjustableShelf() {
            Folder = shelf.Room,
            ClosetMaterial = ClosetMaterials.GetMatchingMaterialName(shelf.Color),
            BandingColor = bandingColor,
            PanelFinish = PanelFinish.NONE,
            EdgeBandFrontAndBack = false,
            Qty = shelf.Qty,
            Width = shelf.Width.AsInches(),
            Depth = shelf.Depth.AsInches(),
            PartComment = string.Empty,
            AddLEDChannel = false,
            LEDOffFront = 0,
            LEDWidth = 0,
            LEDDepth = 0,
        };

    }

    public static IAllmoxyProduct MapPartToAdjustableSafetyShelf(Shelf shelf) {

        string bandingColor;
        if (shelf.Color == shelf.EdgeBandingColor) {
            bandingColor = ClosetEdgeBandingMaterial.MATCH;
        } else {
            bandingColor = ClosetEdgeBandingMaterial.GetMatchingMaterialName(shelf.EdgeBandingColor);
        }

        return new AdjustableSafetyShelf() {
            Folder = shelf.Room,
            ClosetMaterial = ClosetMaterials.GetMatchingMaterialName(shelf.Color),
            BandingColor = bandingColor,
            PanelFinish = PanelFinish.NONE,
            EdgeBandFrontAndBack = false,
            Qty = shelf.Qty,
            Width = shelf.Width.AsInches(),
            Depth = shelf.Depth.AsInches(),
            Recess = shelf.ExtendBack ? Recess.EXTENDED : Recess.FLUSH,
            DrawerLock = false,
            PartComment = string.Empty,
            AddLEDChannel = false,
            LEDOffFront = 0,
            LEDWidth = 0,
            LEDDepth = 0,
        };

    }

    public static IAllmoxyProduct MapPartToFixedShelf(Shelf shelf) {

        string bandingColor;
        if (shelf.Color == shelf.EdgeBandingColor) {
            bandingColor = ClosetEdgeBandingMaterial.MATCH;
        } else {
            bandingColor = ClosetEdgeBandingMaterial.GetMatchingMaterialName(shelf.EdgeBandingColor);
        }

        return new FixedShelf() {
            Folder = shelf.Room,
            ClosetMaterial = ClosetMaterials.GetMatchingMaterialName(shelf.Color),
            BandingColor = bandingColor,
            PanelFinish = PanelFinish.NONE,
            DoubleCams = false,
            EdgeBandFrontAndBack = false,
            Qty = shelf.Qty,
            Width = shelf.Width.AsInches(),
            Depth = shelf.Depth.AsInches(),
            Recess = shelf.ExtendBack ? Recess.EXTENDED : Recess.FLUSH,
            DrawerLock = false,
            PartComment = string.Empty,
            AddLEDChannel = false,
            LEDOffFront = 0,
            LEDWidth = 0,
            LEDDepth = 0,
        };

    } 

    public static IAllmoxyProduct MapPartToShoeShelf(Shelf shelf) {

        string bandingColor;
        if (shelf.Color == shelf.EdgeBandingColor) {
            bandingColor = ClosetEdgeBandingMaterial.MATCH;
        } else {
            bandingColor = ClosetEdgeBandingMaterial.GetMatchingMaterialName(shelf.EdgeBandingColor);
        }

        string panelDepth = shelf.Depth.AsInches() switch {
            12 => ShoeShelfDepth.TWELVE_INCH,
            14 => ShoeShelfDepth.FOURTEEN_INCH,
            16 => ShoeShelfDepth.SIXTEEN_INCH,
            _ => throw new InvalidOperationException("Unsupported shoe shelf panel depth")
        };

        return new ShoeShelf() {
            Folder = shelf.Room,
            ClosetMaterial = ClosetMaterials.GetMatchingMaterialName(shelf.Color),
            BandingColor = bandingColor,
            PanelFinish = PanelFinish.NONE,
            Qty = shelf.Qty,
            Width = shelf.Width.AsInches(),
            PanelDepth = panelDepth,
            PartComment = string.Empty,
        };

    }

    public static IAllmoxyProduct MapDividerShelfPartToAllmoxyProduct(ClosetProCSVCutList.Products.DividerShelf shelf) {

        string bandingColor;
        if (shelf.Color == shelf.EdgeBandingColor) {
            bandingColor = ClosetEdgeBandingMaterial.MATCH;
        } else {
            bandingColor = ClosetEdgeBandingMaterial.GetMatchingMaterialName(shelf.EdgeBandingColor);
        }

        string divisions = shelf.DividerCount switch {
            1 => Divisions.TWO_EQUAL_SECTIONS,
            2 => Divisions.THREE_EQUAL_SECTIONS,
            3 => Divisions.FOUR_EQUAL_SECTIONS,
            4 => Divisions.FIVE_EQUAL_SECTIONS,
            _ => throw new InvalidOperationException("Unsupported number of divider sections")
        };

        string topOrBottom = shelf.Type switch {
            DividerShelfType.Top => TopBottom.TOP,
            DividerShelfType.Bottom => TopBottom.BOTTOM,
            _ => throw new InvalidOperationException("Unexpected divider shelf type")
        };

        return new AllmoxyOrderExport.Products.Shelves.DividerShelf() {
            Folder = shelf.Room,
            ClosetMaterial = ClosetMaterials.GetMatchingMaterialName(shelf.Color),
            BandingColor = bandingColor,
            PanelFinish = PanelFinish.NONE,
            Qty = shelf.Qty,
            Width = shelf.Width.AsInches(),
            Depth = shelf.Depth.AsInches(),
            PartComment = string.Empty,
            Divisions = divisions,
            EdgeDrilling = EndDrilling.SINGLE_CAM,
            TopOrBottom = topOrBottom,
            Opening1 = 0,
            Opening2 = 0,
            Opening3 = 0,
            Opening4 = 0,
        };

    }

    public static IAllmoxyProduct MapCornerShelfToAllmoxyProduct(CornerShelf shelf) {

        return shelf.Type switch {
            CornerShelfType.LAdjustable => MapPartToAdjustableLShelf(shelf),
            CornerShelfType.LFixed => MapPartToFixedLShelf(shelf),
            CornerShelfType.DiagonalAdjustable => MapPartToAdjustableAngledShelf(shelf),
            CornerShelfType.DiagonalFixed => MapPartToFixedAngledShelf(shelf),
            _ => throw new InvalidOperationException("Unexpected corner shelf product type")
        };

    }

    public static IAllmoxyProduct MapPartToAdjustableLShelf(CornerShelf shelf) {

        string bandingColor;
        if (shelf.Color == shelf.EdgeBandingColor) {
            bandingColor = ClosetEdgeBandingMaterial.MATCH;
        } else {
            bandingColor = ClosetEdgeBandingMaterial.GetMatchingMaterialName(shelf.EdgeBandingColor);
        }

        return new AdjustableLShapedShelf() {
            Folder = shelf.Room,
            ClosetMaterial = ClosetMaterials.GetMatchingMaterialName(shelf.Color),
            BandingColor = bandingColor,
            PanelFinish = PanelFinish.NONE,
            Qty = shelf.Qty,
            PartComment = string.Empty,
            A = shelf.NotchSideLength.AsInches(),
            B = shelf.ProductWidth.AsInches(),
            C = shelf.RightWidth.AsInches(),
            D = shelf.ProductLength.AsInches(),
            Notch = NotchDepth.NINETEEN,
            NotchSide = NotchSide.LEFT,
            Radius = 2,
        };

    }

    public static IAllmoxyProduct MapPartToFixedLShelf(CornerShelf shelf) {

        string bandingColor;
        if (shelf.Color == shelf.EdgeBandingColor) {
            bandingColor = ClosetEdgeBandingMaterial.MATCH;
        } else {
            bandingColor = ClosetEdgeBandingMaterial.GetMatchingMaterialName(shelf.EdgeBandingColor);
        }

        return new FixedLShapedShelf() {
            Folder = shelf.Room,
            ClosetMaterial = ClosetMaterials.GetMatchingMaterialName(shelf.Color),
            BandingColor = bandingColor,
            PanelFinish = PanelFinish.NONE,
            Qty = shelf.Qty,
            PartComment = string.Empty,
            A = shelf.NotchSideLength.AsInches(),
            B = shelf.ProductWidth.AsInches(),
            C = shelf.RightWidth.AsInches(),
            D = shelf.ProductLength.AsInches(),
            Notch = NotchDepth.NINETEEN,
            NotchSide = NotchSide.LEFT,
            Radius = 2,
        };

    }

    public static IAllmoxyProduct MapPartToAdjustableAngledShelf(CornerShelf shelf) {

        string bandingColor;
        if (shelf.Color == shelf.EdgeBandingColor) {
            bandingColor = ClosetEdgeBandingMaterial.MATCH;
        } else {
            bandingColor = ClosetEdgeBandingMaterial.GetMatchingMaterialName(shelf.EdgeBandingColor);
        }

        return new AdjustableAngledShelf() {
            Folder = shelf.Room,
            ClosetMaterial = ClosetMaterials.GetMatchingMaterialName(shelf.Color),
            BandingColor = bandingColor,
            PanelFinish = PanelFinish.NONE,
            Qty = shelf.Qty,
            PartComment = string.Empty,
            A = shelf.NotchSideLength.AsInches(),
            B = shelf.ProductWidth.AsInches(),
            C = shelf.RightWidth.AsInches(),
            D = shelf.ProductLength.AsInches(),
            Notch = NotchDepth.NINETEEN,
            NotchSide = NotchSide.LEFT,
        };

    }

    public static IAllmoxyProduct MapPartToFixedAngledShelf(CornerShelf shelf) {

        string bandingColor;
        if (shelf.Color == shelf.EdgeBandingColor) {
            bandingColor = ClosetEdgeBandingMaterial.MATCH;
        } else {
            bandingColor = ClosetEdgeBandingMaterial.GetMatchingMaterialName(shelf.EdgeBandingColor);
        }

        return new FixedAngledShelf() {
            Folder = shelf.Room,
            ClosetMaterial = ClosetMaterials.GetMatchingMaterialName(shelf.Color),
            BandingColor = bandingColor,
            PanelFinish = PanelFinish.NONE,
            Qty = shelf.Qty,
            PartComment = string.Empty,
            A = shelf.NotchSideLength.AsInches(),
            B = shelf.ProductWidth.AsInches(),
            C = shelf.RightWidth.AsInches(),
            D = shelf.ProductLength.AsInches(),
            Notch = NotchDepth.NINETEEN,
            NotchSide = NotchSide.LEFT,
            Recess = Recess.FLUSH
        };

    }

}
