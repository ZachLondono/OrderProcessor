using Domain.Orders.Enums;
using Domain.Orders.Exceptions;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;
using CADCodeProxy.Enums;
using CADCodeProxy.Machining;
using CADCodeProxy.Machining.Tokens;
using Domain.ProductPlanner;

namespace Domain.Orders.Entities.Products.Closets;

public class CustomDrilledVerticalPanel : IProduct, IPPProductContainer, ICNCPartContainer, IClosetPartProduct {

    public Guid Id { get; }
    public int Qty { get; }
    public decimal UnitPrice { get; }
    public int ProductNumber { get; }
    public string Room { get; set; }
    public string SKU { get; }
    public Dimension Width { get; }
    public Dimension Length { get; }
    public ClosetMaterial Material { get; }
    public ClosetPaint? Paint { get; }
    public string EdgeBandingColor { get; }
    public string Comment { get; }
    public List<string> ProductionNotes { get; set; } = [];

    public ClosetVerticalDrillingType DrillingType { get; }
    public Dimension ExtendBack { get; }
    public Dimension ExtendFront { get; }
    public Dimension HoleDimensionFromBottom { get; }
    public Dimension HoleDimensionFromTop { get; }
    public Dimension TransitionHoleDimensionFromBottom { get; }
    public Dimension TransitionHoleDimensionFromTop { get; }
    public Dimension BottomNotchDepth { get; }
    public Dimension BottomNotchHeight { get; }
    public Dimension LEDChannelOffFront { get; } = Dimension.FromMillimeters(51);
    public Dimension LEDChannelWidth { get; } = Dimension.FromMillimeters(18.5);
    public Dimension LEDChannelDepth { get; } = Dimension.FromMillimeters(8.5);

    // TODO: Implement this 
    public bool EdgeBandTop { get; } = false;

    private readonly bool _requiresCustomDrilling;
    private static readonly Dimension s_holeSpacing = Dimension.FromMillimeters(32);
    private static readonly Dimension s_holesOffEdge = Dimension.FromMillimeters(37);
    private static readonly Dimension s_holesOffTop = Dimension.FromMillimeters(9.5);
    private static readonly Dimension s_holesOffBottom = Dimension.FromMillimeters(9.5);
    private static readonly Dimension s_holeDiameter = Dimension.FromMillimeters(5);
    private static readonly Dimension s_stoppedDepth = Dimension.FromMillimeters(16.5);
    private static readonly Dimension s_drillThroughDepth = Dimension.FromMillimeters(26);
    private static readonly string s_cutOutTool = "3-8Comp";
    private static readonly Dimension s_panelThickness = Dimension.FromMillimeters(19.00);

    private static readonly string s_largeLEDToolName = "1-2Dado";
    private static readonly Dimension s_largeLEDToolDiameter = Dimension.FromMillimeters(13.6);
    private static readonly string s_smallLEDToolName = "POCKET3";
    private static readonly Dimension s_smallLEDToolDiameter = Dimension.FromMillimeters(3);

    private static bool IncludeTopHole = false;

    public string GetDescription() => "Closet Part - Custom Drilled Vertical Panel";

    public CustomDrilledVerticalPanel(Guid id, int qty, decimal unitPrice, int productNumber, string room, Dimension width, Dimension length, ClosetMaterial material, ClosetPaint? paint, string edgeBandingColor, string comment,
                                    ClosetVerticalDrillingType drillingType, Dimension extendBack, Dimension extendFront, Dimension holeDimensionFromBottom, Dimension holeDimensionFromTop, Dimension transitionHoleDimensionFromBottom, Dimension transitionHoleDimensionFromTop, Dimension bottomNotchDepth, Dimension bottomNotchHeight,
                                    Dimension ledChannelOffFront, Dimension ledChannelWidth, Dimension ledChannelDepth) {
        Id = id;
        Qty = qty;
        UnitPrice = unitPrice;
        ProductNumber = productNumber;
        Room = room;
        Width = width;
        Length = length;
        Material = material;
        Paint = paint;
        EdgeBandingColor = edgeBandingColor;
        Comment = comment;

        DrillingType = drillingType;
        ExtendBack = extendBack;
        ExtendFront = extendFront;
        HoleDimensionFromBottom = holeDimensionFromBottom;
        HoleDimensionFromTop = holeDimensionFromTop;
        TransitionHoleDimensionFromBottom = transitionHoleDimensionFromBottom;
        TransitionHoleDimensionFromTop = transitionHoleDimensionFromTop;
        BottomNotchDepth = bottomNotchDepth;
        BottomNotchHeight = bottomNotchHeight;
        LEDChannelOffFront = ledChannelOffFront;
        LEDChannelWidth = ledChannelWidth;
        LEDChannelDepth = ledChannelDepth;

        if (DrillingType == ClosetVerticalDrillingType.DrilledThrough) {
            SKU = "PC";
        } else {
            SKU = "PE";
        }

        // PSI can only do custom transition drilling if the 'non-finished' side has full drilling & PSI does not handle custom drilling w/ bottom notch correctly 
        if ((holeDimensionFromBottom == Dimension.Zero && holeDimensionFromTop == Dimension.Zero
            || transitionHoleDimensionFromBottom == Dimension.Zero && transitionHoleDimensionFromTop == Dimension.Zero)
            && !((holeDimensionFromBottom != Dimension.Zero || holeDimensionFromTop != Dimension.Zero || transitionHoleDimensionFromBottom != Dimension.Zero || transitionHoleDimensionFromTop != Dimension.Zero) && BottomNotchHeight != Dimension.Zero)) {
            _requiresCustomDrilling = false;
        } else {
            _requiresCustomDrilling = true;
        }

        // TODO: support two sided drilling
        // If BOTH holes from top and holes from bottom are zero than EITHER transition holes from top can be greater than holes from top and transition holes from bottom can be greater than holes from bottom
        // If holes from top OR holes from bottom is not zero than BOTH transition holes from top must be less than or equal to holes from top and transition holes from bottom must be less than or equal to holes from bottom
        if ((HoleDimensionFromTop != Dimension.Zero || HoleDimensionFromBottom != Dimension.Zero)
            && (TransitionHoleDimensionFromTop > HoleDimensionFromTop || TransitionHoleDimensionFromBottom > HoleDimensionFromBottom)) {
            throw new InvalidProductOptionsException("Invalid hole dimensions, transition holes cannot exceed standard holes without flipping panel");
        }

        if (BottomNotchDepth != Dimension.Zero && BottomNotchHeight == Dimension.Zero
            || BottomNotchDepth == Dimension.Zero && BottomNotchHeight != Dimension.Zero) {
            throw new InvalidProductOptionsException("Invalid notch dimensions");
        }

    }

    public bool ContainsPPProducts() => !_requiresCustomDrilling;

    public IEnumerable<PPProduct> GetPPProducts() {

        if (_requiresCustomDrilling) {
            return Enumerable.Empty<PPProduct>();
        }

        (string materialType, string finishMaterial, string ebMaterial) = Material.Core switch {
            ClosetMaterialCore.ParticleBoard => ("Melamine", "Mela", "PVC"),
            ClosetMaterialCore.Plywood => ("PLY", "Veneer", "Veneer"),
            _ => ("Melamine", "Mela", "PVC"),
        };

        var finishMaterials = new Dictionary<string, PPMaterial>() {
            ["F_DoorDrawer"] = new(finishMaterial, Material.Finish),
            ["F_DrawerBox"] = new(finishMaterial, Material.Finish),
            ["F_Panel"] = new(finishMaterial, Material.Finish)
        };

        var ebMaterials = new Dictionary<string, PPMaterial>() {
            ["EB_Bottom"] = new(ebMaterial, EdgeBandingColor),
            ["EB_DoorDrawer"] = new(ebMaterial, EdgeBandingColor),
            ["EB_DrawerBox"] = new(ebMaterial, EdgeBandingColor),
            ["EB_HandEdgeBand"] = new(ebMaterial, EdgeBandingColor),
            ["EB_Panel"] = new(ebMaterial, EdgeBandingColor),
            ["EB_Top"] = new(ebMaterial, EdgeBandingColor)
        };

        var parameters = new Dictionary<string, string>() {
            ["FinLeft"] = DrillingType == ClosetVerticalDrillingType.FinishedLeft ? "1" : "0",
            ["FinRight"] = DrillingType == ClosetVerticalDrillingType.FinishedRight ? "1" : "0",
            ["ExtendBack"] = ExtendBack.AsMillimeters().ToString(),
            ["ExtendFront"] = ExtendFront.AsMillimeters().ToString(),
            ["HoleDimFromBot"] = HoleDimensionFromBottom.AsMillimeters().ToString(),
            ["HoleDimFromTop"] = HoleDimensionFromTop.AsMillimeters().ToString(),
            ["BottomNotchD"] = BottomNotchDepth.AsMillimeters().ToString(),
            ["BottomNotchH"] = BottomNotchHeight.AsMillimeters().ToString(),
            ["TransHoleBot"] = TransitionHoleDimensionFromBottom.AsMillimeters().ToString(),
            ["TransHoleTop"] = TransitionHoleDimensionFromTop.AsMillimeters().ToString(),
            ["ProductWidth"] = Width.AsMillimeters().ToString(),
            ["ProductLength"] = Length.AsMillimeters().ToString(),
            ["LEDfront"] = LEDChannelOffFront.AsMillimeters().ToString(),
            ["LEDwidth"] = LEDChannelWidth.AsMillimeters().ToString(),
            ["LEDdepth"] = LEDChannelDepth.AsMillimeters().ToString(),
        };

        return new List<PPProduct>() { new PPProduct(Id, Qty, Room, SKU, ProductNumber, "Royal_c", materialType, "slab", "standard", Comment, finishMaterials, ebMaterials, parameters, new Dictionary<string, string>(), new Dictionary<string, string>()) };
    }

    public bool ContainsCNCParts() => _requiresCustomDrilling;

    public IEnumerable<Part> GetCNCParts(string customerName) {

        if (!_requiresCustomDrilling) {
            return Enumerable.Empty<Part>();
        }

        List<IToken> tokens = new();

        if (Width > Dimension.FromInches(12) && IncludeTopHole) {
            // Vertical panels which are deeper than 12" have an additional hole drilled at the top of the panel which matches the first hole at the top of a 12" deep panel
            Dimension depth = DrillingType == ClosetVerticalDrillingType.DrilledThrough ? s_drillThroughDepth : s_stoppedDepth;
            tokens.Add(new Bore(s_holeDiameter.AsMillimeters(),
                                new((Width - Dimension.FromInches(12) + s_holesOffEdge).AsMillimeters(), (Length - s_holesOffTop).AsMillimeters()),
                                depth.AsMillimeters()));
        }

        GetDrillingOperations().ForEach(operation =>
            tokens.AddRange(CreateTwoRowsOfHoles(operation.Start, operation.End, operation.Depth == VPDrillingDepth.Stopped ? s_stoppedDepth : s_drillThroughDepth))
        );

        if (BottomNotchHeight > Dimension.Zero && BottomNotchHeight > Dimension.Zero) {
            tokens.Add(new Route() {
                ToolName = s_cutOutTool,
                StartDepth = s_panelThickness.AsMillimeters(),
                EndDepth = s_panelThickness.AsMillimeters(),
                Offset = Offset.Right,
                Start = new((Width - BottomNotchDepth).AsMillimeters(), 0),
                End = new((Width - BottomNotchDepth).AsMillimeters(), BottomNotchHeight.AsMillimeters())
            });
            tokens.Add(new Route() {
                ToolName = s_cutOutTool,
                StartDepth = s_panelThickness.AsMillimeters(),
                EndDepth = s_panelThickness.AsMillimeters(),
                Offset = Offset.Right,
                Start = new((Width - BottomNotchDepth).AsMillimeters(), BottomNotchHeight.AsMillimeters()),
                End = new(Width.AsMillimeters(), BottomNotchHeight.AsMillimeters())
            });
        }

        var edgeBanding = new EdgeBanding(EdgeBandingColor, "PVC");
        var topEdgeBanding = EdgeBandTop ? new EdgeBanding(EdgeBandingColor, "PVC") : new("", "");

        if (LEDChannelOffFront > Dimension.Zero) {
            tokens.AddRange(CreateLEDChannel());
        }

        string prodName = DrillingType switch {
            ClosetVerticalDrillingType.DrilledThrough => "PC",
            _ => "PE"
        };

        var part = new Part() {
            Width = Length.AsMillimeters(),
            Length = Width.AsMillimeters(),
            Thickness = s_panelThickness.AsMillimeters(),
            Material = Material.ToPSIMaterial(s_panelThickness).GetLongName(),
            IsGrained = true,
            Qty = Qty,
            PrimaryFace = new() {
                ProgramName = $"{SKU}{ProductNumber}",
                Rotation = 90,
                IsMirrored = DrillingType == ClosetVerticalDrillingType.FinishedRight,
                Tokens = [.. tokens]
			},
            Width1Banding = edgeBanding,
            Length1Banding = topEdgeBanding,
            InfoFields = new() {
                { "ProductName", prodName },
                { "Description", "Custom Vertical Panel" },
                { "Level1", Room },
                { "Comment1", "" },
                { "Comment2", "" },
                { "Side1Color", Material.Finish },
                { "Side1Material", Material.Core.ToString() },
                { "CabinetNumber", ProductNumber.ToString() },
                { "Cabinet Number", ProductNumber.ToString() },
                { "CustomerInfo1", customerName }
            }
        };

		return [part];

    }

    public List<VPDrillingOperation> GetDrillingOperations() {

        List<VPDrillingOperation> operations = [];

        if (HoleDimensionFromBottom == Dimension.Zero && HoleDimensionFromTop == Dimension.Zero) {

            /*
             * If hole dimension from top & from bottom are both zero, the whole panel should be drilled
             */
            Dimension stoppedStartHeight = s_holesOffTop + TransitionHoleDimensionFromTop;
            Dimension stoppedEndHeight = TransitionHoleDimensionFromBottom;
            if (stoppedStartHeight > stoppedEndHeight) {
                operations.Add(new(GetValidHolePositionFromTop(Length, stoppedStartHeight),
                                    GetValidHolePositionFromBottom(Length, stoppedEndHeight),
                                    VPDrillingDepth.Stopped));
            }

        } else {
            if (HoleDimensionFromTop > TransitionHoleDimensionFromTop) { // Stopped holes from the top start just after the last transition (full depth) hole. If HoleDimensionFromTop is 0 (or equal to the TransitionHoleDimensionFromTop) then there are no additional holes
                Dimension start;
                if (TransitionHoleDimensionFromTop > Dimension.Zero) {
                    // The position (relative to bottom of panel) of the last through drilled hole
                    start = GetValidHolePositionFromTop(Length, TransitionHoleDimensionFromTop) - s_holeSpacing;
                } else {
                    start = Length - s_holesOffTop;
                }
                Dimension end = GetValidHolePositionFromTop(Length, HoleDimensionFromTop);
                operations.Add(new(start, end, VPDrillingDepth.Stopped));
            }

            if (HoleDimensionFromBottom > TransitionHoleDimensionFromBottom) {
                Dimension end;
                if (TransitionHoleDimensionFromTop > Dimension.Zero) {
                    end = GetValidHolePositionFromBottom(Length, TransitionHoleDimensionFromBottom) + s_holeSpacing;
                } else {
                    end = GetValidHolePositionFromBottom(Length, s_holesOffBottom);
                }
                Dimension start = GetValidHolePositionFromBottom(Length, HoleDimensionFromBottom);
                operations.Add(new(start, end, VPDrillingDepth.Stopped));
            }
        }

        if (TransitionHoleDimensionFromTop > Dimension.Zero) {
            Dimension start = Length - s_holesOffTop;
            Dimension end = GetValidHolePositionFromTop(Length, TransitionHoleDimensionFromTop);
            operations.Add(new(start, end, VPDrillingDepth.Through));
        }

        if (TransitionHoleDimensionFromBottom > Dimension.Zero) {
            operations.Add(new(GetValidHolePositionFromBottom(Length, TransitionHoleDimensionFromBottom),
                                                GetValidHolePositionFromBottom(Length, s_holesOffBottom),
                                                VPDrillingDepth.Through));
        }

        return operations;

    }

    private IToken[] CreateLEDChannel() {

        Dimension toolDiameter = s_largeLEDToolDiameter;
        string toolName = s_largeLEDToolName;

        if (LEDChannelWidth < s_largeLEDToolDiameter) {
            toolDiameter = s_smallLEDToolDiameter;
            toolName = s_smallLEDToolName;
        }

        double offEdgeMM = 7;

        Dimension top = LEDChannelOffFront + LEDChannelWidth;
        Dimension bottom = LEDChannelOffFront;
        Dimension depth = LEDChannelDepth;

        var routeOffset = Offset.Left;

        var passCount = Dimension.CeilingMM(LEDChannelWidth / toolDiameter).AsMillimeters();
        var passDistance = LEDChannelWidth / passCount;

        List<IToken> tokens = new();

        Point start;
        Point end;

        for (int i = 0; i < passCount; i++) {

            start = new Point() {
                X = (top - i * passDistance).AsMillimeters(),
                Y = -offEdgeMM
            };

            end = new Point() {
                X = (top - i * passDistance).AsMillimeters(),
                Y = Length.AsMillimeters() + offEdgeMM
            };

            tokens.Add(new Route() {
                Start = start,
                End = end,
                StartDepth = depth.AsMillimeters(),
                EndDepth = depth.AsMillimeters(),
                Offset = routeOffset,
                ToolName = toolName
            });

        }

        start = new Point() {
            X = bottom.AsMillimeters(),
            Y = Length.AsMillimeters() + offEdgeMM
        };

        end = new Point() {
            X = bottom.AsMillimeters(),
            Y = -offEdgeMM
        };

        tokens.Add(new Route() {
            Start = start,
            End = end,
            StartDepth = depth.AsMillimeters(),
            EndDepth = depth.AsMillimeters(),
            Offset = routeOffset,
            ToolName = toolName
        });

        return tokens.ToArray();

    }

    private IToken[] CreateTwoRowsOfHoles(Dimension startPosition, Dimension endPosition, Dimension depth) {

        List<IToken> tokens = new() {
            new MultiBore(s_holeDiameter.AsMillimeters(),
                            new((s_holesOffEdge + ExtendFront).AsMillimeters(), startPosition.AsMillimeters()),
                            new((s_holesOffEdge + ExtendFront).AsMillimeters(), endPosition.AsMillimeters()),
                            s_holeSpacing.AsMillimeters(),
                            depth.AsMillimeters())
        };

        if (BottomNotchHeight == Dimension.Zero || endPosition > BottomNotchHeight) {
            tokens.Add(new MultiBore(s_holeDiameter.AsMillimeters(),
                                        new((Width - s_holesOffEdge - ExtendBack).AsMillimeters(), startPosition.AsMillimeters()),
                                        new((Width - s_holesOffEdge - ExtendBack).AsMillimeters(), endPosition.AsMillimeters()),
                                        s_holeSpacing.AsMillimeters(),
                                        depth.AsMillimeters()));
        } else if (startPosition < BottomNotchHeight) {
            tokens.Add(new MultiBore(s_holeDiameter.AsMillimeters(),
                                        new((Width - s_holesOffEdge - ExtendBack - BottomNotchDepth).AsMillimeters(), startPosition.AsMillimeters()),
                                        new((Width - s_holesOffEdge - ExtendBack - BottomNotchDepth).AsMillimeters(), endPosition.AsMillimeters()),
                                        s_holeSpacing.AsMillimeters(),
                                        depth.AsMillimeters()));
        } else {
            tokens.Add(new MultiBore(s_holeDiameter.AsMillimeters(),
                                        new((Width - s_holesOffEdge - ExtendBack).AsMillimeters(), startPosition.AsMillimeters()),
                                        new((Width - s_holesOffEdge - ExtendBack).AsMillimeters(), BottomNotchHeight.AsMillimeters()),
                                        s_holeSpacing.AsMillimeters(),
                                        depth.AsMillimeters()));
            tokens.Add(new MultiBore(s_holeDiameter.AsMillimeters(),
                                        new((Width - s_holesOffEdge - ExtendBack - BottomNotchDepth).AsMillimeters(), GetValidHolePositionFromBottom(Length, BottomNotchHeight).AsMillimeters()),
                                        new((Width - s_holesOffEdge - ExtendBack - BottomNotchDepth).AsMillimeters(), endPosition.AsMillimeters()),
                                        s_holeSpacing.AsMillimeters(),
                                        depth.AsMillimeters()));
        }

        return tokens.ToArray();

    }

    /// <summary>
    /// Returns a valid hole position, relative to the bottom of the panel, for a hole that is at most 'maxDistanceFromTop' distance from the top of the panel
    /// </summary>
    /// <param name="length">The length (height) of the vertical panel</param>
    /// <param name="maxDistanceFromTop">A distance measured from the top of the panel</param>
    /// <returns>A dimension which represents the distance from the bottom of the panel to the center of a valid hole position</returns>
    public static Dimension GetValidHolePositionFromTop(Dimension length, Dimension maxDistanceFromTop) {

        if (s_holeSpacing == Dimension.Zero) {
            throw new InvalidOperationException();
        }

        double holeIndex = Math.Floor(((maxDistanceFromTop + Dimension.FromMillimeters(9.5) - s_holesOffTop) / s_holeSpacing).AsMillimeters());

        var distanceFromTop = Dimension.FromMillimeters(holeIndex) * s_holeSpacing + s_holesOffTop;

        if (distanceFromTop >= length) {
            throw new InvalidOperationException();
        }

        return length - distanceFromTop;

    }

    /// <summary>
    /// Calculates the distance from the bottom of the panel to a hole which is _at most_ a distance of 'maxDistanceFromBottom' from the bottom edge of the panel and is a valid hole position (ie it is an integer multiple of s_holeSpacing relative to the top of the panel).
    /// </summary>
    public static Dimension GetValidHolePositionFromBottom(Dimension length, Dimension maxDistanceFromBottom) {

        if (length < s_holesOffTop || s_holeSpacing == Dimension.Zero) {
            throw new InvalidOperationException();
        }

        if (maxDistanceFromBottom > length - s_holesOffTop) {
            return length - s_holesOffTop;
        }

        double holeIndex = Math.Ceiling(((length - (maxDistanceFromBottom + s_holesOffBottom) - s_holesOffTop) / s_holeSpacing).AsMillimeters());

        var distanceFromTop = Dimension.FromMillimeters(holeIndex) * s_holeSpacing + s_holesOffTop;

        if (distanceFromTop >= length) {
            throw new InvalidOperationException();
        }

        var position = length - distanceFromTop;

        return position;

    }

    public record VPDrillingOperation(Dimension Start, Dimension End, VPDrillingDepth Depth);

    public enum VPDrillingDepth {
        Stopped,
        Through
    }

}
