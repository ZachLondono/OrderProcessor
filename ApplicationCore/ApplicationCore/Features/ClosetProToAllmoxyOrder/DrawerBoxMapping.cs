using ApplicationCore.Features.AllmoxyOrderExport.Attributes;
using ApplicationCore.Features.AllmoxyOrderExport.Products;
using ApplicationCore.Features.AllmoxyOrderExport.Products.DrawerBoxes;
using OrderLoading.ClosetProCSVCutList.Products;

namespace ApplicationCore.Features.ClosetProToAllmoxyOrder;

public partial class ClosetProToAllmoxyMapper {

    public static IAllmoxyProduct MapDrawerBoxToAllmoxyProduct(DrawerBox box) {

        return box.Type switch {
            DrawerBoxType.Dovetail => MapDovetailDrawerBoxToAllmoxyProduct(box),
            DrawerBoxType.Dowel => MapDowelDrawerBoxToAllmoxyProduct(box),
            _ => throw new InvalidOperationException("Unexpected drawer box type")
        };

    }

    public static IAllmoxyProduct MapDovetailDrawerBoxToAllmoxyProduct(DrawerBox box) {

        string notch = box.UnderMountNotches ? UndermountNotching.STANDARD : UndermountNotching.NONE;

        return new DovetailDrawerBox() {
            Folder = box.Room,
            SideMaterial = DrawerBoxMaterial.ECONOMY_BIRCH,
            BottomThickness = DrawerBoxBottomThickness.QUARTER_INCH_PLY,
            UndermountNotching = notch,
            Clips = DrawerBoxClips.HETTICH,
            IncludeSlides = DrawerBoxSlides.HETTICH,
            Assembled = true,
            Comments = "",
            Qty = box.Qty,
            Height = DrawerBoxMaterial.GetStandardHeight(box.GetBoxHeight()),
            Width = DrawerBoxMaterial.GetBoxWidthFromOpening(box.Width).AsInches(),
            Depth = DrawerBoxMaterial.GetBoxDepthFromOpening(box.Depth).AsInches(),
            Scoop = box.ScoopFront,
            Logo = false,
            LabelNote = box.Room,
            Insert = DrawerBoxInsert.NONE,
        };

    }

    public static IAllmoxyProduct MapDowelDrawerBoxToAllmoxyProduct(DrawerBox box) {

        return new DoweledDrawerBox() {
            Folder = box.Room,
            BoxConstruction = DoweledDrawerBoxConstruction.WHITE_MELA,
            UndermountNotch = box.UnderMountNotches ? DoweledUndermountNotching.STANDARD : DoweledUndermountNotching.NONE,
            Qty = box.Qty,
            Height = box.Height.AsInches(),
            Width = box.Width.AsInches(),
            Depth = box.Depth.AsInches(),
        };

    }

    //public IAllmoxyProduct MapToZargenDrawerBox(DrawerBox box) => throw new NotImplementedException();

}
