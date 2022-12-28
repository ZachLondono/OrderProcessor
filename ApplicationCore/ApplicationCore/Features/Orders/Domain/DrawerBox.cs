using ApplicationCore.Features.Orders.Domain.ValueObjects;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.Domain;

public class DrawerBox {

    public Guid Id { get; }
    public int LineInOrder { get; }
    public decimal UnitPrice { get; }
    public int Qty { get; }
    public Dimension Height { get; }
    public Dimension Width { get; }
    public Dimension Depth { get; }
    public string Note { get; }
    public IReadOnlyDictionary<string, string> LabelFields { get; }
    public DrawerBoxOptions Options { get; }

    public DrawerBox(Guid id, int line, decimal unitPrice, int qty, Dimension height, Dimension width, Dimension depth, string note, IReadOnlyDictionary<string, string> labelFields, DrawerBoxOptions options) {
        Id = id;
        LineInOrder = line;
        UnitPrice = unitPrice;
        Qty = qty;
        Height = height;
        Width = width;
        Depth = depth;
        Note = note;
        LabelFields = labelFields;
        Options = options;
    }

    public static DrawerBox Create(int line, decimal unitPrice, int qty, Dimension height, Dimension width, Dimension depth, string note, IReadOnlyDictionary<string, string> labelFields,DrawerBoxOptions options) {
        return new(Guid.NewGuid(), line, unitPrice, qty, height, width, depth, note, labelFields, options);
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

        var front = new DrawerBoxPart(DrawerBoxPartType.Front, Qty, Height, Width + fbAdj, Options.BoxMaterialId, Options.ScoopFront ? "Scoop Front" : "");
        var back = new DrawerBoxPart(DrawerBoxPartType.Back, Qty, Height, Width + fbAdj, Options.BoxMaterialId, accComment);
        var side = new DrawerBoxPart(DrawerBoxPartType.Side, Qty * 2, Height, Depth + sdAdj, Options.BoxMaterialId, logoComment);
        var bottom = new DrawerBoxPart(DrawerBoxPartType.Bottom, Qty, Width + btAdj, Depth + btAdj, Options.BottomMaterialId, "");

        if (Options.UBoxDimensions is not null) {

            var a = Options.UBoxDimensions.A;
            var b = Options.UBoxDimensions.B;
            var c = Options.UBoxDimensions.C;
            var diff = Width - (a + b);

            var leftBack = new DrawerBoxPart(DrawerBoxPartType.BackLeft, Qty, Height, a + fbAdj, Options.BoxMaterialId, "");
            var rightBack = new DrawerBoxPart(DrawerBoxPartType.BackRight, Qty, Height, b + fbAdj, Options.BoxMaterialId, "");
            var centerBack = new DrawerBoxPart(DrawerBoxPartType.BackCenter, Qty, Height, diff + fbAdj, Options.BoxMaterialId, "");
            var sideCenter = new DrawerBoxPart(DrawerBoxPartType.SideCenter, Qty, Height, c + sdAdj, Options.BoxMaterialId, "");

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
