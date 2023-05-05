using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.Builders;

public class DovetailDrawerBoxBuilder {

    public int Qty { get; private set; }
    public int ProductNumber { get; private set; }
    public Dimension Height { get; private set; }
    public Dimension Width { get; private set; }
    public Dimension Depth { get; private set; }
    public string Note { get; private set; }
    public DrawerBoxOptions Options { get; private set; }
    public Dictionary<string, string> LabelFields { get; private set; }

    /// <summary>
    /// Minium clearance between top of drawer box and top of drawer face
    /// </summary>
    public static Dimension VerticalClearance { get; set; } = Dimension.FromMillimeters(40);

    /// <summary>
    /// The thickness of the divider between two horizontally adjacent drawer boxes
    /// </summary>
    public static Dimension DividerThickness { get; set; } = Dimension.FromMillimeters(19);

    public static Dimension RollOutBlockThickness { get; set; } = Dimension.FromInches(1);

    public static List<Dimension> StdHeights { get; set; } = new() {
        Dimension.FromMillimeters(64),
        Dimension.FromMillimeters(86),
        Dimension.FromMillimeters(105),
        Dimension.FromMillimeters(137),
        Dimension.FromMillimeters(159),
        Dimension.FromMillimeters(181),
        Dimension.FromMillimeters(210),
        Dimension.FromMillimeters(260),
    };

    public static Dictionary<DrawerSlideType, Dimension> DrawerSlideWidthAdjustments { get; set; } = new() {
        {  DrawerSlideType.UnderMount, Dimension.FromMillimeters(10) },
        {  DrawerSlideType.SideMount, Dimension.FromMillimeters(26) }
    };

    public static Dictionary<DrawerSlideType, Dimension> DrawerSlideDepthClearance { get; set; } = new() {
        {  DrawerSlideType.UnderMount, Dimension.FromMillimeters(19) },
        {  DrawerSlideType.SideMount, Dimension.FromMillimeters(0) }
    };

    public static Dimension[] UnderMountDrawerSlideDepths { get; set; } = new Dimension[] {
        Dimension.FromMillimeters(229),
        Dimension.FromMillimeters(305),
        Dimension.FromMillimeters(380),
        Dimension.FromMillimeters(450),
        Dimension.FromMillimeters(550)
    };

    public DovetailDrawerBoxBuilder() {
        Qty = 0;
        ProductNumber = 0;
        Height = Dimension.Zero;
        Width = Dimension.Zero;
        Depth = Dimension.Zero;
        Note = string.Empty;
        LabelFields = new();
        Options = new("UNKNOWN", "UNKNOWN", "UNKNOWN", "UNKNOWN", "", "", "", LogoPosition.None);
    }

    public DovetailDrawerBoxBuilder WithQty(int qty) {
        Qty = qty;
        return this;
    }

    public DovetailDrawerBoxBuilder WithProductNumber(int productNumber) {
        ProductNumber = productNumber;
        return this;
    }

    public DovetailDrawerBoxBuilder WithBoxHeight(Dimension height) {
        Height = height;
        return this;
    }

    public DovetailDrawerBoxBuilder WithDrawerFaceHeight(Dimension faceHeight) {

        foreach (var height in StdHeights.OrderBy(height => height)) {

            if (height > faceHeight - VerticalClearance) {
                continue;
            }

            Height = height;
            return this;

        }

        throw new ArgumentOutOfRangeException(nameof(faceHeight), "No valid drawer box height for given drawer face height");

    }

    public DovetailDrawerBoxBuilder WithBoxWidth(Dimension width) {
        Width = width;
        return this;
    }

    public DovetailDrawerBoxBuilder WithInnerCabinetWidth(Dimension innerCabinetWidth, int drawerCount, DrawerSlideType slideType) {

        // Between each drawer box there are 2 dividers
        int dividerCount = (drawerCount - 1) * 2;

        Dimension availableWidth = innerCabinetWidth - dividerCount * DividerThickness;

        var widthMM = (availableWidth / drawerCount - DrawerSlideWidthAdjustments[slideType]).AsMillimeters();

        Width = Dimension.FromMillimeters(Math.Round(widthMM, 2));

        return this;
    }

    public DovetailDrawerBoxBuilder WithInnerCabinetWidth(Dimension innerCabinetWidth, RollOutBlockPosition rollOutPositions, DrawerSlideType slideType) {

        Dimension blockWidthAdjustment = rollOutPositions switch {
            RollOutBlockPosition.Left or RollOutBlockPosition.Right => RollOutBlockThickness,
            RollOutBlockPosition.Both => RollOutBlockThickness * 2,
            RollOutBlockPosition.None or _ => Dimension.Zero
        };

        var widthMM = (innerCabinetWidth - blockWidthAdjustment - DrawerSlideWidthAdjustments[slideType]).AsMillimeters();

        Width = Dimension.FromMillimeters(Math.Ceiling(widthMM));

        return this;
    }

    public DovetailDrawerBoxBuilder WithBoxDepth(Dimension depth) {
        Depth = depth;
        return this;
    }

    public DovetailDrawerBoxBuilder WithInnerCabinetDepth(Dimension innerCabinetDepth, DrawerSlideType slideType, bool isRollOut = false) {
        Depth = GetDrawerBoxDepthFromInnerCabinetDepth(innerCabinetDepth, slideType, isRollOut);
        return this;
    }

    public static Dimension GetDrawerBoxDepthFromInnerCabinetDepth(Dimension innerCabinetDepth, DrawerSlideType slideType, bool isRollOut = false) {

        var clearance = DrawerSlideDepthClearance[slideType];

        if (slideType is DrawerSlideType.UnderMount) {

            foreach (var depth in UnderMountDrawerSlideDepths.OrderBy(depth => depth)) {

                if (depth > innerCabinetDepth - clearance) {
                    continue;
                }

                return depth;

            }

        } else if (slideType is DrawerSlideType.SideMount) {

            double accuracy = isRollOut ? (50 / 25.4) : 2;

            int multiple = (int)((innerCabinetDepth - clearance).AsInches() / accuracy);

            return Dimension.FromInches(multiple * accuracy); ;

        }

        throw new ArgumentOutOfRangeException(nameof(innerCabinetDepth), "No valid drawer box depth for given cabinet depth and drawer slide type");

    }

    public DovetailDrawerBoxBuilder WithNote(string note) {
        Note = note;
        return this;
    }

    public DovetailDrawerBoxBuilder WithOptions(DrawerBoxOptions options) {
        Options = options;
        return this;
    }

    public DovetailDrawerBoxBuilder WithLabelFields(Dictionary<string, string> labelFields) {
        LabelFields = labelFields;
        return this;
    }

    public DovetailDrawerBox Build() {
        return new DovetailDrawerBox(Qty, ProductNumber, Height, Width, Depth, Note, Options, LabelFields);
    }

}
