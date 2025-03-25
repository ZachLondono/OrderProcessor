using Domain.Orders.ValueObjects;
using Domain.ValueObjects;
using CADCodeProxy.Enums;
using CADCodeProxy.Machining;
using CADCodeProxy.Machining.Tokens;

namespace Domain.Orders.Components;

public class DoweledDrawerBox : DoweledDrawerBoxConfig, IComponent {

    public int Qty { get; }
    public Dimension Height { get; }
    public Dimension Width { get; }
    public Dimension Depth { get; }

    public string BoxSize { get; }

    // TODO: add a note property

    public DoweledDrawerBox(int qty, Dimension height, Dimension width, Dimension depth,
                            DoweledDrawerBoxMaterial front, DoweledDrawerBoxMaterial back, DoweledDrawerBoxMaterial sides, DoweledDrawerBoxMaterial bottom,
                            bool machineForUM, Dimension frontBackHeightAdjustment, string umNotch)
                            : base(front, back, sides, bottom, machineForUM, frontBackHeightAdjustment, umNotch) {
        Qty = qty;
        Height = height;
        Width = width;
        Depth = depth;

        var h = FormatDimension(Height);
        var w = FormatDimension(Width);
        var d = FormatDimension(Depth);

        BoxSize = $"{h}\"H x {w}\"W x {d}\"D"; 

    }

    public bool ContainsCNCParts() => true;

    public virtual IEnumerable<Part> GetCNCParts() {
        // TODO: maybe pass in the construction object to this method
        // TODO: make the bellow methods static, maybe
        yield return GetFrontPart(Construction);
        yield return GetBackPart(Construction);
        var (left, right) = GetSideParts(Construction);
        yield return left;
        yield return right;
        yield return GetBottomPart(Construction);
    }

    public virtual Part GetFrontPart(DoweledDrawerBoxConstruction construction) {

        Dimension frontLength = Width - 2 * SideMaterial.Thickness - construction.WidthUndersize;

        List<IToken> tokens = new();
        tokens.AddRange(CreateBottomDadoTokens(construction, frontLength, construction.FrontBackBottomDadoLengthOversize.AsMillimeters()));

        if (FrontMaterial.Thickness > construction.UMSlideMaxDistanceOffOutsideFace
            && MachineThicknessForUMSlides) {

            tokens.Add(new Pocket() {
                CornerA = new(0, 0),
                CornerD = new(0, construction.BottomDadoStartHeight.AsMillimeters()),
                CornerC = new(frontLength.AsMillimeters(), 0),
                CornerB = new(frontLength.AsMillimeters(), construction.BottomDadoStartHeight.AsMillimeters()),
                StartDepth = (FrontMaterial.Thickness - construction.UMSlideMaxDistanceOffOutsideFace).AsMillimeters(),
                EndDepth = (FrontMaterial.Thickness - construction.UMSlideMaxDistanceOffOutsideFace).AsMillimeters(),
                ToolName = construction.UMSlidePocketToolName
            });

        }

        return new Part() {
            Qty = Qty,
            Width = (Height - FrontBackHeightAdjustment).AsMillimeters(),
            Length = frontLength.AsMillimeters(),
            Thickness = FrontMaterial.Thickness.AsMillimeters(),
            Material = FrontMaterial.ToPSIMaterial().GetLongName(),
            IsGrained = FrontMaterial.IsGrained,
            InfoFields = new() {
                { "ProductName", "Front" },
                { "Description", "Drawer Box Front" },
                { "Side1Color", FrontMaterial.Name },
                { "Side1Material", "" },
                { "Comment1", $"Box Size: {BoxSize}" },
            },
            PrimaryFace = new() {
                ProgramName = "Front",
                Tokens = tokens.ToArray()
            },
            Length1Banding = new(FrontMaterial.Name, "PVC")
        };

    }

    public virtual Part GetBackPart(DoweledDrawerBoxConstruction construction) {

        Dimension backLength = Width - 2 * SideMaterial.Thickness - construction.WidthUndersize;

        List<IToken> tokens = new();
        tokens.AddRange(CreateBottomDadoTokens(construction, backLength, construction.FrontBackBottomDadoLengthOversize.AsMillimeters()));

        return new Part() {
            Qty = Qty,
            Width = (Height - FrontBackHeightAdjustment).AsMillimeters(),
            Length = backLength.AsMillimeters(),
            Thickness = BackMaterial.Thickness.AsMillimeters(),
            Material = BackMaterial.ToPSIMaterial().GetLongName(),
            IsGrained = BackMaterial.IsGrained,
            InfoFields = new() {
                { "ProductName", "Back" },
                { "Description", "Drawer Box Back" },
                { "Side1Color", BackMaterial.Name },
                { "Side1Material", "" },
                { "Comment1", $"Box Size: {BoxSize}" },
            },
            PrimaryFace = new() {
                ProgramName = "Back",
                Tokens = tokens.ToArray()
            },
            Length1Banding = new(FrontMaterial.Name, "PVC")
        };

    }

    public virtual (Part Left, Part Right) GetSideParts(DoweledDrawerBoxConstruction construction) {

        Dimension dadoLengthAdj = (FrontMaterial.Thickness < BackMaterial.Thickness ? FrontMaterial : BackMaterial).Thickness - construction.BottomDadoDepth;

        List<IToken> tokens = new();
        tokens.AddRange(CreateBottomDadoTokens(construction, Depth, -1 * dadoLengthAdj.AsMillimeters()));

        var positions = GetDowelPositions(construction.DowelPositionsByHeight, Height);
        foreach (var pos in positions) {
            tokens.Add(new Bore(construction.DowelDiameter.AsMillimeters(), new((SideMaterial.Thickness / 2).AsMillimeters(), pos.AsMillimeters()), construction.DowelDepth.AsMillimeters()));
            tokens.Add(new Bore(construction.DowelDiameter.AsMillimeters(), new((Depth - SideMaterial.Thickness / 2).AsMillimeters(), pos.AsMillimeters()), construction.DowelDepth.AsMillimeters()));
        }

        if (SideMaterial.Thickness > construction.UMSlideMaxDistanceOffOutsideFace
            && MachineThicknessForUMSlides) {

            tokens.Add(new Pocket() {
                CornerA = new(FrontMaterial.Thickness.AsMillimeters(), 0),
                CornerD = new(FrontMaterial.Thickness.AsMillimeters(), construction.BottomDadoStartHeight.AsMillimeters()),
                CornerC = new(Depth.AsMillimeters() + 4, 0),
                CornerB = new(Depth.AsMillimeters() + 4, construction.BottomDadoStartHeight.AsMillimeters()),
                StartDepth = (SideMaterial.Thickness - construction.UMSlideMaxDistanceOffOutsideFace).AsMillimeters(),
                EndDepth = (SideMaterial.Thickness - construction.UMSlideMaxDistanceOffOutsideFace).AsMillimeters(),
                ToolName = construction.UMSlidePocketToolName
            });

        }

        var left = new Part() {
            Qty = Qty,
            Width = Height.AsMillimeters(),
            Length = Depth.AsMillimeters(),
            Thickness = SideMaterial.Thickness.AsMillimeters(),
            Material = SideMaterial.ToPSIMaterial().GetLongName(),
            IsGrained = SideMaterial.IsGrained,
            InfoFields = new() {
                { "ProductName", "Left" },
                { "Description", "Drawer Box Left Side" },
                { "Side1Color", SideMaterial.Name },
                { "Side1Material", "" },
                { "Comment1", $"Box Size: {BoxSize}" },
            },
            PrimaryFace = new() {
                ProgramName = "Left",
                IsMirrored = true,
                Tokens = tokens.ToArray()
            },
            Length1Banding = new(FrontMaterial.Name, "PVC"),
            Width1Banding = new(FrontMaterial.Name, "PVC"),
            Width2Banding = new(FrontMaterial.Name, "PVC")
        };

        var right = new Part() {
            Qty = Qty,
            Width = Height.AsMillimeters(),
            Length = Depth.AsMillimeters(),
            Thickness = SideMaterial.Thickness.AsMillimeters(),
            Material = SideMaterial.ToPSIMaterial().GetLongName(),
            IsGrained = SideMaterial.IsGrained,
            InfoFields = new() {
                { "ProductName", "Right" },
                { "Description", "Drawer Box Right Side" },
                { "Side1Color", SideMaterial.Name },
                { "Side1Material", "" },
                { "Comment1", $"Box Size: {BoxSize}" },
            },
            PrimaryFace = new() {
                ProgramName = "Right",
                Tokens = tokens.ToArray()
            },
            Length1Banding = new(FrontMaterial.Name, "PVC"),
            Width1Banding = new(FrontMaterial.Name, "PVC"),
            Width2Banding = new(FrontMaterial.Name, "PVC")

        };

        return (left, right);

    }

    public virtual IEnumerable<IToken> CreateBottomDadoTokens(DoweledDrawerBoxConstruction construction, Dimension partLength, double offEdgeMM, bool mirror = false) {

        var routeOffset = Offset.Right;
        if (mirror) {
            routeOffset = Offset.Left;
        }

        var height = BottomMaterial.Thickness + construction.BottomDadoHeightOversize;
        var bottom = construction.BottomDadoStartHeight;
        var top = height + bottom;

        string toolName;
        Dimension toolDiameter;
        if (height >= construction.LargeBottomDadoToolMinimum) {
            toolName = construction.LargeBottomDadoToolName;
            toolDiameter = construction.LargeBottomDadoToolDiameter;
        } else {
            toolName = construction.SmallBottomDadoToolName;
            toolDiameter = construction.SmallBottomDadoToolDiameter;
        }

        var passCount = double.Ceiling(height / toolDiameter);
        var passDistance = height / passCount;

        List<IToken> tokens = new();

        Point start;
        Point end;

        for (int i = 0; i < passCount - 1; i++) {

            start = new Point() {
                X = -offEdgeMM,
                Y = (top - i * passDistance).AsMillimeters()
            };

            end = new Point() {
                X = partLength.AsMillimeters() + offEdgeMM,
                Y = (top - i * passDistance).AsMillimeters()
            };


            tokens.Add(new Route() {
                Start = start,
                End = end,
                StartDepth = construction.BottomDadoScoringDepth.AsMillimeters(),
                EndDepth = construction.BottomDadoScoringDepth.AsMillimeters(),
                Offset = routeOffset,
                ToolName = toolName
            });

            tokens.Add(new Route() {
                Start = start,
                End = end,
                StartDepth = construction.BottomDadoDepth.AsMillimeters(),
                EndDepth = construction.BottomDadoDepth.AsMillimeters(),
                Offset = routeOffset,
                ToolName = toolName
            });

        }

        start = new Point() {
            X = partLength.AsMillimeters() + offEdgeMM,
            Y = bottom.AsMillimeters()
        };

        end = new Point() {
            X = -offEdgeMM,
            Y = bottom.AsMillimeters()
        };

        tokens.Add(new Route() {
            Start = start,
            End = end,
            StartDepth = construction.BottomDadoScoringDepth.AsMillimeters(),
            EndDepth = construction.BottomDadoScoringDepth.AsMillimeters(),
            Offset = routeOffset,
            ToolName = toolName
        });

        tokens.Add(new Route() {
            Start = start,
            End = end,
            StartDepth = construction.BottomDadoDepth.AsMillimeters(),
            EndDepth = construction.BottomDadoDepth.AsMillimeters(),
            Offset = routeOffset,
            ToolName = toolName
        });

        return tokens;


    }

    public virtual Part GetBottomPart(DoweledDrawerBoxConstruction construction) {
        var bottom = GetBottom(construction);
        return new Part() {
            Qty = bottom.Qty,
            Width = bottom.Width.AsMillimeters(),
            Length = bottom.Length.AsMillimeters(),
            Thickness = bottom.Material.Thickness.AsMillimeters(),
            Material = bottom.Material.ToPSIMaterial().GetLongName(),
            IsGrained = bottom.Material.IsGrained,
            InfoFields = new() {
                { "ProductName", "Bottom" },
                { "Description", "Drawer Box Bottom" },
                { "Side1Color", bottom.Material.Name },
                { "Side1Material", "" },
                { "Comment1", $"Box Size: {BoxSize}" },
            },
            PrimaryFace = new() {
                ProgramName = "Bottom",
                Tokens = Array.Empty<IToken>()
            }
        };
    }

    public static Dimension[] GetDowelPositions(IDictionary<Dimension, Dimension[]> dowelPositionsByHeight, Dimension boxHeight) {

        foreach (var kv in dowelPositionsByHeight.OrderBy(kv => kv.Key)) {

            if (boxHeight <= kv.Key) {
                return kv.Value;
            }

        }

        throw new InvalidOperationException("Invalid drawer box height");
    }

    public DoweledDrawerBoxBottom GetBottom(DoweledDrawerBoxConstruction construction) {

        var width = (Width - 2 * SideMaterial.Thickness - construction.BottomUndersize + 2 * construction.BottomDadoDepth);
        var length = (Depth - FrontMaterial.Thickness - BackMaterial.Thickness - construction.BottomUndersize + 2 * construction.BottomDadoDepth);

        return new(Qty, width, length, BottomMaterial);

    }

    private static string FormatDimension(Dimension dim) {

        var fraction = dim.RoundToInchMultiple((double)1 / 32).AsInchFraction();

        if (fraction.N == 0) {
            return "0";
        }

        int whole = fraction.N / fraction.D;
        int n = fraction.N - whole * fraction.D;

        if (whole == 0) {

            return $"{n}/{fraction.D}";

        } else if (n != 0) {

            return $"{whole}-{n}/{fraction.D}";

        } else {

            return whole.ToString();

        }

    }

    public static DoweledDrawerBoxConstruction Construction = new() {
        DowelPositionsByHeight = new Dictionary<Dimension, Dimension[]> {

                { Dimension.FromMillimeters(63), new Dimension[] {
                    Dimension.FromMillimeters(30),
                    Dimension.FromMillimeters(45)
                } },

                { Dimension.FromMillimeters(76), new Dimension[] {
                    Dimension.FromMillimeters(36),
                    Dimension.FromMillimeters(48)
                } },

                { Dimension.FromMillimeters(101), new Dimension[] {
                    Dimension.FromMillimeters(36),
                    Dimension.FromMillimeters(60)
                } },

                { Dimension.FromMillimeters(127), new Dimension[] {
                    Dimension.FromMillimeters(36),
                    Dimension.FromMillimeters(86)
                } },

                { Dimension.FromMillimeters(152), new Dimension[] {
                    Dimension.FromMillimeters(36),
                    Dimension.FromMillimeters(111)
                } },

                { Dimension.FromMillimeters(178), new Dimension[] {
                    Dimension.FromMillimeters(36),
                    Dimension.FromMillimeters(136)
                } },

                { Dimension.FromMillimeters(203), new Dimension[] {
                    Dimension.FromMillimeters(36),
                    Dimension.FromMillimeters(162)
                } },

                { Dimension.FromMillimeters(228), new Dimension[] {
                    Dimension.FromMillimeters(36),
                    Dimension.FromMillimeters(112),
                    Dimension.FromMillimeters(187)
                } },

                { Dimension.FromMillimeters(254), new Dimension[] {
                    Dimension.FromMillimeters(36),
                    Dimension.FromMillimeters(125),
                    Dimension.FromMillimeters(213)
                } },

                { Dimension.FromMillimeters(279), new Dimension[] {
                    Dimension.FromMillimeters(36),
                    Dimension.FromMillimeters(137),
                    Dimension.FromMillimeters(238)
                } },

                { Dimension.FromMillimeters(305), new Dimension[] {
                    Dimension.FromMillimeters(36),
                    Dimension.FromMillimeters(150),
                    Dimension.FromMillimeters(263)
                } },

                { Dimension.FromMillimeters(330), new Dimension[] {
                    Dimension.FromMillimeters(36),
                    Dimension.FromMillimeters(163),
                    Dimension.FromMillimeters(289)
                } },

                { Dimension.FromMillimeters(355), new Dimension[] {
                    Dimension.FromMillimeters(36),
                    Dimension.FromMillimeters(129),
                    Dimension.FromMillimeters(222),
                    Dimension.FromMillimeters(314)
                } },

                { Dimension.FromMillimeters(393), new Dimension[] {
                    Dimension.FromMillimeters(36),
                    Dimension.FromMillimeters(137),
                    Dimension.FromMillimeters(238),
                    Dimension.FromMillimeters(340)
                } },

                { Dimension.FromMillimeters(432), new Dimension[] {
                    Dimension.FromMillimeters(36),
                    Dimension.FromMillimeters(150),
                    Dimension.FromMillimeters(264),
                    Dimension.FromMillimeters(378)
                } },

                { Dimension.FromMillimeters(470), new Dimension[] {
                    Dimension.FromMillimeters(36),
                    Dimension.FromMillimeters(131),
                    Dimension.FromMillimeters(226),
                    Dimension.FromMillimeters(321),
                    Dimension.FromMillimeters(416)
                } },

                { Dimension.FromMillimeters(508), new Dimension[] {
                    Dimension.FromMillimeters(36),
                    Dimension.FromMillimeters(141),
                    Dimension.FromMillimeters(246),
                    Dimension.FromMillimeters(351),
                    Dimension.FromMillimeters(454)
                } },

                { Dimension.FromMillimeters(546), new Dimension[] {
                    Dimension.FromMillimeters(36),
                    Dimension.FromMillimeters(150),
                    Dimension.FromMillimeters(264),
                    Dimension.FromMillimeters(378),
                    Dimension.FromMillimeters(492)
                } },

                { Dimension.FromMillimeters(584), new Dimension[] {
                    Dimension.FromMillimeters(36),
                    Dimension.FromMillimeters(150),
                    Dimension.FromMillimeters(284),
                    Dimension.FromMillimeters(408),
                    Dimension.FromMillimeters(530)
                } },

                { Dimension.FromMillimeters(635), new Dimension[] {
                    Dimension.FromMillimeters(36),
                    Dimension.FromMillimeters(169),
                    Dimension.FromMillimeters(302),
                    Dimension.FromMillimeters(435),
                    Dimension.FromMillimeters(568)
                } },

                { Dimension.FromMillimeters(999), new Dimension[] {
                    Dimension.FromMillimeters(36),
                    Dimension.FromMillimeters(182),
                    Dimension.FromMillimeters(328),
                    Dimension.FromMillimeters(474),
                    Dimension.FromMillimeters(619)
                } }

            },
        DowelDepth = Dimension.FromMillimeters(10),
        DowelDiameter = Dimension.FromMillimeters(8),
        SmallBottomDadoToolName = "1-4Strt",
        SmallBottomDadoToolDiameter = Dimension.FromMillimeters(6.35),
        LargeBottomDadoToolName = "1-2Dado",
        LargeBottomDadoToolDiameter = Dimension.FromMillimeters(12.7),
        LargeBottomDadoToolMinimum = Dimension.FromMillimeters(12.7),
        BottomDadoScoringDepth = Dimension.FromMillimeters(1.5),
        BottomDadoStartHeight = Dimension.FromInches(0.5),
        BottomDadoDepth = Dimension.FromMillimeters(6.5),
        BottomDadoHeightOversize = Dimension.FromMillimeters(0.75),
        FrontBackBottomDadoLengthOversize = Dimension.FromMillimeters(3),
        BottomUndersize = Dimension.FromMillimeters(1),
        UMSlidePocketToolName = "Pocket9",
        UMSlidePocketDiameter = Dimension.FromMillimeters(9),
        UMSlideMaxDistanceOffOutsideFace = Dimension.FromInches(5.0 / 8.0),
        WidthUndersize = Dimension.FromMillimeters(0.5)
    };

}
