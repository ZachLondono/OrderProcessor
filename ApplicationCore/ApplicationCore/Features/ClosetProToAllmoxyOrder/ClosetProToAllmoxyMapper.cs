using ApplicationCore.Features.AllmoxyOrderExport.Products;
using ApplicationCore.Features.ClosetProCSVCutList;
using ApplicationCore.Features.ClosetProCSVCutList.CSVModels;
using ApplicationCore.Features.ClosetProCSVCutList.Products;
using ApplicationCore.Features.Companies.Contracts.ValueObjects;

namespace ApplicationCore.Features.ClosetProToAllmoxyOrder;

public partial class ClosetProToAllmoxyMapper(ClosetProPartMapper partMapper) {

    private readonly ClosetProPartMapper _partMapper = partMapper;

    public IEnumerable<IAllmoxyProduct> Map(ClosetProOrderInfo closetProOrder) {

        var settings = new ClosetProSettings();

        List<OtherPart> otherParts = [];
        otherParts.AddRange(ClosetProPartMapper.MapPickListToItems(closetProOrder.PickList, [], out var hardwareSpread));
        otherParts.AddRange(ClosetProPartMapper.MapAccessoriesToItems(closetProOrder.Accessories));
        otherParts.AddRange(ClosetProPartMapper.MapBuyOutPartsToItems(closetProOrder.BuyOutParts));

        _partMapper.GroupLikeParts = true; // TODO: Move this into the closet pro settings object

        return _partMapper.MapPartsToProducts(closetProOrder.Parts, hardwareSpread).Select(MapPartToProduct);

    }

    public static IAllmoxyProduct MapPartToProduct(IClosetProProduct cpProduct) {

        if (cpProduct is Shelf shelf) {
            return MapShelfToAllmoxyProduct(shelf);
        } else if (cpProduct is DividerShelf dividerShelf) {
            return MapDividerShelfPartToAllmoxyProduct(dividerShelf);
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
            return MapVerticalPanelToAllmoxyProduct(panel);
        } else if (cpProduct is HutchVerticalPanel hutchPanel) {
            return MapVerticalHutchPanelToAllmoxyProduct(hutchPanel);
        } else if (cpProduct is TransitionVerticalPanel transitionPanel) {
            return MapVerticalTransitionPanelToAllmoxyProduct(transitionPanel);
        } else if (cpProduct is DividerVerticalPanel dividerVertical) {
            return MapDividerPanelToAllmoxyProduct(dividerVertical);
        }
        
        else if (cpProduct is MiscellaneousClosetPart miscellaneous) {
            return MapMiscellaneousToAllmoxyProduct(miscellaneous);
        }

        throw new InvalidOperationException("Unexpected closet pro product");

    }


}
