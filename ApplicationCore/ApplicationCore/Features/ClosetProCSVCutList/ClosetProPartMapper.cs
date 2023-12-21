using ApplicationCore.Features.ClosetProCSVCutList.CSVModels;
using ApplicationCore.Features.ClosetProCSVCutList.Products;
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
using System.Diagnostics.CodeAnalysis;

namespace ApplicationCore.Features.ClosetProCSVCutList;

public partial class ClosetProPartMapper {

    public Dictionary<string, Func<Part, bool, IProduct>> ProductNameMappings { get; }
    public Dictionary<string, Dimension> FrontHardwareSpreads { get; }
    public Dimension HardwareSpread { get; set; } = Dimension.Zero;
    public ClosetProSettings Settings { get; set; } = new();
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

            } else if (part.PartName == "Cab Door Rail" || part.PartName.Contains("Drawer") && part.PartName.Contains("Rail")) {

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

        if (GroupLikeParts) {

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

        var cubby = accum.CreateCubby();

        return cubby.GetProducts(Settings);

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

    public IProduct CreatePanelFromPart(Part part) {
        var panel = CreateFiller(part);
        return panel.ToProduct(Settings);
    }

    public IProduct CreateFrontFromParts(Part rail, Part insert) {

        if (rail.ExportName.Contains("MDF")) {

            var door = CreateMDFFront(rail, insert, HardwareSpread);
            return door.ToProduct();

        } else {

            var door = CreateFivePieceFront(rail, insert, HardwareSpread);
            return door.ToProduct();

        }

    }

    public IProduct CreateVerticalHutchPanelFromPart(Part part, bool wallHasBacking) {
        var panel = CreateHutchVerticalPanel(part, wallHasBacking);
        return panel.ToProduct(Settings.VerticalPanelBottomRadius);
    }

    public IProduct CreateVerticalPanelFromPart(Part part, bool wallHasBacking) {

        if (part.PartName == "Vertical Panel - Island") {
            var panel = CreateIslandVerticalPanel(part);
            return panel.ToProduct();
        }

        double leftDrilling = part.VertDrillL;
        double rightDrilling = part.VertDrillR;
        bool isTransition = leftDrilling != 0 && rightDrilling != 0 && leftDrilling != rightDrilling;

        if (isTransition) {

            var panel = CreateTransitionVerticalPanel(part, wallHasBacking);
            return panel.ToProduct(Settings.VerticalPanelBottomRadius);

        } else {

            var panel = CreateTransitionVerticalPanel(part, wallHasBacking);
            return panel.ToProduct(Settings.VerticalPanelBottomRadius);

        }

    }

    public IProduct CreateToeKickFromPart(Part part, bool wallHasBacking) {
        var toeKick = CreateToeKick(part);
        return toeKick.ToProduct(Settings);
    }

    public IProduct CreateFixedShelfFromPart(Part part, bool wallHasBacking) => CreateFixedShelfFromPart(part, wallHasBacking, false);

    public IProduct CreateFixedShelfFromPart(Part part, bool wallHasBacking, bool extendBack) {

        if (part.ExportName == "L Fixed Shelf") {

            CornerShelf shelf = CreateLFixedShelf(part);
            return shelf.ToProduct(Settings);

        } else if (part.ExportName == "Pie Fixed Shelf") {

            CornerShelf shelf = CreateDiagonalFixedShelf(part);
            return shelf.ToProduct(Settings);

        } else {

            Shelf shelf = CreateFixedShelf(part, extendBack, wallHasBacking);
            return shelf.ToProduct(Settings);

        }

    }

    public IProduct CreateAdjustableShelfFromPart(Part part, bool wallHasBacking) => CreateAdjustableShelfFromPart(part, wallHasBacking, false);

    public IProduct CreateAdjustableShelfFromPart(Part part, bool wallHasBacking, bool extendBack) {

        if (part.ExportName == "L Adj Shelf") {

            CornerShelf shelf = CreateLAdjustableShelf(part);
            return shelf.ToProduct(Settings);

        } else if (part.ExportName == "Pie Adj Shelf") {

            CornerShelf shelf = CreateDiagonalAdjustableShelf(part);
            return shelf.ToProduct(Settings);

        } else {

            Shelf shelf = CreateFixedShelf(part, extendBack, wallHasBacking);
            return shelf.ToProduct(Settings);

        }

    }

    public IProduct CreateShoeShelfFromPart(Part part, bool wallHasBacking) {
        var shelf = CreateShoeShelf(part, false, wallHasBacking);
        return shelf.ToProduct(Settings);
    }

    public IProduct CreateCleatPart(Part part, bool wallHasBacking) {
        var cleat = CreateCleat(part);
        return cleat.ToProduct(Settings);
    }

    public IProduct CreateSlabFront(Part part, bool wallHasBacking) {
        var front = CreateSlabFront(part, HardwareSpread);
        return front.ToProduct();
    }

    public IProduct CreateDoweledDrawerBox(Part part, bool wallHasBacking) {
        var box = CreateDowelDrawerBox(part);
        return box.ToDowelDrawerBox(Settings);
    }

    public IProduct CreateDovetailDrawerBox(Part part, bool wallHasBacking) {
        var box = CreateDovetailDrawerBox(part);
        return box.ToDovetailDrawerBox(_factory);
    }

    public static IProduct CreateZargenDrawerBox(Part part, bool wallHasBacking) {
        throw new NotImplementedException("Zargen drawer boxes are not yet supported");
    }

    public IProduct CreateTopFromPart(Part part, bool wallHasBacking) {
        var top = CreateTop(part);
        return top.ToProduct(Settings);
    }

    public IProduct CreateFillerPanel(Part part, bool wallHasBacking) {
        var filler = CreateFiller(part);
        return filler.ToProduct(Settings);
    }

    public IProduct CreateBackingPart(Part part, bool wallHasBacking) {
        var backing = CreateFiller(part);
        return backing.ToProduct(Settings);
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

    public class ClosetPartComparer : IEqualityComparer<ClosetPart> {

        public bool Equals(ClosetPart? x, ClosetPart? y) {

            if (x is null && y is null) return true;
            if (x is not null && y is null) return false;
            if (x is null && y is not null) return false;

            if (x!.UnitPrice != y!.UnitPrice) return false;
            if (x!.Room != y!.Room) return false;
            if (x!.SKU != y!.SKU) return false;
            if (x.Width != y.Width) return false;
            if (x.Length != y.Length) return false;
            if (x.Material != y.Material) return false;
            if (x.Paint != y.Paint) return false;
            if (x.EdgeBandingColor != y.EdgeBandingColor) return false;
            if (x.Comment != y.Comment) return false;

            if (x.ProductionNotes.Count != y.ProductionNotes.Count
                || !x.ProductionNotes.All(y.ProductionNotes.Contains)) return false;

            if (x.Parameters.Keys.Count != y.Parameters.Keys.Count
                || !x.Parameters.Keys.All(k => y.Parameters.ContainsKey(k) && Equals(y.Parameters[k], x.Parameters[k]))) {
                return false;
            }

            return true;

        }

        public int GetHashCode([DisallowNull] ClosetPart obj) {
            // TODO: do something about this
            return 0;
        }
    }

}

