using CADCode;
using ApplicationCore.Features.CADCode.Services.Domain;
using ApplicationCore.Features.CADCode.Contracts.Machining;
using ApplicationCore.Features.CADCode.Contracts;
using ApplicationCore.Features.CADCode.Services.Domain.Inventory;

namespace ApplicationCore.Features.CADCode.Services.Services.CADCodeGCode;

static class CADCodeExtensions {

    public static IReadOnlyList<string> GetUnplacedPartNames(this CADCodePanelOptimizerClass optimizer) {
        var unplaced = optimizer.GetPartsNotPlaced();
        var unplacedPartNames = new List<string>();
        if (unplaced is null) return unplacedPartNames;
        foreach (var part in unplaced) {
            if (part is not string partname) continue;
            unplacedPartNames.Add(partname);
        }
        return unplacedPartNames;
    }

    public static IReadOnlyList<string> GetProgramFileNamesEnumerable(this CADCodeCodeClass code) {
        var processedFiles = new List<string>();
        var fileNames = code.GetProcessedFileNames();
        if (fileNames is null) return processedFiles;
        foreach (var file in fileNames) {
            if (file is not string filepath) continue;
            try {
                var programname = Path.GetFileName(filepath.Split(',')[0]);
                processedFiles.Add(programname);
            } catch {
                processedFiles.Add("unknown");
            }
        }
        
        return processedFiles;
    }


    public static void AddNestedPartMachining(this CADCodeCodeClass code, CNCPart part, ToolMap toolMap, TableOrientation orientation) {

        // ANDI config
        //code.NestedPart((float)part.Width, (float)part.Length, OriginType.CC_LL, part.FileName, AxisTypes.CC_X_AXIS, 90);

        float width = (float)part.Width; //(float)(orientation == TableOrientation.Standard ? part.Width : part.Length);
        float length = (float)part.Length; //(float)(orientation == TableOrientation.Standard ? part.Length : part.Width);

        code.NestedPart(width, length, OriginType.CC_LL, part.FileName, AxisTypes.CC_AUTO_AXIS, 0);
        
        foreach (var token in part.Tokens) {
            code.AddMachining(token, toolMap, orientation);
        }

    }

    public static void AddSingleProgram(this CADCodeCodeClass code, CNCPart part, UnitTypes units, ToolMap toolMap, TableOrientation orientation) {

        float width = (float) part.Width; // (float)(orientation == TableOrientation.Standard ? part.Width : part.Length);
        float length = (float) part.Length;//(float)(orientation == TableOrientation.Standard ? part.Length : part.Width);

        code.Border(width, length, (float)part.Material.Thickness, units, OriginType.CC_LL, part.FileName, AxisTypes.CC_X_AXIS);
        
        foreach (var token in part.Tokens) {
            code.AddMachining(token, toolMap, orientation);
        }

        code.EndPanel();

    }

    public static void AddMachining(this CADCodeCodeClass code, Token token, ToolMap toolMap, TableOrientation orientation) {

        // TODO: need to get the tool specs (already have tool map passed in here, maybe a better way?)

        var (tool, _) = toolMap.Tools[token.Tool.Name];

        // TODO: **** switch all x and y positions based on table orientation ****

        if (token is Bore bore) {
            code.Bore(bore.Position.X, bore.Position.Y, bore.Depth, FaceTypes.CC_UPPER_FACE, bore.Tool.Diameter, bore.Tool.Name, "", "", (float) tool.SpindleSpeed, (float)tool.FeedSpeed, bore.RType, bore.Sequence, bore.PassCount);
        } else if (token is MultiBore multiBore) {
            code.MultiBore(multiBore.StartPosition.X, multiBore.StartPosition.Y, multiBore.Depth, multiBore.EndPosition.X, multiBore.EndPosition.Y, FaceTypes.CC_UPPER_FACE, multiBore.Tool.Diameter, multiBore.Tool.Name, multiBore.Pitch, (float)tool.SpindleSpeed, (float)tool.FeedSpeed, multiBore.RType, multiBore.NumberOfHoles, multiBore.Sequence, multiBore.PassCount);
        } else if (token is Pocket pocket) {
            code.Pocket(pocket.PositionA.X, pocket.PositionA.Y, pocket.PositionB.X, pocket.PositionB.Y, pocket.PositionC.X, pocket.PositionC.Y, pocket.PositionD.X, pocket.PositionD.Y, pocket.StartDepth, pocket.EndDepth, pocket.Tool.Name, FaceTypes.CC_UPPER_FACE, (float)tool.FeedSpeed, (float)tool.EntrySpeed, (float)tool.SpindleSpeed, (float)tool.CornerFeed, pocket.RType, pocket.Sequence, pocket.Climb, pocket.PassCount);
        }else if (token is Route route) {
            code.RouteLine(route.StartPosition.X, route.StartPosition.Y, route.StartDepth, route.EndPosition.X, route.EndPosition.Y, route.EndDepth, route.Tool.Name, route.Tool.Diameter, route.Offset.Type.AsCCOffsetType(), route.Offset.Amount, tool.Rotation.AsCCRotationType(), FaceTypes.CC_UPPER_FACE, (float)tool.FeedSpeed, (float)tool.EntrySpeed, (float)tool.SpindleSpeed, (float)tool.CornerFeed, route.RType, route.Sequence, route.PassCount);
        } else if (token is Arc arc) {
            code.RouteArc(arc.PositionA.X, arc.PositionA.Y, arc.StartDepth, arc.PositionD.X, arc.PositionD.Y, arc.EndDepth, arc.Tool.Name, arc.Tool.Diameter, arc.Offset.Type.AsCCOffsetType(), arc.Offset.Amount, tool.Rotation.AsCCRotationType(), FaceTypes.CC_UPPER_FACE, (float)tool.FeedSpeed, (float)tool.EntrySpeed, (float)tool.SpindleSpeed, (float)tool.CornerFeed, arc.RType, arc.PositionB.X, arc.PositionB.Y, arc.PositionC.X, arc.PositionC.Y, arc.Radius, arc.Direction.AsCCArcType(), arc.Bulge, arc.Sequence, arc.PassCount);
        } else if (token is Text text) {
            throw new NotImplementedException("Rectangle is not implemented");
        } else if (token is Rectangle rectangle) {
            throw new NotImplementedException("Rectangle is not implemented");
        }
        
    }

    public static List<Part> GetAsCADCodeParts(this CNCPart part, UnitTypes units, TableOrientation orientation) {
        var ccparts = new List<Part>();
        for (int i = 0; i < part.Qty; i++) {

            string width = part.Width.ToString();//(orientation == TableOrientation.Standard ? part.Width : part.Length).ToString();
            string length = part.Length.ToString();//(orientation == TableOrientation.Standard ? part.Length : part.Width).ToString();

            var ccpart = new Part() {
                QuantityOrdered = 1,
                Face5Filename = part.FileName,
                Length = length,
                Width = width,
                Thickness = (float)part.Material.Thickness,
                Description1 = part.Description,
                Material = part.Material.Name,
                Units = units,
                RotationAllowed = 1,
                //DoLabel = false,
            };
            ccparts.Add(ccpart);
        }
        return ccparts;
    }

    public static OffsetTypes AsCCOffsetType(this OffsetType type) => type switch {
        OffsetType.None => OffsetTypes.CC_OFFSET_NONE,
        OffsetType.Left => OffsetTypes.CC_OFFSET_LEFT,
        OffsetType.Right => OffsetTypes.CC_OFFSET_RIGHT,
        OffsetType.Inside => OffsetTypes.CC_OFFSET_INSIDE,
        OffsetType.Outside => OffsetTypes.CC_OFFSET_OUTSIDE,
        OffsetType.Center => OffsetTypes.CC_OFFSET_CENTERLINE,
        _ => throw new NotImplementedException(),
    };

    public static RotationTypes AsCCRotationType(this ToolRotation rotation) => rotation switch {
        ToolRotation.Clockwise => RotationTypes.CC_ROTATION_CLOCKWISE,
        ToolRotation.CounterClockwise => RotationTypes.CC_ROTATION_CCLOCKWISE,
        ToolRotation.Auto => RotationTypes.CC_ROTATION_AUTO,
        _ => throw new NotImplementedException(),
    };

    public static ArcTypes AsCCArcType(this ArcDirection direction) => direction switch {
        ArcDirection.Clockwise => ArcTypes.CC_CLOCKWISE_ARC,
        ArcDirection.CounterClockwise => ArcTypes.CC_COUNTER_CLOCKWISE_ARC,
        ArcDirection.Unknown => ArcTypes.CC_UNKNOWN_ARC,
        _ => throw new NotImplementedException(),
    };

    public static IEnumerable<CutlistInventory> AsCutListInventory(this InventoryItem item, TableOrientation orientation) {
        
        foreach (var size in item.Sizes) {

            yield return new CutlistInventory() {
                Description = item.Name,
                Length = (orientation == TableOrientation.Standard ? size.Length : size.Width).ToString(),
                Width = (orientation == TableOrientation.Standard ? size.Width : size.Length).ToString(),
                Thickness = item.Thickness.ToString(),
                Priority = size.Priority.ToString(),
                Graining = item.IsGrained ? "True" : "False", // TODO: check what this is supposed to be

                // TODO add supply to inventory
                Supply = "999",
                Trim1 = "7",
                Trim2 = "7",
                Trim3 = "4",
                Trim4 = "4",
                MinimumRestockDimension = 0,
                MinimumRestockArea = 0.2f,
                TrimDrop = false
            };

        }

    }

}
