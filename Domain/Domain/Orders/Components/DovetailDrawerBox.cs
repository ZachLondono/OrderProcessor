using Domain.Orders.Enums;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;

namespace Domain.Orders.Components;

public class DovetailDrawerBox : IComponent {

    public int Qty { get; }
    public int ProductNumber { get; }
    public Dimension Height { get; }
    public Dimension Width { get; }
    public Dimension Depth { get; }
    public string Note { get; }
    public DovetailDrawerBoxConfig DrawerBoxOptions { get; }
    public IReadOnlyDictionary<string, string> LabelFields { get; }

    public DovetailDrawerBox(int qty, int productNumber, Dimension height, Dimension width, Dimension depth,
                            string note, DovetailDrawerBoxConfig options, IReadOnlyDictionary<string, string> labelFields) {
        Qty = qty;
        ProductNumber = productNumber;
        Height = height;
        Width = width;
        Depth = depth;
        Note = note;
        DrawerBoxOptions = options;
        LabelFields = labelFields;
    }

    public IEnumerable<DovetailDrawerBoxPart> GetParts(ConstructionValues construction) {

        string accComment = DrawerBoxOptions.Accessory.Equals("None") ? "" : DrawerBoxOptions.Accessory;
        string logoComment = DrawerBoxOptions.Logo switch {
            LogoPosition.Inside => "Logo-Inside",
            LogoPosition.Outside => "Logo-Outside",
            LogoPosition.None or _ => "",
        };

        var fbAdj = Dimension.FromMillimeters(construction.FrontBackWidthAdjustment);
        var sdAdj = Dimension.FromMillimeters(construction.SideLengthAdjustment);
        var btAdj = Dimension.FromMillimeters(construction.BottomSizeAdjustment);

        var front = new DovetailDrawerBoxPart(DrawerBoxPartType.Front, Qty, ProductNumber, Height, Width + fbAdj, DrawerBoxOptions.FrontMaterial, DrawerBoxOptions.ScoopFront ? "Scoop Front" : "");
        var back = new DovetailDrawerBoxPart(DrawerBoxPartType.Back, Qty, ProductNumber, Height, Width + fbAdj, DrawerBoxOptions.BackMaterial, accComment);
        var side = new DovetailDrawerBoxPart(DrawerBoxPartType.Side, Qty * 2, ProductNumber, Height, Depth + sdAdj, DrawerBoxOptions.SideMaterial, logoComment);
        var bottom = new DovetailDrawerBoxPart(DrawerBoxPartType.Bottom, Qty, ProductNumber, Width + btAdj, Depth + btAdj, DrawerBoxOptions.BottomMaterial, "");

        if (DrawerBoxOptions.UBoxDimensions is not null) {

            var a = DrawerBoxOptions.UBoxDimensions.A;
            var b = DrawerBoxOptions.UBoxDimensions.B;
            var c = DrawerBoxOptions.UBoxDimensions.C;
            var diff = Width - (a + b);

            var leftBack = new DovetailDrawerBoxPart(DrawerBoxPartType.BackLeft, Qty, ProductNumber, Height, a + fbAdj, DrawerBoxOptions.BackMaterial, "");
            var rightBack = new DovetailDrawerBoxPart(DrawerBoxPartType.BackRight, Qty, ProductNumber, Height, b + fbAdj, DrawerBoxOptions.BackMaterial, "");
            var centerBack = new DovetailDrawerBoxPart(DrawerBoxPartType.BackCenter, Qty, ProductNumber, Height, diff + fbAdj, DrawerBoxOptions.BackMaterial, "");
            var sideCenter = new DovetailDrawerBoxPart(DrawerBoxPartType.SideCenter, Qty, ProductNumber, Height, c + sdAdj, DrawerBoxOptions.SideMaterial, "");

            return new List<DovetailDrawerBoxPart>() {
                front, leftBack, rightBack, centerBack, sideCenter, side, bottom
            };

        } else {

            return new List<DovetailDrawerBoxPart>() {
                front, back, side, bottom
            };

        }

    }

    public string GetDescription() {

        string description = "Dovetail Drawer Box";

        if (DrawerBoxOptions.UBoxDimensions is not null) {
            description = "U-Shaped " + description;
        }

        if (DrawerBoxOptions.Logo != Enums.LogoPosition.None) {
            description += ", Logo";
        }

        if (DrawerBoxOptions.ScoopFront) {
            description += ", Scoop Front";
        }

        if (!string.IsNullOrWhiteSpace(DrawerBoxOptions.Accessory) && DrawerBoxOptions.Accessory.ToLowerInvariant() != "none") {
            description += $", {DrawerBoxOptions.Accessory}";
        }

        if (DrawerBoxOptions.FixedDividersCounts is not null) {
            description += ", Fixed Dividers";
        }

        if (DrawerBoxOptions.PostFinish) {
            description += ", Finished";
        }

        return description;

    }

}
