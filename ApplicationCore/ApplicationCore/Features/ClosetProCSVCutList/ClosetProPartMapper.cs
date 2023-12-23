using ApplicationCore.Features.ClosetProCSVCutList.CSVModels;
using ApplicationCore.Features.ClosetProCSVCutList.Products;
using ApplicationCore.Features.Companies.Contracts.ValueObjects;
using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.Products.Closets;
using ApplicationCore.Shared;
using ApplicationCore.Shared.Domain;
using System.Diagnostics.CodeAnalysis;

namespace ApplicationCore.Features.ClosetProCSVCutList;

public partial class ClosetProPartMapper {

    public Dictionary<string, Func<Part, bool, IClosetProProduct>> ProductNameMappings { get; }
    public Dimension HardwareSpread { get; set; } = Dimension.Zero;
    public bool GroupLikeParts { get; set; } = false;

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

    }

    public List<IClosetProProduct> MapPartsToProducts(IEnumerable<Part> parts) {

        return parts.GroupBy(p => p.WallNum)
                    .SelectMany(GetPartsForWall)
                    .ToList();

    }

    private IEnumerable<IClosetProProduct> GetPartsForWall(IEnumerable<Part> parts) {

        List<IClosetProProduct> products = [];

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

            } else if (part.PartName == "Cab Door Rail" || part.PartName.Contains("Drawer") && part.PartName.Contains("Rail")) {

                // TODO: Cabinet door parts have a hinge direction, if that information is to be added to the product it will need to be read

                if (!enumerator.MoveNext()) {
                    throw new InvalidOperationException("Unexpected end of part list");
                }

                var insertPart = enumerator.Current;
                if (insertPart.PartName != "Cab Door Insert" && !(insertPart.PartName.Contains("Drawer") && insertPart.PartName.Contains("Insert"))) {
                    throw new InvalidOperationException("Door/Drawer rail part found without door/drawer insert");
                }

                products.Add(CreateFrontFromParts(part, insertPart, HardwareSpread));
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

        if (GroupLikeParts) {

            /*
            var closetParts = products.Where(p => p is ClosetPart)
                                      .Cast<ClosetPart>()
                                      .ToList();

            products.RemoveAll(p => p is ClosetPart);

            var groupedParts = closetParts.GroupBy(p => p, new ClosetPartComparer())
                                            .Select(g => {

                                                var totalQty = g.Sum(g => g.Qty);

                                                var first = g.OrderBy(p => p.ProductNumber).First();
                                                first.Qty = totalQty;

                                                return first;

                                            });

            products.AddRange(groupedParts);
            products.OrderBy(p => p.ProductNumber);
            */

        }

        return products;

    }

    private IEnumerable<IClosetProProduct> CreateCubbyProducts(IEnumerator<Part> enumerator, Part part, Part firstCubbyPart, bool doesWallHaveBacking) {

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

        var cubby = accum.CreateCubby();

        var prods = new List<IClosetProProduct>() {
            cubby.TopDividerShelf,
            cubby.BottomDividerShelf,
        };

        prods.AddRange(cubby.DividerPanels);
        prods.AddRange(cubby.FixedShelves);
            
        return prods;


    }

    public static List<OtherPart> MapPickListToItems(IEnumerable<PickPart> parts, Dictionary<string, Dimension> frontHardwareSpreads, out Dimension hardwareSpread) {

        hardwareSpread = Dimension.Zero;

        List<Dimension> spreads = [];

        List<OtherPart> items = [];
        foreach (var item in parts) {

            if (!TryParseMoneyString(item.Cost, out var cost)) {
                cost = 0;
            }

            items.Add(new() {
                Qty = item.Quantity,
                Name = item.Name,
                UnitPrice = cost
            });

            if (item.Type == "Pull/Knob") {
                // TODO: check pick list for "Pull/Knob" part types, if there is only one then the drilling spacing for drawer fronts can be inferred from that, if there are multiple then spacing cannot be inferred 
                if (frontHardwareSpreads.TryGetValue(item.Name, out Dimension spread)) {
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

    public static List<OtherPart> MapAccessoriesToItems(IEnumerable<Accessory> accessories) {

        List<OtherPart> items = [];

        foreach (var accessory in accessories) {

            if (!TryParseMoneyString(accessory.Cost, out var cost)) {
                cost = 0;
            }

            items.Add(new() {
                Qty = accessory.Quantity,
                Name = accessory.Name,
                UnitPrice = (cost / (decimal) accessory.Quantity)
            });

        }

        return items;

    }

    public static List<OtherPart> MapBuyOutPartsToItems(IEnumerable<BuyOutPart> parts) {

        List<OtherPart> items = [];

        foreach (var part in parts) {

            if (!TryParseMoneyString(part.PartCost, out decimal unitPrice)) {
                unitPrice = 0M;
            }

            string name;
            if (part.PartName == "Hang Rod") {
                name = $"{part.PartName} - {part.Color} - {part.Width}\"L";
            } else {
                name = part.PartName;
            }

            items.Add(new() {
                Qty = part.Quantity,
                Name = name,
                UnitPrice = (unitPrice / (decimal)part.Quantity)
            });

        }

        return items;

    }

    public static IClosetProProduct CreatePanelFromPart(Part part) => CreateFiller(part);

    public static IClosetProProduct CreateFrontFromParts(Part rail, Part insert, Dimension hardwareSpread) {

        if (rail.ExportName.Contains("MDF")) {

            return CreateMDFFront(rail, insert, hardwareSpread);

        } else {

            return CreateFivePieceFront(rail, insert, hardwareSpread);

        }

    }

    public static IClosetProProduct CreateVerticalHutchPanelFromPart(Part part, bool wallHasBacking) => CreateHutchVerticalPanel(part, wallHasBacking);

    public static IClosetProProduct CreateVerticalPanelFromPart(Part part, bool wallHasBacking) {

        if (part.PartName == "Vertical Panel - Island") {
            return CreateIslandVerticalPanel(part);
        }

        double leftDrilling = part.VertDrillL;
        double rightDrilling = part.VertDrillR;
        bool isTransition = leftDrilling != 0 && rightDrilling != 0 && leftDrilling != rightDrilling;

        if (isTransition) {

            return CreateTransitionVerticalPanel(part, wallHasBacking);

        } else {

            return CreateTransitionVerticalPanel(part, wallHasBacking);

        }

    }

    public static IClosetProProduct CreateToeKickFromPart(Part part, bool wallHasBacking) => CreateToeKick(part);

    public static IClosetProProduct CreateFixedShelfFromPart(Part part, bool wallHasBacking) => CreateFixedShelfFromPart(part, wallHasBacking, false);

    public static IClosetProProduct CreateFixedShelfFromPart(Part part, bool wallHasBacking, bool extendBack) {

        if (part.ExportName == "L Fixed Shelf") {

            return CreateLFixedShelf(part);

        } else if (part.ExportName == "Pie Fixed Shelf") {

            return CreateDiagonalFixedShelf(part);

        } else {

            return CreateFixedShelf(part, extendBack, wallHasBacking);

        }

    }

    public static IClosetProProduct CreateAdjustableShelfFromPart(Part part, bool wallHasBacking) => CreateAdjustableShelfFromPart(part, wallHasBacking, false);

    public static IClosetProProduct CreateAdjustableShelfFromPart(Part part, bool wallHasBacking, bool extendBack) {

        if (part.ExportName == "L Adj Shelf") {

            return CreateLAdjustableShelf(part);

        } else if (part.ExportName == "Pie Adj Shelf") {

            return CreateDiagonalAdjustableShelf(part);

        } else {

            return CreateFixedShelf(part, extendBack, wallHasBacking);

        }

    }

    public static IClosetProProduct CreateShoeShelfFromPart(Part part, bool wallHasBacking) => CreateShoeShelf(part, false, wallHasBacking);

    public static IClosetProProduct CreateCleatPart(Part part, bool wallHasBacking) => CreateCleat(part);

    public IClosetProProduct CreateSlabFront(Part part, bool wallHasBacking) => CreateSlabFront(part, HardwareSpread);

    public static IClosetProProduct CreateDoweledDrawerBox(Part part, bool wallHasBacking) => CreateDowelDrawerBox(part);

    public static IClosetProProduct CreateDovetailDrawerBox(Part part, bool wallHasBacking) => CreateDovetailDrawerBox(part);

    public static IClosetProProduct CreateZargenDrawerBox(Part part, bool wallHasBacking) => CreateZargenDrawerBox();

    public static IClosetProProduct CreateTopFromPart(Part part, bool wallHasBacking) => CreateTop(part);

    public static IClosetProProduct CreateFillerPanel(Part part, bool wallHasBacking) => CreateFiller(part);

    public static IClosetProProduct CreateBackingPart(Part part, bool wallHasBacking) => CreateFiller(part);

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

