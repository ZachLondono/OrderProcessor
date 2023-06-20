using ApplicationCore.Features.Orders.OrderExport.Handlers.ExtExport.Contracts;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Shared.Domain;
using CADCodeProxy.Machining;

namespace ApplicationCore.Features.Orders.Shared.Domain.Products;

public class CustomDrilledVerticalPanel : IProduct, IPPProductContainer, ICNCPartContainer {

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

    private bool _requiresCustomDrilling;

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

        if (holeDimensionFromBottom == Dimension.Zero && holeDimensionFromTop == Dimension.Zero) {
            _requiresCustomDrilling = false;
        } else if (transitionHoleDimensionFromBottom != Dimension.Zero && transitionHoleDimensionFromTop != Dimension.Zero) {
            _requiresCustomDrilling = true;
        }

        // If BOTH holes from top and holes from bottom are zero than EITHER transition holes from top can be greater than holes from top and transition holes from bottom can be greater than holes from bottom
        // If holes from top OR holes from bottom is not zero than BOTH transition holes from top must be less than or equal to holes from top and transition holes from bottom must be less than or equal to holes from bottom
        if ((HoleDimensionFromTop != Dimension.Zero || HoleDimensionFromBottom != Dimension.Zero)
            && (TransitionHoleDimensionFromTop > HoleDimensionFromTop || TransitionHoleDimensionFromBottom > HoleDimensionFromBottom)) {
            throw new ArgumentException("Invalid parameters - requires flipping part");
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

    public IEnumerable<Part> GetParts() {

        if (!_requiresCustomDrilling) {
            return Enumerable.Empty<Part>();
        }

        List<IToken> tokens = new();

        Dimension stoppedDepth = Dimension.FromMillimeters(16.5);
        Dimension drillThroughDepth = Dimension.FromMillimeters(19.5);
        Dimension spacing = Dimension.FromMillimeters(32);
        Dimension offTop = Dimension.FromMillimeters(9.5);
        Dimension offEdge = Dimension.FromMillimeters(9.5);
        Dimension holeDiameter = Dimension.FromMillimeters(5);

        if (HoleDimensionFromBottom == Dimension.Zero && HoleDimensionFromTop == Dimension.Zero) {
            Dimension stoppedStartHeight = Length - offTop - TransitionHoleDimensionFromTop;
            Dimension stoppedEndHeight = TransitionHoleDimensionFromBottom;
            if (stoppedStartHeight > stoppedEndHeight) {
                tokens.Add(new MultiBore(holeDiameter.AsMillimeters(),
                                        new(offEdge.AsMillimeters(), stoppedStartHeight.AsMillimeters()),
                                        new(offEdge.AsMillimeters(), stoppedEndHeight.AsMillimeters()),
                                        spacing.AsMillimeters(),
                                        stoppedDepth.AsMillimeters()));
            } 
        } else {
            if (HoleDimensionFromTop > Dimension.Zero) {
                Dimension topStoppedStart = Length - offTop - TransitionHoleDimensionFromTop;
                Dimension topStoppedEnd = HoleDimensionFromTop;
                tokens.Add(new MultiBore(holeDiameter.AsMillimeters(),
                                        new(offEdge.AsMillimeters(), topStoppedStart.AsMillimeters()),
                                        new(offEdge.AsMillimeters(), topStoppedEnd.AsMillimeters()),
                                        spacing.AsMillimeters(),
                                        stoppedDepth.AsMillimeters()));
            }
            
            if (HoleDimensionFromBottom > Dimension.Zero) {
                Dimension bottomStoppedStart = HoleDimensionFromBottom - TransitionHoleDimensionFromBottom;
                Dimension bottomStoppedEnd = TransitionHoleDimensionFromBottom;
                tokens.Add(new MultiBore(holeDiameter.AsMillimeters(),
                                        new(offEdge.AsMillimeters(), bottomStoppedStart.AsMillimeters()),
                                        new(offEdge.AsMillimeters(), bottomStoppedEnd.AsMillimeters()),
                                        spacing.AsMillimeters(),
                                        stoppedDepth.AsMillimeters()));
            }
        }

        if ((Length - TransitionHoleDimensionFromTop) > TransitionHoleDimensionFromBottom) {
            if (TransitionHoleDimensionFromTop > Dimension.Zero) {
                tokens.Add(new MultiBore(holeDiameter.AsMillimeters(),
                                        new(offEdge.AsMillimeters(), (Length - offTop ).AsMillimeters()),
                                        new(offEdge.AsMillimeters(), ((Length - offTop) - TransitionHoleDimensionFromTop).AsMillimeters()),
                                        spacing.AsMillimeters(),
                                        drillThroughDepth.AsMillimeters()));
            }

            if (TransitionHoleDimensionFromBottom > Dimension.Zero) {
                tokens.Add(new MultiBore(holeDiameter.AsMillimeters(),
                                        new(offEdge.AsMillimeters(), TransitionHoleDimensionFromBottom.AsMillimeters()),
                                        new(offEdge.AsMillimeters(), 0),
                                        spacing.AsMillimeters(),
                                        drillThroughDepth.AsMillimeters()));
            }
        } else {
            tokens.Add(new MultiBore(holeDiameter.AsMillimeters(),
                                    new(offEdge.AsMillimeters(), (Length - offTop).AsMillimeters()),
                                    new(offEdge.AsMillimeters(), 0),
                                    spacing.AsMillimeters(),
                                    drillThroughDepth.AsMillimeters()));
        }

        var part = new Part() {
            Width = Width.AsMillimeters(),
            Length = Length.AsMillimeters(),
            Thickness = 19,
            Material = $"{Material.Finish} Mela {Material.Core}",
            Name = "Custom Vertical Panel",
            IsGrained = true,
            Qty = Qty,
            PrimaryFace = new() {
                ProgramName = $"{SKU}{ProductNumber}",
                Tokens = tokens.ToArray()
            }
        };

        return new Part[] { part };

    }

    public IEnumerable<Supply> GetSupplies() => Enumerable.Empty<Supply>(); 

}
