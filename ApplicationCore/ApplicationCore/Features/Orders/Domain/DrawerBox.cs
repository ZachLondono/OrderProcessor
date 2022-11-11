using ApplicationCore.Features.Orders.Domain.ValueObjects;

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
    public DrawerBoxOptions Options { get; }

    public DrawerBox(Guid id, int line, decimal unitPrice, int qty, Dimension height, Dimension width, Dimension depth, string note, DrawerBoxOptions options) {
        Id = id;
        LineInOrder = line;
        UnitPrice = unitPrice;
        Qty = qty;
        Height = height;
        Width = width;
        Depth = depth;
        Note = note;
        Options = options;
    }

    public static DrawerBox Create(int line, decimal unitPrice, int qty, Dimension height, Dimension width, Dimension depth, string note, DrawerBoxOptions options) {
        return new(Guid.NewGuid(), line, unitPrice, qty, height, width, depth, note, options);
    }

    public IEnumerable<DrawerBoxPart> GetParts(ConstructionValues construction){

        string boxMaterial = Options.BoxMaterial.Name;
        if (construction.MaterialCodes.ContainsKey(Options.BoxMaterial.Id.ToString())) {
            boxMaterial = construction.MaterialCodes[Options.BoxMaterial.Id.ToString()];
        }

        string botMaterial = Options.BottomMaterial.Name;
        if (construction.MaterialCodes.ContainsKey(Options.BottomMaterial.Id.ToString())) {
            botMaterial = construction.MaterialCodes[Options.BottomMaterial.Id.ToString()];
        }

        string accComment = Options.Accessory.Name.Equals("None") ? "" : Options.Accessory.Name;
        string logoComment = Options.Logo ? "Logo" : "";

        var fbAdj = Dimension.FromMillimeters(construction.FrontBackWidthAdjustment);
        var sdAdj = Dimension.FromMillimeters(construction.SideLengthAdjustment);
        var btAdj = Dimension.FromMillimeters(construction.BottomSizeAdjustment);

        var sideThickness = Options.BoxMaterial.Thickness;
        var bottomThickness = Options.BottomMaterial.Thickness;

        var front = new DrawerBoxPart(DrawerBoxPartType.Front, Qty, Height, Width + fbAdj, sideThickness, boxMaterial, Options.ScoopFront ? "Scoop Front" : "");
        var back = new DrawerBoxPart(DrawerBoxPartType.Back, Qty, Height, Width + fbAdj, sideThickness, boxMaterial, accComment);
        var side = new DrawerBoxPart(DrawerBoxPartType.Side, Qty * 2, Height, Depth + sdAdj, sideThickness, boxMaterial, logoComment);
        var bottom = new DrawerBoxPart(DrawerBoxPartType.Bottom, Qty, Width + btAdj, Depth + btAdj, bottomThickness, botMaterial, "");

        if (Options.UBoxDimensions is not null) {

            var a = Options.UBoxDimensions.A;
            var b = Options.UBoxDimensions.B;
            var c = Options.UBoxDimensions.C;
            var diff = Width - (a + b);

            var leftBack = new DrawerBoxPart(DrawerBoxPartType.BackLeft, Qty, Height, sideThickness, a + fbAdj, boxMaterial, "");
            var rightBack = new DrawerBoxPart(DrawerBoxPartType.BackRight, Qty, Height, sideThickness, b + fbAdj, boxMaterial, "");
            var centerBack = new DrawerBoxPart(DrawerBoxPartType.BackCenter, Qty, Height, sideThickness, diff + fbAdj, boxMaterial, "");
            var sideCenter = new DrawerBoxPart(DrawerBoxPartType.SideCenter, Qty, Height, sideThickness, c + sdAdj, boxMaterial, "");

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
