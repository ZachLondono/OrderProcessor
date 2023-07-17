using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.OrderLoading.LoadClosetProOrderData.Models;

public class ClosetProPartMapper {

    public Dictionary<string, Func<Part, IProduct>> ProductNameMappings = new() {
        { "CPS FM Vert", CreateVerticalPanelFromPart },
        { "CPS WM Vert", CreateVerticalPanelFromPart },
        { "FixedShelf", CreateFixedShelfFromPart },
        { "AdjustableShelf", CreateAdjustableShelfFromPart },
        { "Toe Kick_3.75", CreateToeKickFromPart },
        { "Toe Kick_2.5", CreateToeKickFromPart },
        { "Cleat", CreateCleatPart }
    };
    
    public IProduct? CreateProductFromPart(Part part) {

        if (ProductNameMappings.TryGetValue(part.ExportName, out var mapper)) {
            return mapper(part);
        }

        throw new InvalidOperationException($"Unexpected part {part.PartName} / {part.ExportName}");

    }

    public AdditionalItem CreateItemFromPart(BuyOutPart part) {

        if (!TryParseMoneyString(part.PartCost, out decimal unitPrice)) {
            unitPrice = 0M;
        }

        if (part.ExportName == "Oval Chrome") {
            return new AdditionalItem(Guid.NewGuid(), $"{part.PartName} - {part.Color} - {part.Width}\"L", unitPrice);
        }

        return new AdditionalItem(Guid.NewGuid(), part.PartName, unitPrice);

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
        bool finRight = (isTransition && leftDrilling < rightDrilling) || (!isTransition && part.VertHand == "R");

        Dictionary<string, string> parameters = new() {
            { "FINLEFT", finLeft ? "1" : "0" },
            { "FINRIGHT", finRight ? "1" : "0" },
            { "ExtendBack", "0" },
            { "BottomNotchD", "0" },
            { "BottomNotchH", "0" },
            { "WallMount", isWallMount ? "1" : "0" },
        };

        return new ClosetPart(Guid.NewGuid(), part.Quantity, unitPrice, part.PartNum, room, sku, width, length, material, paint, edgeBandingColor, comment, parameters);

    }

    public static IProduct CreateToeKickFromPart(Part part) {

        if (!TryParseMoneyString(part.PartCost, out decimal unitPrice)) {
            unitPrice = 0M;
        }
        string room = GetRoomName(part);
        string sku = "TK-F";
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

    public static IProduct CreateFixedShelfFromPart(Part part) {

        if (!TryParseMoneyString(part.PartCost, out decimal unitPrice)) {
            unitPrice = 0M;
        }
        string room = GetRoomName(part);
        string sku = "SF";
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

        return new ClosetPart(Guid.NewGuid(), part.Quantity, unitPrice, part.PartNum, room, sku, width, length, material, paint, edgeBandingColor, comment, parameters);

    }

    public static IProduct CreateAdjustableShelfFromPart(Part part) {

        if (!TryParseMoneyString(part.PartCost, out decimal unitPrice)) {
            unitPrice = 0M;
        }
        string room = GetRoomName(part);
        string sku = "SA";
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

    public static IProduct CreateDrawerFace(Part part) {
        throw new NotImplementedException();
    }

    public static IProduct CreateDrawerBox(Part part) {
        throw new NotImplementedException();
    }

    public static string GetRoomName(Part part) => $"Wall {part.WallNum} Sec {part.SectionNum}";

    public static bool TryParseMoneyString(string text, out decimal value) {
        return decimal.TryParse(text.Replace("$", ""), out value);
    }

}
