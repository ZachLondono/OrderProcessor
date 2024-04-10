using ApplicationCore.Features.AllmoxyOrderExport.Products;
using ApplicationCore.Features.ClosetProToAllmoxyOrder.Models;
using OrderLoading.ClosetProCSVCutList.Products;
using OrderLoading.ClosetProCSVCutList.Products.Fronts;
using OrderLoading.ClosetProCSVCutList.Products.Shelves;
using OrderLoading.ClosetProCSVCutList.Products.Verticals;

namespace ApplicationCore.Features.ClosetProToAllmoxyOrder;

public partial class ClosetProToAllmoxyMapper() {

    public IEnumerable<IAllmoxyProduct> Map(IEnumerable<IClosetProProduct> closetProProducts, CPToAllmoxyMappingSettings settings) {

        return closetProProducts.Select(p => MapPartToProduct(p, settings));

    }

    public static IAllmoxyProduct MapPartToProduct(IClosetProProduct cpProduct, CPToAllmoxyMappingSettings settings) {

        if (cpProduct is Shelf shelf) {
            return MapShelfToAllmoxyProduct(shelf, settings.UseDoubleCamShelves, settings.UseSafetyShelves);
        } else if (cpProduct is DividerShelf dividerShelf) {
            return MapDividerShelfPartToAllmoxyProduct(dividerShelf, settings.UseDoubleCamShelves);
        } else if (cpProduct is CornerShelf cornerShelf) {
            return MapCornerShelfToAllmoxyProduct(cornerShelf);
        }
        
        else if (cpProduct is DrawerBox box) {
            return MapDrawerBoxToAllmoxyProduct(box);
        }

        else if (cpProduct is MDFFront mdf) {
            return MapToMDFDoor(mdf);
        } else if (cpProduct is MelamineSlabFront slab) {
            return MapToSlabDoor(slab);
        } else if (cpProduct is FivePieceFront fivePiece) {
            return MapTo5PieceDoor(fivePiece);
        }
        
        else if (cpProduct is VerticalPanel panel) {
            return MapVerticalPanelToAllmoxyProduct(panel, settings.DoNotUseWallMount);
        } else if (cpProduct is HutchVerticalPanel hutchPanel) {
            return MapVerticalHutchPanelToAllmoxyProduct(hutchPanel, settings.DoNotUseWallMount);
        } else if (cpProduct is TransitionVerticalPanel transitionPanel) {
            return MapVerticalTransitionPanelToAllmoxyProduct(transitionPanel, settings.DoNotUseWallMount);
        } else if (cpProduct is DividerVerticalPanel dividerVertical) {
            return MapDividerPanelToAllmoxyProduct(dividerVertical);
        }
        
        else if (cpProduct is MiscellaneousClosetPart miscellaneous) {
            return MapMiscellaneousToAllmoxyProduct(miscellaneous);
        }

        throw new InvalidOperationException("Unexpected closet pro product");

    }


}
