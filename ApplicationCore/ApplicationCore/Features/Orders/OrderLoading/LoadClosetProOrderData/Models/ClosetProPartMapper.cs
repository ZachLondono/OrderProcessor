using ApplicationCore.Features.Companies.Contracts.ValueObjects;
using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.Products.Closets;
using ApplicationCore.Features.Orders.Shared.Domain.Products.Doors;
using ApplicationCore.Features.Orders.Shared.Domain.Products.DrawerBoxes;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Shared;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.OrderLoading.LoadClosetProOrderData.Models;

public class ClosetProPartMapper {

    public Dictionary<string, Func<Part, bool, IProduct>> ProductNameMappings { get; }
    public Dictionary<string, Dimension> FrontHardwareSpreads { get; }
    public Dimension HardwareSpread { get; set; } = Dimension.Zero;
    public ClosetProSettings Settings { get; set; } = new();

    private readonly ComponentBuilderFactory _factory;

    public ClosetProPartMapper(ComponentBuilderFactory factory) {

        _factory = factory;

        ProductNameMappings = new() {
            { "CPS FM Vert", CreateVerticalPanelFromPart },
            { "CPS WM Vert", CreateVerticalPanelFromPart },
            { "CPS WM Vert Radius", CreateVerticalPanelFromPart },
            { "CPS WM Vert Straight", CreateVerticalPanelFromPart },
            { "VP-Hutch", CreateVerticalHutchPanelFromPart },
            { "VP-Corner Floor Mount", CreateVerticalPanelFromPart },
            { "VP-Corner Wall Hung", CreateVerticalPanelFromPart },
            { "FixedShelf", CreateFixedShelfFromPart },
            { "AdjustableShelf", CreateAdjustableShelfFromPart },
            { "ShoeShelf", CreateShoeShelfFromPart },
            { "Toe Kick_3.75", CreateToeKickFromPart },
            { "Toe Kick_2.5", CreateToeKickFromPart },
            { "Cleat", CreateCleatPart },
            { "Melamine Sidemount", CreateDoweledDrawerBox },
            { "Melamine Undermount", CreateDoweledDrawerBox },
            { "Dovetail Sidemount", CreateDovetailDrawerBox },
            { "Dovetail Undermount", CreateDovetailDrawerBox },
            { "Scoop Front Box", CreateDovetailDrawerBox },
            { "Zargen", CreateZargenDrawerBox },
            { "Slab", CreateSlabFront },
            { "Filler Panel", CreateFillerPanel },
            { "Backing", CreateBackingPart },
            { "Back-Panel", CreateBackingPart }
        };

        FrontHardwareSpreads = new();

    }

    public List<IProduct> MapPartsToProducts(IEnumerable<Part> parts) {

        List<IProduct> products = new();

        parts.GroupBy(p => p.WallNum)
            .ForEach(partsOnWall => {

                var productsOnWall = GetPartsForWall(partsOnWall);

                products.AddRange(productsOnWall);

            });


        return products;

    }

    private IEnumerable<IProduct> GetPartsForWall(IEnumerable<Part> parts) {

        List<IProduct> products = new();

        Dictionary<int, double> sectionDepths = parts.Where(p => p.ExportName.Contains("Vert"))
                                                     .DistinctBy(p => p.SectionNum)
                                                     .ToDictionary(p => p.SectionNum, p => p.Depth);
        bool doesWallHaveBacking = parts.Any(p => p.ExportName == "Backing");

        var enumerator = parts.GetEnumerator();
        if (!enumerator.MoveNext()) {
            throw new InvalidOperationException("No products found in order");
        }
        Part part = enumerator.Current;

        while (true) {

            Part? nextPart = null;
            if (part.PartName == "Melamine") {

                products.Add(CreatePanelFromPart(part));

            } else if (part.PartType == "Countertop") {

                if (part.Height != 0.75) {
                    throw new InvalidOperationException($"Unsupported counter top thickness '{part.Height}', only 3/4\" is supported");
                }
                products.Add(CreateTopFromPart(part, doesWallHaveBacking));

            } else if (part.PartName == "Cab Door Rail" || (part.PartName.Contains("Drawer") && part.PartName.Contains("Rail"))) {

                // TODO: Cabinet door parts have a hinge direction, if that information is to be added to the product it will need to be read

                if (!enumerator.MoveNext()) {
                    throw new InvalidOperationException("Unexpected end of part list");
                }

                var insertPart = enumerator.Current;
                if (insertPart.PartName != "Cab Door Insert" && !(insertPart.PartName.Contains("Drawer") && insertPart.PartName.Contains("Insert"))) {
                    throw new InvalidOperationException("Door/Drawer rail part found without door/drawer insert");
                }

                products.Add(CreateFrontFromParts(part, insertPart));
                if (enumerator.MoveNext()) {
                    part = enumerator.Current;
                    continue;
                } else {
                    break;
                }

            } else if (part.ExportName == "FixedShelf" && enumerator.MoveNext()) {

                var possibleCubbyPart = enumerator.Current;
                if (possibleCubbyPart.ExportName == "Cubby-V" || possibleCubbyPart.ExportName == "Cubby-H") {

                    var cubbyProducts = CreateCubbyProducts(enumerator, part, possibleCubbyPart, doesWallHaveBacking);
                    products.AddRange(cubbyProducts);

                    if (enumerator.MoveNext()) {
                        part = enumerator.Current;
                        continue;
                    } else {
                        break;
                    }

                }

                bool extendBack = false;
                if (sectionDepths.TryGetValue(part.SectionNum, out var depth)) {
                    if (part.Depth == depth && doesWallHaveBacking) {
                        extendBack = true;
                    }
                }

                products.Add(CreateFixedShelfFromPart(part, doesWallHaveBacking, extendBack));
                nextPart = possibleCubbyPart;

            } else if (part.ExportName == "AdjustableShelf") {

                bool extendBack = false;
                if (sectionDepths.TryGetValue(part.SectionNum, out var depth)) {
                    if (part.Depth == depth && doesWallHaveBacking) {
                        extendBack = true;
                    }
                }

                products.Add(CreateAdjustableShelfFromPart(part, doesWallHaveBacking, extendBack));

            } else if (ProductNameMappings.TryGetValue(part.ExportName, out var mapper)) {
                try {
                    products.Add(mapper(part, doesWallHaveBacking));
                } catch (Exception ex) {
                    throw new InvalidOperationException($"Part could not be mapped to a valid product - #{part.PartNum} {part.PartName}/{part.ExportName} W{part.WallNum} S{part.SectionNum} - {ex.Message}", ex);
                }
            } else {
                throw new InvalidOperationException($"Unexpected part {part.PartName} / {part.ExportName}");
            }

            if (nextPart is not null) {
                part = nextPart;
            } else if (enumerator.MoveNext()) {
                part = enumerator.Current;
            } else {
                break;
            }

        }

        return products;

    }

    private IEnumerable<IProduct> CreateCubbyProducts(IEnumerator<Part> enumerator, Part part, Part firstCubbyPart, bool doesWallHaveBacking) {

        var accum = new CubbyAccumulator();
        accum.AddBottomShelf(part);
        if (firstCubbyPart.ExportName == "Cubby-V") {
            accum.AddVerticalPanel(firstCubbyPart);
        } else {
            accum.AddHorizontalPanel(firstCubbyPart);
        }


        Part cubbyPart;
        bool areAllCubbyPartsRead = false;
        while (!areAllCubbyPartsRead) {

            if (!enumerator.MoveNext()) {
                throw new InvalidOperationException("Ran out of parts before all cubby parts where found");
            }

            cubbyPart = enumerator.Current;

            switch (cubbyPart.ExportName) {
                case "Cubby-V":
                    accum.AddVerticalPanel(cubbyPart);
                    break;
                case "Cubby-H":
                    accum.AddHorizontalPanel(cubbyPart);
                    break;
                case "FixedShelf":
                    if ((cubbyPart.PartName == "Top Fixed Shelf" || cubbyPart.PartName == "Bottom Fixed Shelf") && doesWallHaveBacking) {
                        throw new InvalidOperationException("Cannot create a top/bottom divider shelf with extended back for back panel. A closet section/wall with a back panel must have a top & bottom fixed shelf with an 'ExtendedBack' to support the back panel, divider shelves do not support this.");
                    }
                    accum.AddTopShelf(cubbyPart);
                    areAllCubbyPartsRead = true;
                    break;
                default:
                    throw new InvalidOperationException("Unexpected part found within cubby parts");
            }

        }

        return accum.GetProducts(this);

    }

    public List<AdditionalItem> MapPickListToItems(IEnumerable<PickPart> parts, out Dimension hardwareSpread) {

        hardwareSpread = Dimension.Zero;

        List<Dimension> spreads = new();

        List<AdditionalItem> items = new();
        foreach (var item in parts) {

            if (!TryParseMoneyString(item.Cost, out var cost)) {
                cost = 0;
            }

            items.Add(new(Guid.NewGuid(), $"({item.Quantity}) {item.Name}", cost));

            if (item.Type == "Pull/Knob") {
                // TODO: check pick list for "Pull/Knob" part types, if there is only one then the drilling spacing for drawer fronts can be inferred from that, if there are multiple then spacing cannot be inferred 
                if (FrontHardwareSpreads.TryGetValue(item.Name, out Dimension spread)) {
                    spreads.Add(spread);
                }
            }

        }

        if (spreads.Count == 1) {
            hardwareSpread = spreads[0];
        } else {
            // Hardware spread is unknown
            hardwareSpread = Dimension.Zero;
        }

        return items;

    }

    public static List<AdditionalItem> MapAccessoriesToItems(IEnumerable<Accessory> accessories) {

        List<AdditionalItem> items = new();

        foreach (var accessory in accessories) {

            if (!TryParseMoneyString(accessory.Cost, out var cost)) {
                cost = 0;
            }

            items.Add(new(Guid.NewGuid(), $"({accessory.Quantity}) {accessory.Name}", cost));

        }

        return items;

    }

    public static List<AdditionalItem> MapBuyOutPartsToItems(IEnumerable<BuyOutPart> parts) {

        List<AdditionalItem> items = new();

        foreach (var part in parts) {

            if (!TryParseMoneyString(part.PartCost, out decimal unitPrice)) {
                unitPrice = 0M;
            }

            if (part.PartName == "Hang Rod") {
                items.Add(new AdditionalItem(Guid.NewGuid(), $"({part.Quantity}) {part.PartName} - {part.Color} - {part.Width}\"L", unitPrice));
            } else {
                items.Add(new AdditionalItem(Guid.NewGuid(), $"({part.Quantity}) {part.PartName}", unitPrice));
            }

        }

        return items;

    }

    public static IProduct CreatePanelFromPart(Part part) {
        
        Dimension width = Dimension.FromInches(part.Width);
        Dimension length = Dimension.FromInches(part.Height);
        ClosetMaterial material = new(part.Color, ClosetMaterialCore.ParticleBoard);
        ClosetPaint? paint = null;
        string edgeBandingColor = part.InfoRecords
                                        .Where(i => i.PartName == "Edge Banding") // i.CornerShelfSizes contains the information about what edges to apply banding
                                        .Select(i => i.Color)
                                        .FirstOrDefault() ?? part.Color;
        string comment = "";

        Dictionary<string, string> parameters = new();

        string room = GetRoomName(part);

        if (!TryParseMoneyString(part.PartCost, out decimal unitPrice)) {
            unitPrice = 0M;
        }

        return new ClosetPart(Guid.NewGuid(), part.Quantity, unitPrice, part.PartNum, room, "PANEL", width, length, material, paint, edgeBandingColor, comment, parameters) {
            ProductionNotes = new() {
                "Custom Entered Material"
            }
        };

    }

    public static IProduct CreateFrontFromParts(Part rail, Part insert) {

        if (rail.Quantity != insert.Quantity) {
            throw new InvalidOperationException("Unexpected mismatch in door rail and insert quantity.");
        }

        DoorFrame frame = new(Dimension.FromInches((rail.Height - insert.Height) / 2), Dimension.FromInches((rail.Width - insert.Width) / 2));

        Dimension width = Dimension.FromInches(rail.Width);
        Dimension height = Dimension.FromInches(rail.Height);
        Dimension frameThickness = Dimension.FromInches(0.75);
        Dimension panelThickness = Dimension.FromInches(0.25);
        string color = rail.Color;

        if (!TryParseMoneyString(rail.PartCost, out decimal unitPriceRail)) {
            unitPriceRail = 0M;
        }
        if (!TryParseMoneyString(insert.PartCost, out decimal unitPriceInsert)) {
            unitPriceInsert = 0M;
        }
        string room = GetRoomName(rail);

        if (rail.ExportName.Contains("MDF")) {

            DoorType type = rail.PartName.Contains("Drawer") ? DoorType.DrawerFront : DoorType.Door;

            return new MDFDoorProduct(Guid.NewGuid(),
                                        unitPriceRail + unitPriceInsert,
                                        room,
                                        rail.Quantity,
                                        rail.PartNum,
                                        type,
                                        height,
                                        width,
                                        string.Empty,
                                        frame,
                                        $"MDF-{frameThickness.AsInchFraction()}\"",
                                        frameThickness,
                                        "Shaker",
                                        "Square",
                                        "Flat",
                                        Dimension.FromInches(0.25),
                                        DoorOrientation.Vertical,
                                        Array.Empty<AdditionalOpening>(),
                                        color);

        } else {

            return new FivePieceDoorProduct(Guid.NewGuid(),
                                           rail.Quantity,
                                           unitPriceRail + unitPriceInsert,
                                           rail.PartNum,
                                           room,
                                           width,
                                           height,
                                           frame,
                                           frameThickness,
                                           panelThickness,
                                           color);

        }

    }

    public static IProduct CreateVerticalHutchPanelFromPart(Part part, bool wallHasBacking) {

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
        string room = GetRoomName(part);
        string sku = (finLeft || finRight) ? "PEH" : "PCH";

        Dimension width = Dimension.FromInches(part.Depth);
        Dimension length = Dimension.FromInches(part.Height);
        ClosetMaterial material = new(part.Color, ClosetMaterialCore.ParticleBoard);
        ClosetPaint? paint = null;
        string edgeBandingColor = part.InfoRecords
                                        .Where(i => i.PartName == "Edge Banding") // i.CornerShelfSizes contains the information about what edges to apply banding
                                        .Select(i => i.Color)
                                        .FirstOrDefault() ?? part.Color;
        string comment = "";

        Dictionary<string, string> parameters = new() {
            { "FINLEFT", finLeft ? "1" : "0" },
            { "FINRIGHT", finRight ? "1" : "0" },
            { "ExtendBack", wallHasBacking ? "19.05" : "0" },
            { "BottomNotchD", "0" },
            { "BottomNotchH", "0" },
            { "TopDepth", Dimension.FromInches(topDepth).AsMillimeters().ToString() },
            { "DwrPanelH", Dimension.FromInches(baseHeight).AsMillimeters().ToString() },
            //{ "WallMount", isWallMount ? "1" : "0" },  // Closet pro does not allow wall hung hutch panels
            //{ "BottomRadius", hasRadiusBottom ? Settings.VerticalPanelBottomRadius.AsMillimeters().ToString() : "0" },
        };

        return new ClosetPart(Guid.NewGuid(), part.Quantity, unitPrice, part.PartNum, room, sku, width, length, material, paint, edgeBandingColor, comment, parameters);

    }

    public IProduct CreateVerticalPanelFromPart(Part part, bool wallHasBacking) {

        if (part.PartName == "Vertical Panel - Island") {
            return CreateVerticalIslandPanelFromPart(part, wallHasBacking);
        }

        double leftDrilling = part.VertDrillL;
        double rightDrilling = part.VertDrillR;

        bool isTransition = leftDrilling != 0 && rightDrilling != 0 && leftDrilling != rightDrilling;

        bool isWallMount = part.ExportName.Contains("WM");

        bool hasRadiusBottom = part.ExportName.Contains("Radius");

        if (!TryParseMoneyString(part.PartCost, out decimal unitPrice)) {
            unitPrice = 0M;
        }
        string room = GetRoomName(part);

        string sku = "";
        if (isTransition) {
            sku = "PCDT";
        } else {
            sku = part.VertHand == "T" ? "PC" : "PE";
        }

        Dimension width = Dimension.FromInches(part.Depth);
        Dimension length = Dimension.FromInches(part.Height);
        ClosetMaterial material = new(part.Color, ClosetMaterialCore.ParticleBoard);
        ClosetPaint? paint = null;
        string edgeBandingColor = part.InfoRecords
                                        .Where(i => i.PartName == "Edge Banding") // i.CornerShelfSizes contains the information about what edges to apply banding
                                        .Select(i => i.Color)
                                        .FirstOrDefault() ?? part.Color;
        string comment = "";

        bool finLeft = (isTransition && leftDrilling < rightDrilling) || (!isTransition && part.VertHand == "L");
        bool finRight = (isTransition && rightDrilling < leftDrilling) || (!isTransition && part.VertHand == "R");

        Dictionary<string, string> parameters = new() {
            { "FINLEFT", finLeft ? "1" : "0" },
            { "FINRIGHT", finRight ? "1" : "0" },
            { "ExtendBack", wallHasBacking ? "19.05" : "0" },
            { "BottomNotchD", "0" },
            { "BottomNotchH", "0" },
            { "WallMount", isWallMount ? "1" : "0" },
            { "BottomRadius", hasRadiusBottom ? Settings.VerticalPanelBottomRadius.AsMillimeters().ToString() : "0" },
        };

        if (isTransition) {
            var middleHoles = Dimension.FromInches(double.Min(leftDrilling, rightDrilling)) - Dimension.FromMillimeters(37);
            parameters.Add("MiddleHoles", middleHoles.AsMillimeters().ToString());
        }

        return new ClosetPart(Guid.NewGuid(), part.Quantity, unitPrice, part.PartNum, room, sku, width, length, material, paint, edgeBandingColor, comment, parameters);

    }

    public static IProduct CreateVerticalIslandPanelFromPart(Part part, bool wallHasBacking) {

        if (part.VertHand == "T") {
            throw new InvalidOperationException("Through drilled island panels are not supported");
        }

        if (wallHasBacking) {
            throw new InvalidOperationException("Cannot create vertical island panel with backing");
        }

        if (!TryParseMoneyString(part.PartCost, out decimal unitPrice)) {
            unitPrice = 0M;
        }

        string room = GetRoomName(part);

        Dimension width = Dimension.FromInches(part.Depth);
        Dimension length = Dimension.FromInches(part.Height);
        ClosetMaterial material = new(part.Color, ClosetMaterialCore.ParticleBoard);
        ClosetPaint? paint = null;
        string edgeBandingColor = part.InfoRecords
                                        .Where(i => i.PartName == "Edge Banding") // i.CornerShelfSizes contains the information about what edges to apply banding
                                        .Select(i => i.Color)
                                        .FirstOrDefault() ?? part.Color;
        string comment = "";


        var leftDrilling = Dimension.FromInches(part.VertDrillL);
        var rightDrilling = Dimension.FromInches(part.VertDrillR);

        bool finLeft = part.VertHand == "L";
        bool finRight = part.VertHand == "R";

        var sku = part.VertHand == "T" ? "PIC" : "PIE";

        Dimension row1Holes = (finLeft ? rightDrilling : leftDrilling) - Dimension.FromMillimeters(37);

        Dictionary<string, string> parameters = new() {
            { "FINLEFT", finLeft ? "1" : "0" },
            { "FINRIGHT", finRight ? "1" : "0" },
            { "Row1Holes", row1Holes.AsMillimeters().ToString() }, // TODO: what does it look like when there is a island center panel (probably both vert drill ll and vert drill r are set), and is it even possible to have a center island panel in closet pro??
            { "Row3Holes", "0" } // Optional drawer slide holes
        };

        return new ClosetPart(Guid.NewGuid(), part.Quantity, unitPrice, part.PartNum, room, sku, width, length, material, paint, edgeBandingColor, comment, parameters);

    }

    public IProduct CreateToeKickFromPart(Part part, bool wallHasBacking) {

        if (!TryParseMoneyString(part.PartCost, out decimal unitPrice)) {
            unitPrice = 0M;
        }
        string room = GetRoomName(part);
        string sku = Settings.ToeKickSKU;
        Dimension width = Dimension.FromInches(part.Height);
        Dimension length = Dimension.FromInches(part.Width);
        ClosetMaterial material = new(part.Color, ClosetMaterialCore.ParticleBoard);
        ClosetPaint? paint = null;
        string edgeBandingColor = part.InfoRecords
                                        .Where(i => i.PartName == "Edge Banding")
                                        .Select(i => i.Color)
                                        .FirstOrDefault() ?? part.Color;
        string comment = "";
        Dictionary<string, string> parameters = new();

        return new ClosetPart(Guid.NewGuid(), part.Quantity, unitPrice, part.PartNum, room, sku, width, length, material, paint, edgeBandingColor, comment, parameters);

    }

    public IProduct CreateFixedShelfFromPart(Part part, bool wallHasBacking) => CreateFixedShelfFromPart(part, wallHasBacking, false);

    public IProduct CreateFixedShelfFromPart(Part part, bool wallHasBacking, bool extendBack) {

        if (!TryParseMoneyString(part.PartCost, out decimal unitPrice)) {
            unitPrice = 0M;
        }
        string room = GetRoomName(part);
        Dimension width = Dimension.FromInches(part.Depth);
        Dimension length = Dimension.FromInches(part.Width);
        ClosetMaterial material = new(part.Color, ClosetMaterialCore.ParticleBoard);
        ClosetPaint? paint = null;
        string edgeBandingColor = part.InfoRecords
                                .Where(i => i.PartName == "Edge Banding")
                                .Select(i => i.Color)
                                .FirstOrDefault() ?? part.Color;
        string comment = "";

        Dictionary<string, string> parameters = new();

        string sku;
        if (part.ExportName == "L Fixed Shelf") {

            var dimensions = ParseCornerShelfDimensions(part.CornerShelfSizes);
            var left = dimensions[0];
            var right = dimensions[3];

            sku = Settings.LFixedShelfSKU;
            parameters.Add("RightWidth", left.AsMillimeters().ToString());
            parameters.Add("NotchSideLength", right.AsMillimeters().ToString());
            parameters.Add("NotchLeft", "Y");
            parameters.Add("ShelfRadius", Settings.LShelfRadius.AsMillimeters().ToString());

        } else if (part.ExportName == "Pie Fixed Shelf") {

            var dimensions = ParseCornerShelfDimensions(part.CornerShelfSizes);
            var left = dimensions[0];
            var right = dimensions[3];

            sku = Settings.DiagonalFixedShelfSKU;
            parameters.Add("RightWidth", left.AsMillimeters().ToString());
            parameters.Add("NotchSideLength", right.AsMillimeters().ToString());
            parameters.Add("NotchLeft", "Y");

        } else {

            sku = Settings.FixedShelfSKU;

        }

        if (extendBack || (wallHasBacking && (part.PartName == "Top Fixed Shelf" || part.PartName == "Bottom Fixed Shelf"))) {
            parameters.Add("ExtendBack", "19.05");
        }

        return new ClosetPart(Guid.NewGuid(), part.Quantity, unitPrice, part.PartNum, room, sku, width, length, material, paint, edgeBandingColor, comment, parameters);

    }

    public IProduct CreateAdjustableShelfFromPart(Part part, bool wallHasBacking) => CreateAdjustableShelfFromPart(part, wallHasBacking, false);

    public IProduct CreateAdjustableShelfFromPart(Part part, bool wallHasBacking, bool extendBack) {

        if (!TryParseMoneyString(part.PartCost, out decimal unitPrice)) {
            unitPrice = 0M;
        }
        string room = GetRoomName(part);
        Dimension width = Dimension.FromInches(part.Depth);
        Dimension length = Dimension.FromInches(part.Width);
        ClosetMaterial material = new(part.Color, ClosetMaterialCore.ParticleBoard);
        ClosetPaint? paint = null;
        string edgeBandingColor = part.InfoRecords
                                .Where(i => i.PartName == "Edge Banding")
                                .Select(i => i.Color)
                                .FirstOrDefault() ?? part.Color;
        string comment = "";
        Dictionary<string, string> parameters = new();

        string sku;
        // TODO: When the right and left sides of a corner shelf are not same length the notch should go on the longer side to give it extra support.
        if (part.ExportName == "L Adj Shelf") {

            var dimensions = ParseCornerShelfDimensions(part.CornerShelfSizes);
            var left = dimensions[0];
            var right = dimensions[3];

            sku = Settings.LAdjustableShelfSKU;
            parameters.Add("RightWidth", left.AsMillimeters().ToString());
            parameters.Add("NotchSideLength", right.AsMillimeters().ToString());
            parameters.Add("NotchLeft", "Y");
            parameters.Add("ShelfRadius", Settings.LShelfRadius.AsMillimeters().ToString());

        } else if (part.ExportName == "Pie Adj Shelf") {

            var dimensions = ParseCornerShelfDimensions(part.CornerShelfSizes);
            var left = dimensions[0];
            var right = dimensions[3];

            sku = Settings.DiagonalAdjustableShelfSKU;
            parameters.Add("RightWidth", left.AsMillimeters().ToString());
            parameters.Add("NotchSideLength", right.AsMillimeters().ToString());
            parameters.Add("NotchLeft", "Y");

        } else {

            sku = Settings.AdjustableShelfSKU;

        }

        // While not all types of adjustable shelves need to have the extended back parameter set, some do (SA5)
        if (extendBack) {
            parameters.Add("ExtendBack", "19.05");
        }

        return new ClosetPart(Guid.NewGuid(), part.Quantity, unitPrice, part.PartNum, room, sku, width, length, material, paint, edgeBandingColor, comment, parameters);

    }

    public IProduct CreateDividerShelfFromPart(Part part, int dividerCount, bool isBottom, bool wallHasBacking) {

        // TODO: get the drilling type from ClosetProSettings
        var drillingType = HorizontalDividerPanelEndDrillingType.DoubleCams;

        string sku = $"SF-D{dividerCount}{(isBottom ? "B" : "T")}{GetDividerShelfSuffix(drillingType)}";

        if (!TryParseMoneyString(part.PartCost, out decimal unitPrice)) {
            unitPrice = 0M;
        }
        string room = GetRoomName(part);
        Dimension width = Dimension.FromInches(part.Depth);
        Dimension length = Dimension.FromInches(part.Width);
        ClosetMaterial material = new(part.Color, ClosetMaterialCore.ParticleBoard);
        ClosetPaint? paint = null;
        string edgeBandingColor = part.InfoRecords
                                .Where(i => i.PartName == "Edge Banding")
                                .Select(i => i.Color)
                                .FirstOrDefault() ?? part.Color;
        string comment = "";
        Dictionary<string, string> parameters = new() {
            { "Div1", "0" },
            { "Div2", "0" },
            { "Div3", "0" },
            { "Div4", "0" },
            { "Div5", "0" }
        };

        return new ClosetPart(Guid.NewGuid(), part.Quantity, unitPrice, part.PartNum, room, sku, width, length, material, paint, edgeBandingColor, comment, parameters);

    }

    public IProduct CreateDividerPanelFromPart(Part part, bool wallHasBacking) {

        // TODO: get the drilling type from ClosetProSettings
        var drillingType = VerticalDividerPanelEndDrillingType.DoubleCams;

        string sku = $"PCDV{GetDividerPanelSuffix(drillingType)}";

        if (!TryParseMoneyString(part.PartCost, out decimal unitPrice)) {
            unitPrice = 0M;
        }
        string room = GetRoomName(part);
        Dimension width = Dimension.FromInches(part.Depth);
        Dimension length = Dimension.FromInches(part.Height);
        ClosetMaterial material = new(part.Color, ClosetMaterialCore.ParticleBoard);
        ClosetPaint? paint = null;
        string edgeBandingColor = part.InfoRecords
                                .Where(i => i.PartName == "Edge Banding")
                                .Select(i => i.Color)
                                .FirstOrDefault() ?? part.Color;
        string comment = "";
        Dictionary<string, string> parameters = new();

        return new ClosetPart(Guid.NewGuid(), part.Quantity, unitPrice, part.PartNum, room, sku, width, length, material, paint, edgeBandingColor, comment, parameters);


    }

    public static IProduct CreateShoeShelfFromPart(Part part, bool wallHasBacking) {

        if (!TryParseMoneyString(part.PartCost, out decimal unitPrice)) {
            unitPrice = 0M;
        }
        string room = GetRoomName(part);
        Dimension width = Dimension.FromInches(part.Depth);
        Dimension length = Dimension.FromInches(part.Width);
        ClosetMaterial material = new(part.Color, ClosetMaterialCore.ParticleBoard);
        ClosetPaint? paint = null;
        string edgeBandingColor = part.InfoRecords
                                .Where(i => i.PartName == "Edge Banding")
                                .Select(i => i.Color)
                                .FirstOrDefault() ?? part.Color;
        string comment = "";
        Dictionary<string, string> parameters = new();

        string sku = width.AsInches() switch {
            12 => "SS12-TAG",
            14 => "SS14-TAG",
            16 => "SS16-TAG",
            _ => "SS12-TAG" // TODO: Add custom depth shoe shelves
        };

        return new ClosetPart(Guid.NewGuid(), part.Quantity, unitPrice, part.PartNum, room, sku, width, length, material, paint, edgeBandingColor, comment, parameters);

    }

    public static IProduct CreateCleatPart(Part part, bool wallHasBacking) {

        if (!TryParseMoneyString(part.PartCost, out decimal unitPrice)) {
            unitPrice = 0M;
        }
        string room = GetRoomName(part);
        string sku = "NL1";
        Dimension width = Dimension.FromInches(part.Height);
        Dimension length = Dimension.FromInches(part.Width);
        ClosetMaterial material = new(part.Color, ClosetMaterialCore.ParticleBoard);
        ClosetPaint? paint = null;
        string edgeBandingColor = part.InfoRecords
                                .Where(i => i.PartName == "Edge Banding")
                                .Select(i => i.Color)
                                .FirstOrDefault() ?? part.Color;
        string comment = "";
        Dictionary<string, string> parameters = new();

        return new ClosetPart(Guid.NewGuid(), part.Quantity, unitPrice, part.PartNum, room, sku, width, length, material, paint, edgeBandingColor, comment, parameters);

    }

    public IProduct CreateSlabFront(Part part, bool wallHasBacking) {

        if (!TryParseMoneyString(part.PartCost, out decimal unitPrice)) {
            unitPrice = 0M;
        }
        string room = GetRoomName(part);
        ClosetMaterial material = new(part.Color, ClosetMaterialCore.ParticleBoard);
        ClosetPaint? paint = null;
        string edgeBandingColor = part.Color;
        string comment = "";
        Dictionary<string, string> parameters = new();

        string sku;
        Dimension length;
        Dimension width;
        if (part.PartName == "Cab Door Insert") {
            sku = "DOOR";
            length = Dimension.FromInches(part.Height);
            width = Dimension.FromInches(part.Width);
        } else {
            sku = "DF-XX";
            length = Dimension.FromInches(part.Width);
            width = Dimension.FromInches(part.Height);
            parameters.Add("PullCenters", HardwareSpread.AsMillimeters().ToString());
        }

        return new ClosetPart(Guid.NewGuid(), part.Quantity, unitPrice, part.PartNum, room, sku, width, length, material, paint, edgeBandingColor, comment, parameters);

    }

    public IProduct CreateDoweledDrawerBox(Part part, bool wallHasBacking) {

        if (!TryParseMoneyString(part.PartCost, out decimal unitPrice)) {
            unitPrice = 0M;
        }
        string room = GetRoomName(part);
        int productNumber = part.PartNum;
        var height = Dimension.FromInches(part.Height);
        var width = Dimension.FromInches(part.Width);
        var depth = Dimension.FromInches(part.Depth);

        string matName = Settings.DoweledDrawerBoxMaterialFinish;

        var matThickness = Dimension.FromInches(0.625);
        var material = new DoweledDrawerBoxMaterial(matName, matThickness, true);
        var botMatThickness = Dimension.FromInches(0.5);
        var botMaterial = new DoweledDrawerBoxMaterial(matName, botMatThickness, true);

        bool isUndermount = part.PartName.Contains("Undermount");
        var heightAdj = Dimension.FromMillimeters(1);

        return new DoweledDrawerBoxProduct(Guid.NewGuid(), unitPrice, part.Quantity, room, productNumber, height, width, depth, material, material, material, botMaterial, isUndermount, heightAdj);

    }

    public IProduct CreateDovetailDrawerBox(Part part, bool wallHasBacking) {

        string notch = "Standard Notch";
        DrawerSlideType slideType = DrawerSlideType.UnderMount;
        if (part.PartName.Contains("Sidemount")) {
            slideType = DrawerSlideType.SideMount;
            notch = "No Notch";
        }

        if (!TryParseMoneyString(part.PartCost, out decimal unitPrice)) {
            unitPrice = 0M;
        }
        string room = GetRoomName(part);
        string materialName = DovetailDrawerBoxConfig.FINGER_JOINT_BIRCH;
        string bottomMaterial = "1/4\" Ply";
        string clips = "Blum";
        string accessory = "None";
        bool scoopFront = part.ExportName == "Scoop Front Box";

        return _factory.CreateDovetailDrawerBoxBuilder()
            .WithOptions(new(materialName, materialName, materialName, bottomMaterial, clips, notch, accessory, LogoPosition.None, scoopFront: scoopFront))
            .WithDrawerFaceHeight(Dimension.FromInches(part.Height))
            .WithInnerCabinetWidth(Dimension.FromInches(part.Width), 1, slideType)
            .WithInnerCabinetDepth(Dimension.FromInches(part.Depth), slideType, false)
            .WithQty(part.Quantity)
            .WithProductNumber(part.PartNum)
            .BuildProduct(unitPrice, room);

    }

    public static IProduct CreateTopFromPart(Part part, bool wallHasBacking) {

        if (!TryParseMoneyString(part.PartCost, out decimal unitPrice)) {
            unitPrice = 0M;
        }
        string room = GetRoomName(part);
        string sku = "TOP";
        Dimension width = Dimension.FromInches(part.Width); // TODO: need to choose width / depth correctly so graining is going in the right direction
        Dimension length = Dimension.FromInches(part.Depth);
        ClosetMaterial material = new(part.Color, ClosetMaterialCore.ParticleBoard);
        ClosetPaint? paint = null;
        string edgeBandingColor = part.InfoRecords
                                .Where(i => i.PartName == "Edge Banding")
                                .Select(i => i.Color)
                                .FirstOrDefault() ?? part.Color;
        string comment = "";
        Dictionary<string, string> parameters = new();

        return new ClosetPart(Guid.NewGuid(), part.Quantity, unitPrice, part.PartNum, room, sku, width, length, material, paint, edgeBandingColor, comment, parameters);

    }

    public static IProduct CreateZargenDrawerBox(Part part, bool wallHasBacking) {
        throw new NotImplementedException("Zargen drawer boxes are not yet supported");
    }

    public static string GetRoomName(Part part) => $"Wall {part.WallNum} Sec {part.SectionNum}";

    public static bool TryParseMoneyString(string text, out decimal value) {
        return decimal.TryParse(text.Replace("$", ""), out value);
    }

    public static string GetDividerShelfSuffix(HorizontalDividerPanelEndDrillingType type) => type switch {
        HorizontalDividerPanelEndDrillingType.FiveMM => "5",
        HorizontalDividerPanelEndDrillingType.FiveMMSingleCams => "5C",
        HorizontalDividerPanelEndDrillingType.FiveMMDoubleCams => "5C-D",
        HorizontalDividerPanelEndDrillingType.SingleCams => "",
        HorizontalDividerPanelEndDrillingType.DoubleCams => "-D",
        _ => ""
    };

    public static string GetDividerPanelSuffix(VerticalDividerPanelEndDrillingType type) => type switch {
        VerticalDividerPanelEndDrillingType.DoubleCams => "-CAM-D",
        VerticalDividerPanelEndDrillingType.SingleCams => "-CAM",
        VerticalDividerPanelEndDrillingType.NoCams => "",
        _ => ""
    };

    public static Dimension[] ParseCornerShelfDimensions(string cornerShelfSizes) {

        var dimensions = cornerShelfSizes.Split('|')
                            .Select(double.Parse)
                            .Select(Dimension.FromInches)
                            .ToArray();

        if (dimensions.Length != 4) {
            throw new InvalidOperationException($"Unexpected corner shelf dimensions '{cornerShelfSizes}'");
        }

        return dimensions;

    }

    public static IProduct CreateFillerPanel(Part part, bool wallHasBacking) {

        if (!TryParseMoneyString(part.PartCost, out decimal unitPrice)) {
            unitPrice = 0M;
        }
        string room = GetRoomName(part);
        ClosetMaterial material = new(part.Color, ClosetMaterialCore.ParticleBoard);
        ClosetPaint? paint = null;
        string edgeBandingColor = part.Color;
        string comment = "";
        Dictionary<string, string> parameters = new();

        string sku = "NL1";
        Dimension length = Dimension.FromInches(part.Height);
        // TODO: make nailer depth a changeable setting
        Dimension width = Dimension.FromInches(4);

        return new ClosetPart(Guid.NewGuid(), part.Quantity, unitPrice, part.PartNum, room, sku, width, length, material, paint, edgeBandingColor, comment, parameters);

    }

    public static IProduct CreateBackingPart(Part part, bool wallHasBacking) {

        if (!TryParseMoneyString(part.PartCost, out decimal unitPrice)) {
            unitPrice = 0M;
        }
        string room = GetRoomName(part);
        string sku = "BK34";
        Dimension width = Dimension.FromInches(part.Width);
        Dimension length = Dimension.FromInches(part.Height);
        ClosetMaterial material = new(part.Color, ClosetMaterialCore.ParticleBoard);
        ClosetPaint? paint = null;
        string edgeBandingColor = part.InfoRecords
                                .Where(i => i.PartName == "Edge Banding")
                                .Select(i => i.Color)
                                .FirstOrDefault() ?? part.Color;
        string comment = "";
        Dictionary<string, string> parameters = new();

        return new ClosetPart(Guid.NewGuid(), part.Quantity, unitPrice, part.PartNum, room, sku, width, length, material, paint, edgeBandingColor, comment, parameters);

    }

}
