using ApplicationCore.Features.AllmoxyOrderExport.Attributes;
using ApplicationCore.Features.AllmoxyOrderExport.Products;
using ApplicationCore.Features.AllmoxyOrderExport.Products.Fronts;
using Domain.Orders.Enums;
using OrderLoading.ClosetProCSVCutList.Products;

namespace ApplicationCore.Features.ClosetProToAllmoxyOrder;

public partial class ClosetProToAllmoxyMapper {

    public static IAllmoxyProduct MapToMDFDoor(MDFFront front) {

        if (front.Frame.TopRail != front.Frame.BottomRail
            || front.Frame.TopRail != front.Frame.LeftStile
            || front.Frame.TopRail != front.Frame.RightStile) {
            throw new InvalidOperationException("Allmoxy MDF door product does not support different stile/rail widths");
        }

        return new MDFDoorFront() {
            Folder = front.Room,
            FramingBead = FramingBead.SHAKER,
            PanelDetail = PanelDetail.FLAT,
            EdgeProfile = EdgeProfile.EASED,
            Finish = Finish.SANDED,
            Material = MDFMaterial.STANDARD,
            Rails = front.Frame.TopRail.AsInches(),
            Stiles = front.Frame.TopRail.AsInches(),
            ReducedRails = front.Frame.TopRail.AsInches(),
            CustomPanelDrop = 0,
            HingeDrilling = false,
            OrderComments = "",
            Qty = front.Qty,
            Width = front.Width.AsInches(),
            Height = front.Height.AsInches(),
            GlassFrame = false,
            ReduceRails = false,
            Comments = "",
        };

    }

    public static IAllmoxyProduct MapToSlabDoor(MelamineSlabFront front) {

        string bandingColor;
        if (front.Color == front.EdgeBandingColor) {
            bandingColor = ClosetEdgeBandingMaterial.MATCH;
        } else {
            bandingColor = ClosetEdgeBandingMaterial.GetMatchingMaterialName(front.EdgeBandingColor);
        }

        return front.Type switch {
            DoorType.Door => new SlabDoorFront() {
                Folder = front.Room,
                ClosetMaterial = ClosetEdgeBandingMaterial.GetMatchingMaterialName(front.Color),
                BandingColor = bandingColor,
                PanelFinish = PanelFinish.NONE,
                Qty = front.Qty,
                Width = front.Width.AsInches(),
                Height = front.Height.AsInches(),
                HingeDrilling = false,
                PartComment = "",
            },
            DoorType.DrawerFront => new SlabDrawerFront() {
                Folder = front.Room,
                ClosetMaterial = ClosetEdgeBandingMaterial.GetMatchingMaterialName(front.Color),
                BandingColor = bandingColor,
                //PanelFinish = PanelFinish.NONE,
                Qty = front.Qty,
                Width = front.Width.AsInches(),
                Height = front.Height.AsInches(),
                DrawerLock = false,
                HardwareDrilling = HardwareDrilling.NONE,
                PartComment = "",
            },
            _ => throw new InvalidOperationException("Unexpected slab door type")
        };

    }

    public static IAllmoxyProduct MapTo5PieceDoor(FivePieceFront front) {

        if (front.Frame.TopRail != front.Frame.BottomRail
            || front.Frame.TopRail != front.Frame.LeftStile
            || front.Frame.TopRail != front.Frame.RightStile) {
            throw new InvalidOperationException("Allmoxy 5-Piece door product does not support different stile/rail widths");
        }

        return new FivePieceDoor() {
            Folder = front.Room,
            ClosetMaterial = ClosetEdgeBandingMaterial.GetMatchingMaterialName(front.Color),
            Qty = front.Qty,
            Height = front.Height.AsInches(),
            Width = front.Width.AsInches(),
            Rails = front.Frame.TopRail.AsInches(),
            PartComment = ""
        };

    }

}
