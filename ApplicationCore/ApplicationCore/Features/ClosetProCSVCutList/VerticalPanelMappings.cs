using ApplicationCore.Features.ClosetProCSVCutList.CSVModels;
using ApplicationCore.Features.ClosetProCSVCutList.Products;
using Domain.ValueObjects;

namespace ApplicationCore.Features.ClosetProCSVCutList;

public partial class ClosetProPartMapper {

    public static IClosetProProduct CreateVerticalPanelFromPart(Part part, bool wallHasBacking, RoomNamingStrategy strategy) {

        if (part.PartName == "Vertical Panel - Island") {
            return CreateIslandVerticalPanel(part, strategy);
        }

        double leftDrilling = part.VertDrillL;
        double rightDrilling = part.VertDrillR;
        bool isTransition = leftDrilling != 0 && rightDrilling != 0 && leftDrilling != rightDrilling;

        if (isTransition) {

            return CreateTransitionVerticalPanel(part, wallHasBacking, strategy);

        } else {

            return CreateVerticalPanel(part, wallHasBacking, strategy);

        }

    }

    public static VerticalPanel CreateVerticalPanel(Part part, bool wallHasBacking, RoomNamingStrategy strategy) {

        double leftDrilling = part.VertDrillL;
        double rightDrilling = part.VertDrillR;

        bool isWallMount = part.ExportName.Contains("WM");

        bool hasRadiusBottom = part.ExportName.Contains("Radius");

        if (!TryParseMoneyString(part.PartCost, out decimal unitPrice)) {
            unitPrice = 0M;
        }
        string room = GetRoomName(part, strategy);

        Dimension depth = Dimension.FromInches(part.Depth);
        Dimension height = Dimension.FromInches(part.Height);
        string edgeBandingColor = part.InfoRecords
                                        .Where(i => i.PartName == "Edge Banding") // i.CornerShelfSizes contains the information about what edges to apply banding
                                        .Select(i => i.Color)
                                        .FirstOrDefault() ?? part.Color;

        bool finLeft = part.VertHand == "L";
        bool finRight = part.VertHand == "R";

        var notchDepth = Dimension.FromInches(part.BBDepth);
        var notchHeight = Dimension.FromInches(part.BBHeight);
        var baseNotch = new BaseNotch(notchHeight, notchDepth);

        var transitionDepth = Dimension.FromInches(double.Min(leftDrilling, rightDrilling));

        return new() {
            Qty = part.Quantity,
            PartNumber = part.PartNum,
            Room = room,
            UnitPrice = unitPrice,
            Color = part.Color,
            EdgeBandingColor = edgeBandingColor,

            Height = height,
            Depth = depth,

            BaseNotch = baseNotch,
            ExtendBack = wallHasBacking,
            WallHung = isWallMount,               // Closet pro does not support wall hung hutch panels, at this time
            HasBottomRadius = false,
            Drilling = finLeft ? VerticalPanelDrilling.FinishedLeft : finRight ? VerticalPanelDrilling.FinishedRight : VerticalPanelDrilling.DrilledThrough,
        };

    }

    public static TransitionVerticalPanel CreateTransitionVerticalPanel(Part part, bool wallHasBacking, RoomNamingStrategy strategy) {

        double leftDrilling = part.VertDrillL;
        double rightDrilling = part.VertDrillR;

        bool isWallMount = part.ExportName.Contains("WM");

        bool hasRadiusBottom = part.ExportName.Contains("Radius");

        if (!TryParseMoneyString(part.PartCost, out decimal unitPrice)) {
            unitPrice = 0M;
        }
        string room = GetRoomName(part, strategy);

        Dimension depth = Dimension.FromInches(part.Depth);
        Dimension height = Dimension.FromInches(part.Height);
        string edgeBandingColor = part.InfoRecords
                                        .Where(i => i.PartName == "Edge Banding") // i.CornerShelfSizes contains the information about what edges to apply banding
                                        .Select(i => i.Color)
                                        .FirstOrDefault() ?? part.Color;

        bool finLeft = leftDrilling < rightDrilling;
        bool finRight = rightDrilling < leftDrilling;

        var notchDepth = Dimension.FromInches(part.BBDepth);
        var notchHeight = Dimension.FromInches(part.BBHeight);
        var baseNotch = new BaseNotch(notchHeight, notchDepth);

        var transitionDepth = Dimension.FromInches(double.Min(leftDrilling, rightDrilling));

        return new() {
            Qty = part.Quantity,
            PartNumber = part.PartNum,
            Room = room,
            UnitPrice = unitPrice,
            Color = part.Color,
            EdgeBandingColor = edgeBandingColor,

            Height = height,
            Depth = depth,
            TransitionDepth = transitionDepth,

            BaseNotch = baseNotch,
            ExtendBack = wallHasBacking,
            WallHung = isWallMount,               // Closet pro does not support wall hung hutch panels, at this time
            HasBottomRadius = false,
            Drilling = finLeft ? VerticalPanelDrilling.FinishedLeft : finRight ? VerticalPanelDrilling.FinishedRight : VerticalPanelDrilling.DrilledThrough,
        };

    }

    public static IslandVerticalPanel CreateIslandVerticalPanel(Part part, RoomNamingStrategy strategy) {

        if (part.VertHand == "T") {
            throw new InvalidOperationException("Through drilled island panels are not supported");
        }

        if (!TryParseMoneyString(part.PartCost, out decimal unitPrice)) {
            unitPrice = 0M;
        }

        string room = GetRoomName(part, strategy);

        Dimension panelDepth = Dimension.FromInches(part.Depth);
        Dimension height = Dimension.FromInches(part.Height);
        string edgeBandingColor = part.InfoRecords
                                        .Where(i => i.PartName == "Edge Banding") // i.CornerShelfSizes contains the information about what edges to apply banding
                                        .Select(i => i.Color)
                                        .FirstOrDefault() ?? part.Color;

        var leftDrilling = Dimension.FromInches(part.VertDrillL);
        var rightDrilling = Dimension.FromInches(part.VertDrillR);

        bool finLeft = part.VertHand == "L";
        bool finRight = part.VertHand == "R";

        Dimension side1Depth = (finLeft ? rightDrilling : leftDrilling);

        return new() {
            Qty = part.Quantity,
            PartNumber = part.PartNum,
            Room = room,
            UnitPrice = unitPrice,
            Color = part.Color,
            EdgeBandingColor = edgeBandingColor,

            Height = height,
            PanelDepth = panelDepth,
            Side1Depth = side1Depth,
            Drilling = finLeft ? VerticalPanelDrilling.FinishedLeft : finRight ? VerticalPanelDrilling.FinishedRight : VerticalPanelDrilling.DrilledThrough,
        };

    }

    public static HutchVerticalPanel CreateHutchVerticalPanel(Part part, bool wallHasBacking, RoomNamingStrategy strategy) {

        bool finLeft = part.VertHand == "Left";
        bool finRight = part.VertHand == "Right";

        if (!finLeft && !finRight && part.VertDrillL != part.VertDrillR) {
            throw new InvalidOperationException("Hutch transition panels are not supported");
        }

        string[]? dims;
        if (finRight) {
            dims = part.UnitR.Split('|');
        } else {
            dims = part.UnitL.Split('|');
        }

        if (dims is null || dims.Length != 4) {
            throw new InvalidOperationException("Invalid hutch panel dimensions");
        }

        if (!double.TryParse(dims[0], out double baseDepth)) throw new InvalidOperationException($"Invalid hutch panel base depth value '{dims[0]}'");
        if (!double.TryParse(dims[1], out double baseHeight)) throw new InvalidOperationException($"Invalid hutch panel base height value '{dims[1]}'");
        if (!double.TryParse(dims[2], out double topDepth)) throw new InvalidOperationException($"Invalid hutch panel top depth value '{dims[2]}'");
        if (!double.TryParse(dims[3], out double topHeight)) throw new InvalidOperationException($"Invalid hutch panel top height value '{dims[3]}'");

        if (baseHeight + topHeight != part.Height) {
            throw new InvalidOperationException($"Hutch panel height does not match sum of top and base heights | panel:{part.Height} top:{topHeight} base:{baseHeight}");
        }

        if (baseDepth != part.Depth) {
            throw new InvalidOperationException($"Hutch panel depth does not match base depth | panel:{part.Depth} base:{baseDepth}");
        }

        if (!TryParseMoneyString(part.PartCost, out decimal unitPrice)) {
            unitPrice = 0M;
        }
        string room = GetRoomName(part, strategy);
        string sku = finLeft || finRight ? "PEH" : "PCH";

        Dimension bottomDepth = Dimension.FromInches(part.Depth);
        Dimension panelHeight = Dimension.FromInches(part.Height);
        string edgeBandingColor = part.InfoRecords
                                        .Where(i => i.PartName == "Edge Banding") // i.CornerShelfSizes contains the information about what edges to apply banding
                                        .Select(i => i.Color)
                                        .FirstOrDefault() ?? part.Color;
        var notchDepth = Dimension.FromInches(part.BBDepth);
        var notchHeight = Dimension.FromInches(part.BBHeight);
        var baseNotch = new BaseNotch(notchHeight, notchDepth);

        bool isWallMount = part.ExportName.Contains("WM");

        return new HutchVerticalPanel() {
            Qty = part.Quantity,
            PartNumber = part.PartNum,
            Room = room,
            UnitPrice = unitPrice,
            Color = part.Color,
            EdgeBandingColor = edgeBandingColor,

            PanelHeight = panelHeight,
            BottomHeight = Dimension.FromInches(baseHeight),
            TopDepth = Dimension.FromInches(topDepth),
            BottomDepth = bottomDepth,

            BaseNotch = baseNotch,
            ExtendBack = wallHasBacking,
            WallHung = isWallMount,               // Closet pro does not support wall hung hutch panels, at this time
            HasBottomRadius = false,
            Drilling = finLeft ? VerticalPanelDrilling.FinishedLeft : finRight ? VerticalPanelDrilling.FinishedRight : VerticalPanelDrilling.DrilledThrough,
        };

    }

}
