using ApplicationCore.Features.Orders.OrderExport.Handlers.ExtExport.Contracts;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Exceptions;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Shared.Domain;
using CADCodeProxy.Enums;
using CADCodeProxy.Machining;

namespace ApplicationCore.Features.Orders.Shared.Domain.Products;

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

    public ClosetVerticalDrillingType DrillingType { get; }
    public Dimension ExtendBack { get; }
    public Dimension ExtendFront { get; }
    public Dimension HoleDimensionFromBottom { get; }
    public Dimension HoleDimensionFromTop { get; }
    public Dimension TransitionHoleDimensionFromBottom { get; }
    public Dimension TransitionHoleDimensionFromTop { get; }
    public Dimension BottomNotchDepth { get; }
    public Dimension BottomNotchHeight { get; }

    // TODO: Implement this 
    public bool EdgeBandTop { get; } = false;

    private bool _requiresCustomDrilling;
    private static readonly Dimension s_holeSpacing = Dimension.FromMillimeters(32);
    private static readonly Dimension s_holesOffEdge = Dimension.FromMillimeters(37);
    private static readonly Dimension s_holesOffTop = Dimension.FromMillimeters(9.5);
    private static readonly Dimension s_holeDiameter = Dimension.FromMillimeters(5);
    private static readonly Dimension s_stoppedDepth = Dimension.FromMillimeters(16.5);
    private static readonly Dimension s_drillThroughDepth = Dimension.FromMillimeters(26);
    private static readonly string s_cutOutTool = "3-8Comp";
    private static readonly Dimension s_panelThickness = Dimension.FromMillimeters(19.05);

    public string GetDescription() => "Closet Part - Custom Drilled Vertical Panel";

    public CustomDrilledVerticalPanel(Guid id, int qty, decimal unitPrice, int productNumber, string room, Dimension width, Dimension length, ClosetMaterial material, ClosetPaint? paint, string edgeBandingColor, string comment,
                                    ClosetVerticalDrillingType drillingType, Dimension extendBack, Dimension extendFront, Dimension holeDimensionFromBottom, Dimension holeDimensionFromTop, Dimension transitionHoleDimensionFromBottom, Dimension transitionHoleDimensionFromTop, Dimension bottomNotchDepth, Dimension bottomNotchHeight) {
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

        if (DrillingType == ClosetVerticalDrillingType.DrilledThrough) {
            SKU = "PC";
        } else {
            SKU = "PE";
        }

        if (((holeDimensionFromBottom == Dimension.Zero && holeDimensionFromTop == Dimension.Zero)
            || (transitionHoleDimensionFromBottom == Dimension.Zero && transitionHoleDimensionFromTop == Dimension.Zero))
            // PSI does not handle custom drilling w/ bottom notch correctly 
            && !((holeDimensionFromBottom != Dimension.Zero || holeDimensionFromTop != Dimension.Zero || transitionHoleDimensionFromBottom != Dimension.Zero || transitionHoleDimensionFromTop != Dimension.Zero) && BottomNotchHeight != Dimension.Zero)) {
            _requiresCustomDrilling = false;
        } else {
            _requiresCustomDrilling = true;
        }

        // If BOTH holes from top and holes from bottom are zero than EITHER transition holes from top can be greater than holes from top and transition holes from bottom can be greater than holes from bottom
        // If holes from top OR holes from bottom is not zero than BOTH transition holes from top must be less than or equal to holes from top and transition holes from bottom must be less than or equal to holes from bottom
        if ((HoleDimensionFromTop != Dimension.Zero || HoleDimensionFromBottom != Dimension.Zero)
            && (TransitionHoleDimensionFromTop > HoleDimensionFromTop || TransitionHoleDimensionFromBottom > HoleDimensionFromBottom)) {
            throw new InvalidProductOptionsException("Invalid hole dimensions, transition holes cannot exceed standard holes without flipping panel");
        }

        if ((BottomNotchDepth != Dimension.Zero && BottomNotchHeight == Dimension.Zero)
            || (BottomNotchDepth == Dimension.Zero && BottomNotchHeight != Dimension.Zero)) {
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
            ["FinLeft"] = DrillingType == ClosetVerticalDrillingType.FinishedLeft ? "Y" : "N",
            ["FinRight"] = DrillingType == ClosetVerticalDrillingType.FinishedRight ? "Y" : "N",
            ["ExtendBack"] = ExtendBack.AsMillimeters().ToString(),
            ["ExtendFront"] = ExtendFront.AsMillimeters().ToString(),
            ["HoleDimFromBot"] = HoleDimensionFromBottom.AsMillimeters().ToString(),
            ["HoleDimFromTop"] = HoleDimensionFromTop.AsMillimeters().ToString(),
            ["BottomNotchD"] = BottomNotchDepth.AsMillimeters().ToString(),
            ["BottomNotchH"] = BottomNotchHeight.AsMillimeters().ToString(),
            ["TransHoleBot"] = TransitionHoleDimensionFromBottom.AsMillimeters().ToString(),
            ["TransHoleTop"] = TransitionHoleDimensionFromTop.AsMillimeters().ToString(),
            ["ProductWidth"] = Width.AsMillimeters().ToString(),
            ["ProductLength"] = Length.AsMillimeters().ToString()
        };

        return new List<PPProduct>() { new PPProduct(Id, Qty, Room, SKU, ProductNumber, "Royal_c", materialType, "slab", "standard", Comment, finishMaterials, ebMaterials, parameters, new Dictionary<string, string>(), new Dictionary<string, string>()) };
    }

    public bool ContainsCNCParts() => _requiresCustomDrilling;

    public IEnumerable<Part> GetCNCParts() {

        if (!_requiresCustomDrilling) {
            return Enumerable.Empty<Part>();
        }

        List<IToken> tokens = new();

        if (Width > Dimension.FromInches(12)) {
            Dimension depth = DrillingType == ClosetVerticalDrillingType.DrilledThrough ? s_drillThroughDepth : s_stoppedDepth;
            tokens.Add(new Bore(s_holeDiameter.AsMillimeters(),
                                new((Width - Dimension.FromInches(12) + s_holesOffEdge).AsMillimeters(), (Length - s_holesOffTop).AsMillimeters()),
                                depth.AsMillimeters()));
        }

        if (HoleDimensionFromBottom == Dimension.Zero && HoleDimensionFromTop == Dimension.Zero) {
            Dimension stoppedStartHeight = Length - s_holesOffTop - TransitionHoleDimensionFromTop;
            Dimension stoppedEndHeight = TransitionHoleDimensionFromBottom;
            if (stoppedStartHeight > stoppedEndHeight) {
                tokens.AddRange(CreateTwoRowsOfHoles(GetValidHolePositionFromTop(Length, stoppedStartHeight),
                                                    stoppedEndHeight,
                                                    s_stoppedDepth));
            } 
        } else {
            if (HoleDimensionFromTop > Dimension.Zero) {
                Dimension transEnd = GetValidHolePositionFromTop(Length, TransitionHoleDimensionFromTop);
                Dimension start = Length - s_holesOffTop;
                if (transEnd > Dimension.Zero) {
                    start =  transEnd - s_holeSpacing;
                }
                tokens.AddRange(CreateTwoRowsOfHoles(start,
                                                    Length - HoleDimensionFromTop,
                                                    s_stoppedDepth));
            }
            
            if (HoleDimensionFromBottom > Dimension.Zero) {
                Dimension transStart = GetValidHolePositionFromBottom(Length, TransitionHoleDimensionFromBottom);
                Dimension end = transStart;
                if (transStart > Dimension.Zero) {
                    end += s_holeSpacing;
                }
                tokens.AddRange(CreateTwoRowsOfHoles(GetValidHolePositionFromBottom(Length, HoleDimensionFromBottom),
                                                    end,
                                                    s_stoppedDepth));
            }
        }

        if (TransitionHoleDimensionFromTop > Dimension.Zero) {
             tokens.AddRange(CreateTwoRowsOfHoles(Length - s_holesOffTop,
                                                Length - TransitionHoleDimensionFromTop,
                                                s_drillThroughDepth));
        }
        
        if (TransitionHoleDimensionFromBottom > Dimension.Zero) {
            tokens.AddRange(CreateTwoRowsOfHoles(GetValidHolePositionFromBottom(Length, TransitionHoleDimensionFromBottom),
                                                Dimension.Zero,
                                                s_drillThroughDepth));
        }

        if (BottomNotchHeight > Dimension.Zero && BottomNotchHeight > Dimension.Zero) {
            tokens.Add(new Route() {
                ToolName = s_cutOutTool,
                StartDepth = s_panelThickness.AsMillimeters(),
                EndDepth = s_panelThickness.AsMillimeters(),
                Offset = Offset.Right,
                Start = new((Width - BottomNotchDepth).AsMillimeters(),0),
                End = new((Width - BottomNotchDepth).AsMillimeters(),BottomNotchHeight.AsMillimeters())
            });
            tokens.Add(new Route() {
                ToolName = s_cutOutTool,
                StartDepth = s_panelThickness.AsMillimeters(),
                EndDepth = s_panelThickness.AsMillimeters(),
                Offset = Offset.Right,
                Start = new((Width - BottomNotchDepth).AsMillimeters(),BottomNotchHeight.AsMillimeters()),
                End = new(Width.AsMillimeters(),BottomNotchHeight.AsMillimeters())
            });
        }

        var edgeBanding = new EdgeBanding(EdgeBandingColor, "PVC");
        var topEdgeBanding = EdgeBandTop ? new EdgeBanding(EdgeBandingColor, "PVC") : new("", "");

        var part = new Part() {
            Width = Width.AsMillimeters(),
            Length = Length.AsMillimeters(),
            Thickness = s_panelThickness.AsMillimeters(),
            Material = $"{Material.Finish} Mela {Material.Core}",
            Name = "Custom Vertical Panel",
            IsGrained = true,
            Qty = Qty,
            PrimaryFace = new() {
                ProgramName = $"{SKU}{ProductNumber}",
                IsRotated = true,
                IsMirrored = DrillingType == ClosetVerticalDrillingType.FinishedRight,
                Tokens = tokens.ToArray()
            },
            Width1Banding = edgeBanding,
            Length1Banding = topEdgeBanding
        };


        return new Part[] { part };

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

    // Returns hole position relative to the bottom of the panel 
    public static Dimension GetValidHolePositionFromTop(Dimension length, Dimension maxSpaceFromTop) {

        if (maxSpaceFromTop < s_holesOffTop || s_holeSpacing == Dimension.Zero) {
            return Dimension.Zero;
        }

        double holeIndex = Math.Floor(((maxSpaceFromTop - s_holesOffTop) / s_holeSpacing).AsMillimeters());

        var distanceFromTop = Dimension.FromMillimeters(holeIndex) * s_holeSpacing + s_holesOffTop;

        if (distanceFromTop >= length) {
            return Dimension.Zero;
        }

        return length - distanceFromTop;

    }

    // Returns hole position relative to the bottom of the panel
    public static Dimension GetValidHolePositionFromBottom(Dimension length, Dimension maxSpaceFromBottom) {

        if (length < s_holesOffTop || maxSpaceFromBottom > length - s_holesOffTop || s_holeSpacing == Dimension.Zero) {
            return Dimension.Zero;
        }

        double holeIndex = Math.Ceiling(((length - maxSpaceFromBottom - s_holesOffTop) / s_holeSpacing).AsMillimeters());

        var distanceFromTop = (Dimension.FromMillimeters(holeIndex) * s_holeSpacing + s_holesOffTop);

        if (distanceFromTop >= length) {
            return Dimension.Zero;
        }

        var position = length - distanceFromTop;
        
        return position;

    }

    public IEnumerable<Supply> GetSupplies() => Enumerable.Empty<Supply>(); 

}
