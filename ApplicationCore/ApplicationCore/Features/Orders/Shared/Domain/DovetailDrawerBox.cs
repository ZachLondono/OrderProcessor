using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain;

public class DovetailDrawerBox {

    public int Qty { get; }
    public Dimension Height { get; }
    public Dimension Width { get; }
    public Dimension Depth { get; }
    public string Note { get; }
    public DrawerBoxOptions Options { get; }
    public IReadOnlyDictionary<string, string> LabelFields { get; }

    public DovetailDrawerBox(int qty, Dimension height, Dimension width, Dimension depth,
                            string note, DrawerBoxOptions options, IReadOnlyDictionary<string, string> labelFields) {
        Qty = qty;
        Height = height;
        Width = width;
        Depth = depth;
        Note = note;
        Options = options;
        LabelFields = labelFields;
    }

    public IEnumerable<DrawerBoxPart> GetParts(ConstructionValues construction) {

        string accComment = Options.Accessory.Equals("None") ? "" : Options.Accessory;
        string logoComment = Options.Logo switch {
            LogoPosition.Inside => "Logo-Inside",
            LogoPosition.Outside => "Logo-Outside",
            LogoPosition.None or _ => "",
        };

        var fbAdj = Dimension.FromMillimeters(construction.FrontBackWidthAdjustment);
        var sdAdj = Dimension.FromMillimeters(construction.SideLengthAdjustment);
        var btAdj = Dimension.FromMillimeters(construction.BottomSizeAdjustment);

        var front = new DrawerBoxPart(DrawerBoxPartType.Front, Qty, Height, Width + fbAdj, Options.FrontMaterial, Options.ScoopFront ? "Scoop Front" : "");
        var back = new DrawerBoxPart(DrawerBoxPartType.Back, Qty, Height, Width + fbAdj, Options.BackMaterial, accComment);
        var side = new DrawerBoxPart(DrawerBoxPartType.Side, Qty * 2, Height, Depth + sdAdj, Options.SideMaterial, logoComment);
        var bottom = new DrawerBoxPart(DrawerBoxPartType.Bottom, Qty, Width + btAdj, Depth + btAdj, Options.BottomMaterial, "");

        if (Options.UBoxDimensions is not null) {

            var a = Options.UBoxDimensions.A;
            var b = Options.UBoxDimensions.B;
            var c = Options.UBoxDimensions.C;
            var diff = Width - (a + b);

            var leftBack = new DrawerBoxPart(DrawerBoxPartType.BackLeft, Qty, Height, a + fbAdj, Options.BackMaterial, "");
            var rightBack = new DrawerBoxPart(DrawerBoxPartType.BackRight, Qty, Height, b + fbAdj, Options.BackMaterial, "");
            var centerBack = new DrawerBoxPart(DrawerBoxPartType.BackCenter, Qty, Height, diff + fbAdj, Options.BackMaterial, "");
            var sideCenter = new DrawerBoxPart(DrawerBoxPartType.SideCenter, Qty, Height, c + sdAdj, Options.SideMaterial, "");

            return new List<DrawerBoxPart>() {
                front, leftBack, rightBack, centerBack, sideCenter, side, bottom
            };

        } else {

            return new List<DrawerBoxPart>() {
                front, back, side, bottom
            };

        }

    }

}
