using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

public class DovetailDrawerBox {

    public const string FINGER_JOINT_BIRCH = "Birch FJ";
    public const string SOLID_BIRCH = "Birch CL";

    public int Qty { get; }
    public int ProductNumber { get; }
    public Dimension Height { get; }
    public Dimension Width { get; }
    public Dimension Depth { get; }
    public string Note { get; }
    public DrawerBoxOptions DrawerBoxOptions { get; }
    public IReadOnlyDictionary<string, string> LabelFields { get; }

    public DovetailDrawerBox(int qty, int productNumber, Dimension height, Dimension width, Dimension depth,
                            string note, DrawerBoxOptions options, IReadOnlyDictionary<string, string> labelFields) {
        Qty = qty;
        ProductNumber = productNumber;
        Height = height;
        Width = width;
        Depth = depth;
        Note = note;
        DrawerBoxOptions = options;
        LabelFields = labelFields;
    }

    public IEnumerable<DrawerBoxPart> GetParts(ConstructionValues construction) {

        string accComment = DrawerBoxOptions.Accessory.Equals("None") ? "" : DrawerBoxOptions.Accessory;
        string logoComment = DrawerBoxOptions.Logo switch {
            LogoPosition.Inside => "Logo-Inside",
            LogoPosition.Outside => "Logo-Outside",
            LogoPosition.None or _ => "",
        };

        var fbAdj = Dimension.FromMillimeters(construction.FrontBackWidthAdjustment);
        var sdAdj = Dimension.FromMillimeters(construction.SideLengthAdjustment);
        var btAdj = Dimension.FromMillimeters(construction.BottomSizeAdjustment);

        var front = new DrawerBoxPart(DrawerBoxPartType.Front, Qty, ProductNumber, Height, Width + fbAdj, DrawerBoxOptions.FrontMaterial, DrawerBoxOptions.ScoopFront ? "Scoop Front" : "");
        var back = new DrawerBoxPart(DrawerBoxPartType.Back, Qty, ProductNumber, Height, Width + fbAdj, DrawerBoxOptions.BackMaterial, accComment);
        var side = new DrawerBoxPart(DrawerBoxPartType.Side, Qty * 2, ProductNumber, Height, Depth + sdAdj, DrawerBoxOptions.SideMaterial, logoComment);
        var bottom = new DrawerBoxPart(DrawerBoxPartType.Bottom, Qty, ProductNumber, Width + btAdj, Depth + btAdj, DrawerBoxOptions.BottomMaterial, "");

        if (DrawerBoxOptions.UBoxDimensions is not null) {

            var a = DrawerBoxOptions.UBoxDimensions.A;
            var b = DrawerBoxOptions.UBoxDimensions.B;
            var c = DrawerBoxOptions.UBoxDimensions.C;
            var diff = Width - (a + b);

            var leftBack = new DrawerBoxPart(DrawerBoxPartType.BackLeft, Qty, ProductNumber, Height, a + fbAdj, DrawerBoxOptions.BackMaterial, "");
            var rightBack = new DrawerBoxPart(DrawerBoxPartType.BackRight, Qty, ProductNumber, Height, b + fbAdj, DrawerBoxOptions.BackMaterial, "");
            var centerBack = new DrawerBoxPart(DrawerBoxPartType.BackCenter, Qty, ProductNumber, Height, diff + fbAdj, DrawerBoxOptions.BackMaterial, "");
            var sideCenter = new DrawerBoxPart(DrawerBoxPartType.SideCenter, Qty, ProductNumber, Height, c + sdAdj, DrawerBoxOptions.SideMaterial, "");

            return new List<DrawerBoxPart>() {
                front, leftBack, rightBack, centerBack, sideCenter, side, bottom
            };

        } else {

            return new List<DrawerBoxPart>() {
                front, back, side, bottom
            };

        }

    }

    public string GetMaterialFriendlyName() {

        if (DrawerBoxOptions.FrontMaterial == DrawerBoxOptions.BackMaterial 
            && DrawerBoxOptions.SideMaterial == DrawerBoxOptions.SideMaterial) {
            return DrawerBoxOptions.FrontMaterial;
        }

        if (DrawerBoxOptions.FrontMaterial == DrawerBoxOptions.BackMaterial
            && DrawerBoxOptions.FrontMaterial == FINGER_JOINT_BIRCH
            && DrawerBoxOptions.SideMaterial == SOLID_BIRCH) {
            return "Hybrid Birch";
        }

        return $"{DrawerBoxOptions.FrontMaterial} / {DrawerBoxOptions.BackMaterial} / {DrawerBoxOptions.SideMaterial}";

    }

}