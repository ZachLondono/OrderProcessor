using ApplicationCore.Features.Orders.Shared.Domain;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Orders.Loader.Providers.AllmoxyXMLModels;
using ApplicationCore.Features.Shared.Domain;
using System.Xml.Serialization;
using ApplicationCore.Features.Orders.Loader.XMLValidation;
using System.Text;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using MoreLinq;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Loader.Dialog;
using ApplicationCore.Features.Orders.Shared.Domain.Builders;

namespace ApplicationCore.Features.Orders.Loader.Providers;

internal class AllmoxyXMLOrderProvider : IOrderProvider {

    private readonly AllmoxyConfiguration _configuration;
    private readonly AllmoxyClientFactory _clientfactory;
    private readonly IXMLValidator _validator;

    public IOrderLoadingViewModel? OrderLoadingViewModel { get; set; }

    public AllmoxyXMLOrderProvider(AllmoxyConfiguration configuration, AllmoxyClientFactory clientfactory, IXMLValidator validator) {
        _configuration = configuration;
        _clientfactory = clientfactory;
        _validator = validator;
    }

    public Task<OrderData?> LoadOrderData(string source) {

        // Load order to a string
        string exportXML = _clientfactory.CreateClient().GetExport(source, 6);

        // Validate data
        if (!ValidateData(exportXML)) {
            OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, "Order data was not valid");
            return Task.FromResult<OrderData?>(null);
        }

        // Deserialize data
        OrderModel? data = DeserializeData(exportXML);
        if (data is null) {
            OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, "Could not find order information in given data");
            return Task.FromResult<OrderData?>(null);
        }

        ShippingInfo shipping = new() {
            Contact = data.Shipping.Attn,
            Method = data.Shipping.Method,
            PhoneNumber = "",
            Price = StringToMoney(data.Invoice.Shipping),
            Address = new Address() {
                Line1 = data.Shipping.Address.Line1,
                Line2 = data.Shipping.Address.Line2,
                Line3 = data.Shipping.Address.Line3,
                City = data.Shipping.Address.City,
                State = data.Shipping.Address.State,
                Zip = data.Shipping.Address.Zip,
                Country = data.Shipping.Address.Country
            }
        };

        Customer customer = new() {
            Name = data.Customer.Company
        };

        var info = new Dictionary<string, string>() {
            { "Notes", data.Note },
            { "Shipping Attn", data.Shipping.Attn },
            { "Shipping Instructions", data.Shipping.Instructions },
            { "Allmoxy Customer Id", data.Customer.CompanyId.ToString() }
        };

        DateTime orderDate = ParseOrderDate(data.OrderDate);

        var billing = new BillingInfo() {
            InvoiceEmail = null,
            PhoneNumber = "",
            Address = new()
        };

        OrderData? order = new() {
            Number = data.Number.ToString(),
            Name = data.Name,
            Comment = data.Description,
            Shipping = shipping,
            Billing = billing,
            Tax = StringToMoney(data.Invoice.Tax),
            PriceAdjustment = 0M,
            OrderDate = orderDate,
            Customer = customer,
            VendorId = Guid.Parse(_configuration.VendorId),
            AdditionalItems = new(),
            Products = MapProductsFromData(data),
            Rush = data.Shipping.Method.Contains("Rush"),
            Info = info
        };

        return Task.FromResult<OrderData?>(order);

    }

    public bool ValidateData(string data) {

        try {

            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));
            var errors = _validator.ValidateXML(stream, _configuration.SchemaFilePath);

            errors.ForEach(error => OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, $"[XML Error] [{error.Severity}] {error.Exception.Message}"));

            return !errors.Any();

        } catch {

            return false;

        }

    }

    private static decimal StringToMoney(string str) => decimal.Parse(str.Replace("$", "").Replace(",", ""));

    private DateTime ParseOrderDate(string orderDateStr) {

        if (DateTime.TryParse(orderDateStr, out DateTime orderDate)) {
            return orderDate;
        }

        OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Warning, $"Could not parse order date '{(orderDateStr == "" ? "[BLANK]" : orderDateStr)}'");

        return DateTime.Now;

    }

    private List<IProduct> MapProductsFromData(OrderModel data) {
        List<IProduct> products = new();

        data.Products
            .BaseCabinets
            .ForEach(c => MapAndAddProduct(c, MapToBaseCabinet, products));

        data.Products
            .WallCabinets
            .ForEach(c => MapAndAddProduct(c, MapToWallCabinet, products));

        data.Products
            .DrawerBaseCabinets
            .ForEach(c => MapAndAddProduct(c, MapToDrawerBaseCabinet, products));

        data.Products
            .TallCabinets
            .ForEach(c => MapAndAddProduct(c, MapToTallCabinet, products));

        data.Products
            .BasePieCutCornerCabinets
            .ForEach(c => MapAndAddProduct(c, MapToBasePieCutCabinet, products));

        data.Products
            .WallPieCutCornerCabinets
            .ForEach(c => MapAndAddProduct(c, MapToWallPieCutCabinet, products));

        data.Products
            .BaseDiagonalCornerCabinets
            .ForEach(c => MapAndAddProduct(c, MapToBaseDiagonalCabinet, products));

        data.Products
            .WallDiagonalCornerCabinets
            .ForEach(c => MapAndAddProduct(c, MapToWallDiagonalCabinet, products));

        data.Products
            .SinkCabinets
            .ForEach(c => MapAndAddProduct(c, MapToSinkCabinet, products));

        data.Products
            .BlindBaseCabinets
            .ForEach(c => MapAndAddProduct(c, MapToBlindBaseCabinet, products));

        data.Products
            .BlindWallCabinets
            .ForEach(c => MapAndAddProduct(c, MapToBlindWallCabinet, products));

        return products;
    }

    private void MapAndAddProduct<T>(T data, Func<T, IProduct> mapper, List<IProduct> products) {

        try {

            var product = mapper(data);
            products.Add(product);

        } catch (Exception ex) {

            OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, $"Could not load product {ex.Message}");

        }

    }

    public static BaseCabinet MapToBaseCabinet(BaseCabinetModel model) {

        MDFDoorOptions? mdfOptions = null;
        if (model.Cabinet.Fronts.Type != "Slab") mdfOptions = new(model.Cabinet.Fronts.Style, model.Cabinet.Fronts.Color);

        bool hingeLeft = (model.HingeSide == "Left");
        BaseCabinetDoors doors = model.DoorQty switch {
            0 => BaseCabinetDoors.NoDoors(),
            1 => new(hingeLeft ? HingeSide.Left : HingeSide.Right, mdfOptions),
            2 => new(mdfOptions),
            _ => new(hingeLeft ? HingeSide.Left : HingeSide.Right, mdfOptions)
        };
        HorizontalDrawerBank drawers = new() {
            BoxMaterial = GetDrawerMaterial(model.DrawerMaterial),
            FaceHeight = Dimension.FromMillimeters(model.DrawerFaceHeight),
            Quantity = model.DrawerQty,
            SlideType = GetDrawerSlideType(model.DrawerSlide)
        };

        BaseCabinetInside inside;
        Dimension[] rollOutBoxPositions = GetRollOutPositions(model.RollOuts.Pos1, model.RollOuts.Pos2, model.RollOuts.Pos3, model.RollOuts.Pos4, model.RollOuts.Pos5);
        RollOutBlockPosition rollOutBlocks = GetRollOutBlockPositions(model.RollOuts.Blocks);
        if (rollOutBoxPositions.Length != 0) {
            var rollOutOptions = new RollOutOptions(rollOutBoxPositions, true, rollOutBlocks, drawers.SlideType, drawers.BoxMaterial);
            inside = new(model.AdjShelfQty, rollOutOptions, GetShelfDepth(model.ShelfDepth));
        } else inside = new(model.AdjShelfQty, model.VerticalDividerQty, GetShelfDepth(model.ShelfDepth));

        var toeType = GetToeType(model.ToeType);

        return InitilizeBuilder<BaseCabinetBuilder, BaseCabinet>(new(), model)
            .WithInside(inside)
            .WithToeType(toeType)
            .WithDoors(doors)
            .WithDrawers(drawers)
            .Build();

    }

    public static WallCabinet MapToWallCabinet(WallCabinetModel model) {

        MDFDoorOptions? mdfOptions = null;
        if (model.Cabinet.Fronts.Type != "Slab") mdfOptions = new(model.Cabinet.Fronts.Style, model.Cabinet.Fronts.Color);

        bool hingeLeft = (model.HingeSide == "Left");
        Dimension doorExtendDown = Dimension.FromMillimeters(model.ExtendDoorDown);
        WallCabinetDoors doors = model.DoorQty switch {
            0 => WallCabinetDoors.NoDoors(),
            1 => new(hingeLeft ? HingeSide.Left : HingeSide.Right, doorExtendDown, mdfOptions),
            2 => new(doorExtendDown, mdfOptions),
            _ => new(hingeLeft ? HingeSide.Left : HingeSide.Right, doorExtendDown, mdfOptions)
        };
        
        WallCabinetInside inside = new(model.AdjShelfQty, model.VerticalDividerQty);
        bool finishBottom = (model.FinishedBottom == "Yes");

        return InitilizeBuilder<WallCabinetBuilder, WallCabinet>(new(), model)
            .WithDoors(doors)
            .WithInside(inside)
            .WithFinishBottom(finishBottom)
            .Build();


    }

    public static DrawerBaseCabinet MapToDrawerBaseCabinet(DrawerBaseCabinetModel model) {

        var drawerFaces = new Dimension[model.DrawerQty == 1 ? 0 : model.DrawerQty];
        if (model.DrawerQty > 1) drawerFaces[0] = Dimension.FromMillimeters(model.DrawerFace1); // For 1 drawer box cabinets, the drawer box size is calculated
        if (model.DrawerQty >= 2) drawerFaces[1] = Dimension.FromMillimeters(model.DrawerFace2);
        if (model.DrawerQty >= 3) drawerFaces[2] = Dimension.FromMillimeters(model.DrawerFace3);
        if (model.DrawerQty >= 4) drawerFaces[3] = Dimension.FromMillimeters(model.DrawerFace4);
        if (model.DrawerQty >= 5) drawerFaces[4] = Dimension.FromMillimeters(model.DrawerFace5);
 
        MDFDoorOptions? mdfOptions = null;
        if (model.Cabinet.Fronts.Type != "Slab") mdfOptions = new(model.Cabinet.Fronts.Style, model.Cabinet.Fronts.Color);

        VerticalDrawerBank verticalDrawerBank = new() {
            BoxMaterial = GetDrawerMaterial(model.DrawerMaterial),
            FaceHeights = drawerFaces,
            SlideType = GetDrawerSlideType(model.DrawerSlide)
        };

        return InitilizeBuilder<DrawerBaseCabinetBuilder, DrawerBaseCabinet>(new(), model)
            .WithToeType(GetToeType(model.ToeType))
            .WithDrawers(verticalDrawerBank)
            .WithFronts(mdfOptions)
            .Build();

    }

    public static TallCabinet MapToTallCabinet(TallCabinetModel model) {

        MDFDoorOptions? mdfOptions = null;
        if (model.Cabinet.Fronts.Type != "Slab") mdfOptions = new(model.Cabinet.Fronts.Style, model.Cabinet.Fronts.Color);

        TallCabinetInside inside;
        Dimension[] rollOutBoxPositions = GetRollOutPositions(model.RollOuts.Pos1, model.RollOuts.Pos2, model.RollOuts.Pos3, model.RollOuts.Pos4, model.RollOuts.Pos5);
        RollOutBlockPosition rollOutBlocks = GetRollOutBlockPositions(model.RollOuts.Blocks);
        if (rollOutBoxPositions.Length != 0) {
            var rollOutOptions = new RollOutOptions(rollOutBoxPositions, true, rollOutBlocks, GetDrawerSlideType(model.DrawerSlide), GetDrawerMaterial(model.DrawerMaterial));
            inside = new(model.UpperAdjShelfQty, model.LowerAdjShelfQty, model.UpperVerticalDividerQty, rollOutOptions);
        } else inside = new(model.UpperAdjShelfQty, model.LowerAdjShelfQty, model.UpperVerticalDividerQty, model.LowerVerticalDividerQty);

        TallCabinetDoors doors;
        HingeSide hingeSide = model.LowerDoorQty == 1 ? ((model.HingeSide == "Left") ? HingeSide.Left : HingeSide.Right) : HingeSide.NotApplicable;
        if (model.LowerDoorQty == 0) {
            doors = TallCabinetDoors.NoDoors();
        } else if (model.UpperDoorQty != 0) {
            doors = new(Dimension.FromMillimeters(model.LowerDoorHeight), hingeSide, mdfOptions);
        } else {
            doors = new(hingeSide, mdfOptions);
        }

        return InitilizeBuilder<TallCabinetBuilder, TallCabinet>(new(), model)
            .WithDoors(doors)
            .WithToeType(GetToeType(model.ToeType))
            .WithInside(inside)
            .Build();

    }

    public static BasePieCutCornerCabinet MapToBasePieCutCabinet(BasePieCutCornerCabinetModel model) {

        MDFDoorOptions? mdfOptions = null;
        if (model.Cabinet.Fronts.Type != "Slab") mdfOptions = new(model.Cabinet.Fronts.Style, model.Cabinet.Fronts.Color);

        return InitilizeBuilder<BasePieCutCornerCabinetBuilder, BasePieCutCornerCabinet>(new(), model)
                .WithRightWidth(Dimension.FromMillimeters(model.RightWidth))
                .WithRightDepth(Dimension.FromMillimeters(model.RightDepth))
                .WithToeType(GetToeType(model.ToeType))
                .WithAdjustableShelves(model.AdjShelfQty)
                .WithHingeSide((model.HingeSide == "Left") ? HingeSide.Left : HingeSide.Right)
                .WithMDFOptions(model.Cabinet.Fronts.Type == "Slab" ? null : mdfOptions)
                .Build();

    }

    public static WallPieCutCornerCabinet MapToWallPieCutCabinet(WallPieCutCornerCabinetModel model) {

        MDFDoorOptions? mdfOptions = null;
        if (model.Cabinet.Fronts.Type != "Slab") mdfOptions = new(model.Cabinet.Fronts.Style, model.Cabinet.Fronts.Color);

        return InitilizeBuilder<WallPieCutCornerCabinetBuilder, WallPieCutCornerCabinet>(new(), model)
                .WithRightWidth(Dimension.FromMillimeters(model.RightWidth))
                .WithRightDepth(Dimension.FromMillimeters(model.RightDepth))
                .WithAdjustableShelves(model.AdjShelfQty)
                .WithHingeSide((model.HingeSide == "Left") ? HingeSide.Left : HingeSide.Right)
                .WithMDFOptions(model.Cabinet.Fronts.Type == "Slab" ? null : mdfOptions)
                .Build();

    }

    public static BaseDiagonalCornerCabinet MapToBaseDiagonalCabinet(BaseDiagonalCornerCabinetModel model) {

        MDFDoorOptions? mdfOptions = null;
        if (model.Cabinet.Fronts.Type != "Slab") mdfOptions = new(model.Cabinet.Fronts.Style, model.Cabinet.Fronts.Color);

        return InitilizeBuilder<BaseDiagonalCornerCabinetBuilder, BaseDiagonalCornerCabinet>(new(), model)
                .WithRightWidth(Dimension.FromMillimeters(model.RightWidth))
                .WithRightDepth(Dimension.FromMillimeters(model.RightDepth))
                .WithToeType(GetToeType(model.ToeType))
                .WithAdjustableShelves(model.AdjShelfQty)
                .WithHingeSide((model.HingeSide == "Left") ? HingeSide.Left : HingeSide.Right)
                .WithDoorQty(model.DoorQty)
                .WithMDFOptions(model.Cabinet.Fronts.Type == "Slab" ? null : mdfOptions)
                .Build();

    }

    public static WallDiagonalCornerCabinet MapToWallDiagonalCabinet(WallDiagonalCornerCabinetModel model) {

        MDFDoorOptions? mdfOptions = null;
        if (model.Cabinet.Fronts.Type != "Slab") mdfOptions = new(model.Cabinet.Fronts.Style, model.Cabinet.Fronts.Color);

        return InitilizeBuilder<WallDiagonalCornerCabinetBuilder, WallDiagonalCornerCabinet>(new(), model)
            .WithRightWidth(Dimension.FromMillimeters(model.RightWidth))
            .WithRightDepth(Dimension.FromMillimeters(model.RightDepth))
            .WithAdjustableShelves(model.AdjShelfQty)
            .WithHingeSide((model.HingeSide == "Left") ? HingeSide.Left : HingeSide.Right)
            .WithDoorQty(model.DoorQty)
            .WithMDFOptions(model.Cabinet.Fronts.Type == "Slab" ? null : mdfOptions)
            .Build();

    }

    public static SinkCabinet MapToSinkCabinet(SinkCabinetModel model) {

        MDFDoorOptions? mdfOptions = null;
        if (model.Cabinet.Fronts.Type != "Slab") mdfOptions = new(model.Cabinet.Fronts.Style, model.Cabinet.Fronts.Color);

        var rollOutOptions = new RollOutOptions(Array.Empty<Dimension>(), true, RollOutBlockPosition.None, GetDrawerSlideType(model.DrawerSlide), GetDrawerMaterial(model.DrawerMaterial));

        return InitilizeBuilder<SinkCabinetBuilder, SinkCabinet>(new(), model)
            .WithRollOutBoxes(rollOutOptions)
            .WithToeType(GetToeType(model.ToeType))
            .WithHingeSide((model.HingeSide == "Left") ? HingeSide.Left : HingeSide.Right)
            .WithDoorQty(model.DoorQty)
            .WithFalseDrawerQty(model.DrawerQty)
            .WithDrawerFaceHeight(Dimension.FromMillimeters(model.DrawerFaceHeight))
            .WithAdjustableShelves(model.AdjShelfQty)
            .WithMDFOptions(model.Cabinet.Fronts.Type == "Slab" ? null : mdfOptions)
            .Build();

    }

    public static BlindBaseCabinet MapToBlindBaseCabinet(BlindBaseCabinetModel model) {

        MDFDoorOptions? mdfOptions = null;
        if (model.Cabinet.Fronts.Type != "Slab") mdfOptions = new(model.Cabinet.Fronts.Style, model.Cabinet.Fronts.Color);

        bool hingeLeft = (model.HingeSide == "Left");
        BlindCabinetDoors doors = model.DoorQty switch {
            1 => new(hingeLeft ? HingeSide.Left : HingeSide.Right, mdfOptions),
            2 => new(mdfOptions),
            _ => new(hingeLeft ? HingeSide.Left : HingeSide.Right, mdfOptions)
        };

        HorizontalDrawerBank drawers = new() {
            BoxMaterial = GetDrawerMaterial(model.DrawerMaterial),
            FaceHeight = Dimension.FromMillimeters(model.DrawerFaceHeight),
            Quantity = model.DrawerQty,
            SlideType = GetDrawerSlideType(model.DrawerSlide)
        };

        var blindSide = (model.BlindSide == "Left" ? BlindSide.Left : BlindSide.Right);

        return InitilizeBuilder<BlindBaseCabinetBuilder, BlindBaseCabinet>(new(), model)
                .WithBlindSide(blindSide)
                .WithBlindWidth(Dimension.FromMillimeters(model.BlindWidth))
                .WithAdjustableShelves(model.AdjShelfQty)
                .WithDrawers(drawers)
                .WithToeType(GetToeType(model.ToeType))
                .WithDoors(doors)
                .Build();

    }

    public static BlindWallCabinet MapToBlindWallCabinet(BlindWallCabinetModel model) {

        MDFDoorOptions? mdfOptions = null;
        if (model.Cabinet.Fronts.Type != "Slab") mdfOptions = new(model.Cabinet.Fronts.Style, model.Cabinet.Fronts.Color);

        bool hingeLeft = (model.HingeSide == "Left");
        BlindCabinetDoors doors = model.DoorQty switch {
            1 => new(hingeLeft ? HingeSide.Left : HingeSide.Right, mdfOptions),
            2 => new(mdfOptions),
            _ => new(hingeLeft ? HingeSide.Left : HingeSide.Right, mdfOptions)
        };            

        return InitilizeBuilder<BlindWallCabinetBuilder, BlindWallCabinet>(new(), model)
                    .WithDoors(doors)
                    .WithAdjustableShelves(model.AdjShelfQty)
                    .WithBlindSide(model.BlindSide == "Left" ? BlindSide.Left : BlindSide.Right)
                    .WithBlindWidth(Dimension.FromMillimeters(model.BlindWidth))
                    .Build();

    }

    public static TBuilder InitilizeBuilder<TBuilder, TCabinet>(TBuilder builder, CabinetModelBase model) where TBuilder :CabinetBuilder<TCabinet> where TCabinet : Cabinet {

        CabinetMaterialCore boxCore = GetMaterialCore(model.Cabinet.BoxMaterial.Type);
        CabinetMaterialCore finishCore = GetFinishedSideMaterialCore(model.Cabinet.FinishMaterial.Type, boxCore);

        MDFDoorOptions? mdfOptions = null;
        if (model.Cabinet.Fronts.Type != "Slab") mdfOptions = new(model.Cabinet.Fronts.Style, model.Cabinet.Fronts.Color);

        string finishColor = (model.Cabinet.FinishMaterial.Type == "paint" ? model.Cabinet.BoxMaterial.Finish : model.Cabinet.FinishMaterial.Finish);
        CabinetMaterial boxMaterial = new(model.Cabinet.BoxMaterial.Finish, boxCore);
        CabinetMaterial finishMaterial = new(finishColor, finishCore);
        CabinetSide leftSide = new(GetCabinetSideType(model.Cabinet.LeftSide), mdfOptions);
        CabinetSide rightSide = new(GetCabinetSideType(model.Cabinet.RightSide), mdfOptions);

        string edgeBandingColor = (model.Cabinet.EdgeBandColor == "Match Finish" ? model.Cabinet.FinishMaterial.Finish : model.Cabinet.EdgeBandColor);


        return (TBuilder) builder.WithQty(model.Cabinet.Qty)
                                    .WithUnitPrice(StringToMoney(model.Cabinet.UnitPrice))
                                    .WithBoxMaterial(boxMaterial)
                                    .WithFinishMaterial(finishMaterial)
                                    .WithLeftSide(leftSide)
                                    .WithRightSide(rightSide)
                                    .WithEdgeBandingColor(edgeBandingColor)
                                    .WithWidth(Dimension.FromMillimeters(model.Cabinet.Width))
                                    .WithHeight(Dimension.FromMillimeters(model.Cabinet.Height))
                                    .WithDepth(Dimension.FromMillimeters(model.Cabinet.Depth))
                                    .WithRoom(model.Cabinet.Room)
                                    .WithAssembled(model.Cabinet.Assembled == "Yes");

    }

    private static OrderModel? DeserializeData(string xmlString) {
        var serializer = new XmlSerializer(typeof(OrderModel));
        using var reader = new StringReader(xmlString);
        if (serializer.Deserialize(reader) is OrderModel data) {
            return data;
        }
        return null;
    }

    public static CabinetDrawerBoxMaterial GetDrawerMaterial(string name) => name switch {
        "Pre-Finished Birch" => CabinetDrawerBoxMaterial.SolidBirch,
        "Economy Birch" => CabinetDrawerBoxMaterial.FingerJointBirch,
        _ => CabinetDrawerBoxMaterial.FingerJointBirch
    };

    public static DrawerSlideType GetDrawerSlideType(string name) => name switch {
        "Under Mount" => DrawerSlideType.UnderMount,
        "Side Mount" => DrawerSlideType.SideMount,
        _ => DrawerSlideType.UnderMount
    };

    public static RollOutBlockPosition GetRollOutBlockPositions(string name) => name switch {
        "Left" => RollOutBlockPosition.Left,
        "Right" => RollOutBlockPosition.Right,
        "Both" => RollOutBlockPosition.Both,
        _ => RollOutBlockPosition.None
    };

    public static IToeType GetToeType(string name) => name switch {
        "Leg Levelers" => new LegLevelers(Dimension.FromMillimeters(102)),
        "Full Height Sides" => new FurnitureBase(Dimension.FromMillimeters(102)),
        "No Toe" => new NoToe(),
        "Notched" => new Notched(Dimension.FromMillimeters(102)),
        _ => new LegLevelers(Dimension.FromMillimeters(102))
    };

    public static CabinetSideType GetCabinetSideType(string name) => name switch {
        "Unfinished" => CabinetSideType.Unfinished,
        "Finished" => CabinetSideType.Finished,
        "Integrated" => CabinetSideType.IntegratedPanel,
        "Applied" => CabinetSideType.AppliedPanel,
        _ => CabinetSideType.Unfinished
    };

    public static CabinetMaterialCore GetMaterialCore(string name) => name switch {
        "pb" => CabinetMaterialCore.Flake,
        "ply" => CabinetMaterialCore.Plywood,
        _ => CabinetMaterialCore.Flake
    };

    public static ShelfDepth GetShelfDepth(string name) => name switch {
        "Full" => ShelfDepth.Full,
        "Half" => ShelfDepth.Half,
        "3/4" => ShelfDepth.ThreeQuarters,
        _ => ShelfDepth.Default
    };

    public static Dimension[] GetRollOutPositions(string pos1, string pos2, string pos3, string pos4, string pos5) {

        int count = 0;
        if (pos5 == "Yes") count = 5;
        else if (pos4 == "Yes") count = 4;
        else if (pos3 == "Yes") count = 3;
        else if (pos2 == "Yes") count = 2;
        else if (pos1 == "Yes") count = 1;

        if (count == 0) return Array.Empty<Dimension>();

        var positions = new Dimension[count];
        if (count >= 1) positions[0] = pos1 == "Yes" ? Dimension.FromMillimeters(19) : Dimension.Zero;
        if (count >= 2) positions[1] = pos2 == "Yes" ? Dimension.FromMillimeters(300) : Dimension.Zero;
        if (count >= 3) positions[2] = pos3 == "Yes" ? (count >= 4 ? Dimension.FromMillimeters(581) : Dimension.FromMillimeters(497)) : Dimension.Zero;
        if (count >= 4) positions[3] = pos4 == "Yes" ? Dimension.FromMillimeters(862) : Dimension.Zero;
        if (count >= 5) positions[4] = pos5 == "Yes" ? Dimension.FromMillimeters(1142) : Dimension.Zero;

        return positions;

    }

    public static CabinetMaterialCore GetFinishedSideMaterialCore(string name, CabinetMaterialCore boxMaterial) {

        if (boxMaterial == CabinetMaterialCore.Flake) {

            return name switch {
                "veneer" => CabinetMaterialCore.Plywood,
                _ => CabinetMaterialCore.Flake
            };

        } else if (boxMaterial == CabinetMaterialCore.Plywood) {

            return CabinetMaterialCore.Plywood;

        }

        return CabinetMaterialCore.Flake;
    }

}
