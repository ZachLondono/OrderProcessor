using ApplicationCore.Features.Companies.Contracts.ValueObjects;
using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.OrderLoading.LoadClosetProOrderData.Models;

public class ClosetProPartMapper {

    public Dictionary<string, Func<Part, IProduct>> ProductNameMappings { get; }
    public Dictionary<string, Dimension> FrontHardwareSpreads { get; }
    public Dimension HardwareSpread { get; set; } = Dimension.Zero;
    public ClosetProSettings Settings { get; set; } = new();

    private readonly ComponentBuilderFactory _factory;

    public ClosetProPartMapper(ComponentBuilderFactory factory) {

        _factory = factory;

        ProductNameMappings = new() {
            { "CPS FM Vert", CreateVerticalPanelFromPart },
            { "CPS WM Vert", CreateVerticalPanelFromPart },
            { "VP-Corner Floor Mount", CreateVerticalPanelFromPart },
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
            { "Slab", CreateSlabFront }
        };

        FrontHardwareSpreads = new();

    }
    
    public List<IProduct> MapPartsToProducts(IEnumerable<Part> parts) {

        List<IProduct> products = new();
        var enumerator = parts.GetEnumerator();
        if (!enumerator.MoveNext()) {
            throw new InvalidOperationException("No products found in order");
        }
        Part part = enumerator.Current;
        while (true) {

            if (part.PartName == "Cab Door Rail") {

                if (!enumerator.MoveNext()) {
                    throw new InvalidOperationException("Unexpected end of part list");
                }

                var insertPart = enumerator.Current;
                if (insertPart.PartName != "Cab Door Insert") {
                    throw new InvalidOperationException("Cab door rail part found without cab door insert");
                }

                products.Add(CreateFrontFromParts(part, insertPart));
                if (enumerator.MoveNext()) {
                    part = enumerator.Current;
                    continue;
                } else {
                    break;
                }

            }

            Part? nextPart = null;
            if (part.ExportName == "FixedShelf") {

                if (enumerator.MoveNext()) {

                    var cubbyPart = enumerator.Current;
                    if (cubbyPart.ExportName == "Cubby-V" || cubbyPart.ExportName == "Cubby-H") {

                        var accum = new CubbyAccumulator();
                        accum.AddBottomShelf(part);
                        if (cubbyPart.ExportName == "Cubby-V") {
                            accum.AddVerticalPanel(cubbyPart);
                        } else {
                            accum.AddHorizontalPanel(cubbyPart);
                        }

                        while(enumerator.MoveNext()) {
                            cubbyPart = enumerator.Current;
                            if (cubbyPart.ExportName == "Cubby-V") {
                                accum.AddVerticalPanel(cubbyPart);
                            } else if (cubbyPart.ExportName == "Cubby-H") {
                                accum.AddHorizontalPanel(cubbyPart);
                            } else if (cubbyPart.ExportName == "FixedShelf") {
                                accum.AddTopShelf(cubbyPart);
                                break;
                            }
                        }

                        products.AddRange(accum.GetProducts(this));

                        if (enumerator.MoveNext()) {
                            part = enumerator.Current;
                            continue;
                        } else {
                            break;
                        }

                    } else {
                        nextPart = cubbyPart;
                    }

                }

            }
            
            if (ProductNameMappings.TryGetValue(part.ExportName, out var mapper)) {
                products.Add(mapper(part));
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

    public List<AdditionalItem> MapAccessoriesToItems(IEnumerable<Accessory> accessories) {
 
        List<AdditionalItem> items = new();

        foreach (var accessory in accessories) {

            if (!TryParseMoneyString(accessory.Cost, out var cost)) {
                cost = 0;
            }

            items.Add(new(Guid.NewGuid(), $"({accessory.Quantity}) {accessory.Name}", cost));

        }

        return items;

    }

    public List<AdditionalItem> MapBuyOutPartsToItems(IEnumerable<BuyOutPart> parts) {

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

    public IProduct CreateFrontFromParts(Part rail, Part insert) {
        // Doors are described in two parts a "Rail" and an "Insert"
        throw new NotImplementedException();
    }

    public static IProduct CreateVerticalPanelFromPart(Part part) {

        double leftDrilling = part.VertDrillL;
        double rightDrilling = part.VertDrillR;

        bool isTransition = leftDrilling != 0 && rightDrilling != 0 && leftDrilling != rightDrilling;

        bool isWallMount = part.ExportName.Contains("WM");

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
            { "ExtendBack", "0" },
            { "BottomNotchD", "0" },
            { "BottomNotchH", "0" },
            { "WallMount", isWallMount ? "1" : "0" },
        };

        if (isTransition) {
            var middleHoles = Dimension.FromInches(double.Min(leftDrilling, rightDrilling)) - Dimension.FromMillimeters(37);
            parameters.Add("MiddleHoles", middleHoles.AsMillimeters().ToString());
        }

        return new ClosetPart(Guid.NewGuid(), part.Quantity, unitPrice, part.PartNum, room, sku, width, length, material, paint, edgeBandingColor, comment, parameters);

    }

    public IProduct CreateToeKickFromPart(Part part) {

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

    public IProduct CreateFixedShelfFromPart(Part part) {

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

            sku = "SFL19"; // TODO: get sku from ClosetProSettings
            parameters.Add("RightWidth", left.AsMillimeters().ToString());
            parameters.Add("NotchSideLength", right.AsMillimeters().ToString());
            parameters.Add("NotchLeft", "Y");
            parameters.Add("ShelfRadius", "0"); // TODO: get radius from ClosetProSettings

        } else if (part.ExportName == "Pie Fixed Shelf") {

            var dimensions = ParseCornerShelfDimensions(part.CornerShelfSizes);
            var left = dimensions[0];
            var right = dimensions[3];

            sku = "SFD19"; // TODO: get sku from ClosetProSettings
            parameters.Add("RightWidth", left.AsMillimeters().ToString());
            parameters.Add("NotchSideLength", right.AsMillimeters().ToString());
            parameters.Add("NotchLeft", "Y");

        } else {

            sku = Settings.FixedShelfSKU;

        }

        return new ClosetPart(Guid.NewGuid(), part.Quantity, unitPrice, part.PartNum, room, sku, width, length, material, paint, edgeBandingColor, comment, parameters);

    }

    public IProduct CreateAdjustableShelfFromPart(Part part) {

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
        if (part.ExportName == "L Adj Shelf") {

            var dimensions = ParseCornerShelfDimensions(part.CornerShelfSizes);
            var left = dimensions[0];
            var right = dimensions[3];

            sku = "SAL19"; // TODO: get sku from ClosetProSettings
            parameters.Add("RightWidth", left.AsMillimeters().ToString());
            parameters.Add("NotchSideLength", right.AsMillimeters().ToString());
            parameters.Add("NotchLeft", "Y");
            parameters.Add("ShelfRadius", "0"); // TODO: get radius from ClosetProSettings

        } else if(part.ExportName == "Pie Adj Shelf") {

            var dimensions = ParseCornerShelfDimensions(part.CornerShelfSizes);
            var left = dimensions[0];
            var right = dimensions[3];

            sku = "SAD19"; // TODO: get sku from ClosetProSettings
            parameters.Add("RightWidth", left.AsMillimeters().ToString());
            parameters.Add("NotchSideLength", right.AsMillimeters().ToString());
            parameters.Add("NotchLeft", "Y");

        } else {

            sku = Settings.AdjustableShelfSKU;

        }

        return new ClosetPart(Guid.NewGuid(), part.Quantity, unitPrice, part.PartNum, room, sku, width, length, material, paint, edgeBandingColor, comment, parameters);

    }

    public IProduct CreateDividerShelfFromPart(Part part, int dividerCount, bool isBottom) {

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

    public IProduct CreateDividerPanelFromPart(Part part) {

        // TODO: get the drilling type from ClosetProSettings
        var drillingType = VerticalDividerPanelEndDrillingType.DoubleCams;

        string sku = $"PEDV{GetDividerPanelSuffix(drillingType)}";

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

    public static IProduct CreateShoeShelfFromPart(Part part) {

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

    public static IProduct CreateCleatPart(Part part) {

        if (!TryParseMoneyString(part.PartCost, out decimal unitPrice)) {
            unitPrice = 0M;
        }
        string room = GetRoomName(part);
        string sku = "CLEAT";
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

    public IProduct CreateSlabFront(Part part) {

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

    public static IProduct CreateDoweledDrawerBox(Part part) {

        if (!TryParseMoneyString(part.PartCost, out decimal unitPrice)) {
            unitPrice = 0M;
        }
        string room = GetRoomName(part);
        ClosetMaterial material = new(part.Color, ClosetMaterialCore.ParticleBoard);
        ClosetPaint? paint = null;
        string edgeBandingColor = part.Color;
        string comment = "";
        Dictionary<string, string> parameters = new();
        Dimension length = Dimension.FromInches(part.Height);
        Dimension width = Dimension.FromInches(part.Width);

        return new ClosetPart(Guid.NewGuid(), part.Quantity, unitPrice, part.PartNum, room, "Mela Side Mount", width, length, material, paint, edgeBandingColor, comment, parameters);

    }

    public IProduct CreateDovetailDrawerBox(Part part) {

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
        string materialName = DrawerBoxOptions.FINGER_JOINT_BIRCH;
        string bottomMaterial = "1/4\" Ply";
        string clips = "Blum";
        string accessory = "None";
        bool scoopFront = part.PartName == "Scoop Front Box";

        return _factory.CreateDovetailDrawerBoxBuilder()
            .WithOptions(new(materialName, materialName, materialName, bottomMaterial, clips, notch, accessory, LogoPosition.None, scoopFront:scoopFront))
            .WithDrawerFaceHeight(Dimension.FromInches(part.Height))
            .WithInnerCabinetWidth(Dimension.FromInches(part.Width), 1, slideType)
            .WithInnerCabinetDepth(Dimension.FromInches(part.Depth), slideType, false)
            .WithQty(part.Quantity)
            .WithProductNumber(part.PartNum)
            .BuildProduct(unitPrice, room);

    }

    public static IProduct CreateZargenDrawerBox(Part part) {
        throw new NotImplementedException();
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

}
