using CADCode;
using ApplicationCore.Features.CNC.GCode.Domain.Inventory;
using ApplicationCore.Features.CNC.GCode.Domain;
using ApplicationCore.Features.CNC.GCode.Contracts.Machining;
using ApplicationCore.Features.CNC.GCode.Contracts;

namespace ApplicationCore.Features.CNC.GCode;

static class CADCodeExtensions
{

    public static IReadOnlyList<string> GetUnplacedPartNames(this CADCodePanelOptimizerClass optimizer)
    {
        var unplaced = optimizer.GetPartsNotPlaced();
        var unplacedPartNames = new List<string>();
        if (unplaced is null) return unplacedPartNames;
        foreach (var part in unplaced)
        {
            if (part is not string partname) continue;
            unplacedPartNames.Add(partname);
        }
        return unplacedPartNames;
    }

    public static IReadOnlyList<string> GetProgramFileNamesEnumerable(this CADCodeCodeClass code)
    {
        var processedFiles = new List<string>();
        var fileNames = code.GetProcessedFileNames();
        if (fileNames is null) return processedFiles;
        foreach (var file in fileNames)
        {
            if (file is not string filepath) continue;
            try
            {
                var programname = Path.GetFileName(filepath.Split(',')[0]);
                processedFiles.Add(programname);
            }
            catch
            {
                processedFiles.Add("unknown");
            }
        }

        return processedFiles;
    }


    public static void AddNestedPartMachining(this CADCodeCodeClass code, CNCPart part, ToolMap toolMap)
    {

        code.NestedPart((float)part.Length, (float)part.Width, OriginType.CC_LL, part.FileName, AxisTypes.CC_AUTO_AXIS, 0);

        foreach (var token in part.Tokens)
        {
            code.AddMachining(token, toolMap);
        }

    }

    public static void AddSingleProgram(this CADCodeCodeClass code, CNCPart part, UnitTypes units, ToolMap toolMap)
    {

        code.Border((float)part.Length, (float)part.Width, (float)part.Material.Thickness, units, OriginType.CC_LL, part.FileName, AxisTypes.CC_X_AXIS);

        foreach (var token in part.Tokens)
        {
            code.AddMachining(token, toolMap);
        }

        code.EndPanel();

    }

    public static void AddMachining(this CADCodeCodeClass code, Token token, ToolMap toolMap)
    {

        // TODO: need to get the tool specs (already have tool map passed in here, maybe a better way?)

        var (tool, _) = toolMap.Tools[token.Tool.Name.ToLower()];

        if (token is Bore bore)
        {

            code.Bore((float)bore.Position.X,
                      (float)bore.Position.Y,
                      (float)bore.Depth,
                             FaceTypes.CC_UPPER_FACE,
                      (float)bore.Tool.Diameter,
                             bore.Tool.Name,
                             "",
                             "",
                      (float)tool.SpindleSpeed,
                      (float)tool.FeedSpeed,
                             bore.RType,
                             bore.Sequence,
                             bore.PassCount);

        }
        else if (token is MultiBore multiBore)
        {

            code.MultiBore((float)multiBore.StartPosition.X,
                            (float)multiBore.StartPosition.Y,
                            (float)multiBore.Depth,
                            (float)multiBore.EndPosition.X,
                            (float)multiBore.EndPosition.Y,
                                   FaceTypes.CC_UPPER_FACE,
                            (float)multiBore.Tool.Diameter,
                                   multiBore.Tool.Name,
                            (float)multiBore.Pitch,
                            (float)tool.SpindleSpeed,
                            (float)tool.FeedSpeed,
                                   multiBore.RType,
                                   multiBore.NumberOfHoles,
                                   multiBore.Sequence,
                                   multiBore.PassCount);

        }
        else if (token is Pocket pocket)
        {

            code.Pocket((float)pocket.PositionA.X,
                        (float)pocket.PositionA.Y,
                        (float)pocket.PositionB.X,
                        (float)pocket.PositionB.Y,
                        (float)pocket.PositionC.X,
                        (float)pocket.PositionC.Y,
                        (float)pocket.PositionD.X,
                        (float)pocket.PositionD.Y,
                        (float)pocket.StartDepth,
                        (float)pocket.EndDepth,
                               pocket.Tool.Name,
                               FaceTypes.CC_UPPER_FACE,
                        (float)tool.FeedSpeed,
                        (float)tool.EntrySpeed,
                        (float)tool.SpindleSpeed,
                        (float)tool.CornerFeed,
                               pocket.RType,
                               pocket.Sequence,
                               pocket.Climb,
                               pocket.PassCount);

        }
        else if (token is PocketArc parc)
        {

            code.DefinePocket((float)parc.StartPosition.X,
                                (float)parc.StartPosition.Y,
                                (float)parc.StartDepth,
                                (float)parc.EndPosition.X,
                                (float)parc.EndPosition.Y,
                                (float)parc.EndDepth,
                                       0,
                                       0,
                                       0,
                                (float)parc.Radius,
                                       parc.Direction.AsCCArcType(),
                                       parc.Offset.Type.AsCCOffsetType(),
                                (float)parc.Offset.Amount,
                                       tool.Rotation.AsCCRotationType(),
                                       0,
                                       tool.Name,
                                (float)tool.Diameter,
                                (float)tool.FeedSpeed,
                                (float)tool.EntrySpeed,
                                       0,
                                       parc.Sequence,
                                       Array.Empty<byte>(),
                                       false,
                                       parc.PassCount);


        }
        else if (token is PocketSegment psegment)
        {

            code.DefinePocket((float)psegment.StartPosition.X,
                                (float)psegment.StartPosition.Y,
                                (float)psegment.StartDepth,
                                (float)psegment.EndPosition.X,
                                (float)psegment.EndPosition.Y,
                                (float)psegment.EndDepth,
                                       0,
                                       0,
                                       0,
                                       0,
                                       ArcTypes.CC_UNKNOWN_ARC,
                                       psegment.Offset.Type.AsCCOffsetType(),
                                (float)psegment.Offset.Amount,
                                       tool.Rotation.AsCCRotationType(),
                                       0,
                                       tool.Name,
                                (float)tool.Diameter,
                                (float)tool.FeedSpeed,
                                (float)tool.EntrySpeed,
                                       0,
                                       psegment.Sequence,
                                       Array.Empty<byte>(),
                                       false,
                                       psegment.PassCount);


        }
        else if (token is RouteArc arc)
        {

            code.RouteArc((float)arc.StartPosition.X,
                            (float)arc.StartPosition.Y,
                            (float)arc.StartDepth,
                            (float)arc.EndPosition.X,
                            (float)arc.EndPosition.Y,
                            (float)arc.EndDepth,
                                   arc.Tool.Name,
                            (float)arc.Tool.Diameter,
                                   arc.Offset.Type.AsCCOffsetType(),
                            (float)arc.Offset.Amount,
                                   tool.Rotation.AsCCRotationType(),
                                   FaceTypes.CC_UPPER_FACE,
                            (float)tool.FeedSpeed,
                            (float)tool.EntrySpeed,
                            (float)tool.SpindleSpeed,
                            (float)tool.CornerFeed,
                                   arc.RType,
                                   0,
                                   0,
                                   0,
                                   0,
                            (float)arc.Radius,
                                   arc.Direction.AsCCArcType(),
                                   0,
                                   arc.Sequence,
                                   arc.PassCount);

        }
        else if (token is RouteLine route)
        {

            code.RouteLine((float)route.StartPosition.X,
                            (float)route.StartPosition.Y,
                            (float)route.StartDepth,
                            (float)route.EndPosition.X,
                            (float)route.EndPosition.Y,
                            (float)route.EndDepth,
                                   route.Tool.Name,
                            (float)route.Tool.Diameter,
                                   route.Offset.Type.AsCCOffsetType(),
                            (float)route.Offset.Amount,
                                   tool.Rotation.AsCCRotationType(),
                                   FaceTypes.CC_UPPER_FACE,
                            (float)tool.FeedSpeed,
                            (float)tool.EntrySpeed,
                            (float)tool.SpindleSpeed,
                            (float)tool.CornerFeed,
                                   route.RType,
                                   route.Sequence,
                                   route.PassCount);

        }
        else if (token is OutlineArc oarc)
        {

            code.DefineOutLine((float)oarc.Start.X,
                                (float)oarc.Start.Y,
                                (float)oarc.End.X,
                                (float)oarc.End.Y,
                                       0,
                                       0,
                                (float)oarc.Radius,
                                       oarc.Direction.AsCCArcType(),
                                       oarc.Offset.Type.AsCCOffsetType(),
                                       tool.Name,
                                (float)tool.FeedSpeed,
                                (float)tool.SpindleSpeed,
                                       oarc.Sequence,
                                       oarc.PassCount,
                                       0);

        }
        else if (token is OutlineSegment osegment)
        {

            code.DefineOutLine((float)osegment.Start.X,
                                (float)osegment.Start.Y,
                                (float)osegment.End.X,
                                (float)osegment.End.Y,
                                       0,
                                       0,
                                       0,
                                       ArcTypes.CC_UNKNOWN_ARC,
                                       osegment.Offset.Type.AsCCOffsetType(),
                                       tool.Name,
                                (float)tool.FeedSpeed,
                                (float)tool.SpindleSpeed,
                                       osegment.Sequence,
                                       osegment.PassCount,
                                       0);

        }
        else if (token is Rectangle rectangle)
        {

            var components = rectangle.GetComponents();
            foreach (var component in components)
            {
                code.AddMachining(component, toolMap);
            }

        }

    }

    public static List<Part> GetAsCADCodeParts(this CNCPart part, UnitTypes units, TableOrientation orientation)
    {
        var ccparts = new List<Part>();
        for (int i = 0; i < part.Qty; i++)
        {

            var ccpart = new Part()
            {
                QuantityOrdered = 1,
                Face5Filename = part.FileName,
                Length = part.Length.ToString(), // The length is the PanelXDimension
                Width = part.Width.ToString(), // The width is the PanelYDimension
                Thickness = (float)part.Material.Thickness,
                Description1 = part.Description,
                Material = part.Material.Name,
                Units = units,
                RotationAllowed = 1,
                Rotated = false,
                ContainsShape = part.ContainsShape,
                RouteShape = part.ContainsShape,
                PerimeterRoute = true
            };

            ccparts.Add(ccpart);
        }
        return ccparts;
    }

    public static OffsetTypes AsCCOffsetType(this OffsetType type) => type switch
    {
        OffsetType.None => OffsetTypes.CC_OFFSET_NONE,
        OffsetType.Left => OffsetTypes.CC_OFFSET_LEFT,
        OffsetType.Right => OffsetTypes.CC_OFFSET_RIGHT,
        OffsetType.Inside => OffsetTypes.CC_OFFSET_INSIDE,
        OffsetType.Outside => OffsetTypes.CC_OFFSET_OUTSIDE,
        OffsetType.Center => OffsetTypes.CC_OFFSET_CENTERLINE,
        _ => throw new NotImplementedException(),
    };

    public static RotationTypes AsCCRotationType(this ToolRotation rotation) => rotation switch
    {
        ToolRotation.Clockwise => RotationTypes.CC_ROTATION_CLOCKWISE,
        ToolRotation.CounterClockwise => RotationTypes.CC_ROTATION_CCLOCKWISE,
        ToolRotation.Auto => RotationTypes.CC_ROTATION_AUTO,
        _ => throw new NotImplementedException(),
    };

    public static ArcTypes AsCCArcType(this ArcDirection direction) => direction switch
    {
        ArcDirection.Clockwise => ArcTypes.CC_CLOCKWISE_ARC,
        ArcDirection.CounterClockwise => ArcTypes.CC_COUNTER_CLOCKWISE_ARC,
        ArcDirection.Unknown => ArcTypes.CC_UNKNOWN_ARC,
        _ => throw new NotImplementedException(),
    };

    public static IEnumerable<CutlistInventory> AsCutListInventory(this InventoryItem item, TableOrientation orientation)
    {

        foreach (var size in item.Sizes)
        {

            yield return new CutlistInventory()
            {
                Description = item.Name,
                Length = (orientation == TableOrientation.Standard ? size.Length : size.Width).ToString(),
                Width = (orientation == TableOrientation.Standard ? size.Width : size.Length).ToString(),
                Thickness = item.Thickness.ToString(),
                Priority = size.Priority.ToString(),
                Graining = item.IsGrained ? "1" : "0",

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
