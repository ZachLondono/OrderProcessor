using ApplicationCore.Features.Companies.Commands;
using ApplicationCore.Features.Companies.Domain;
using ApplicationCore.Features.Companies.Domain.ValueObjects;
using ApplicationCore.Features.Companies.Queries;
using ApplicationCore.Features.Orders.Shared.Domain;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Orders.Loader.Providers.AllmoxyXMLModels;
using ApplicationCore.Features.Orders.Loader.Providers.DTO;
using ApplicationCore.Infrastructure;
using ApplicationCore.Features.Shared.Domain;
using System.Xml.Serialization;
using ApplicationCore.Features.Orders.Loader.XMLValidation;
using System.Text;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using MoreLinq;

namespace ApplicationCore.Features.Orders.Loader.Providers;

internal class AllmoxyXMLOrderProvider : IOrderProvider {

    private readonly IBus _bus;
    private readonly AllmoxyConfiguration _configuration;
    private readonly AllmoxyClientFactory _clientfactory;
    private readonly IXMLValidator _validator;

    public IOrderLoadingViewModel? OrderLoadingViewModel { get; set; }

    public AllmoxyXMLOrderProvider(IBus bus, AllmoxyConfiguration configuration, AllmoxyClientFactory clientfactory, IXMLValidator validator) {
        _bus = bus;
        _configuration = configuration;
        _clientfactory = clientfactory;
        _validator = validator;
    }

    public async Task<OrderData?> LoadOrderData(string source) {

        // Load order to a string
        string exportXML = _clientfactory.CreateClient().GetExport(source, 6);

        // Validate data
        if (!ValidateData(exportXML)) {
            OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, "Order data was not valid");
            return null;
        }

        // Deserialize data
        OrderModel? data = DeserializeData(exportXML);
        if (data is null) {
            OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, "Could not find order information in given data");
            return null;
        }

        // Get customer company id
        Guid? customerId = await GetCustomerId(data);
        if (customerId is null) {
            OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, "Could not find/save customer information");
            return null;
        }

        var info = new Dictionary<string, string>() {
            { "Notes", data.Note },
            { "Shipping Attn", data.Shipping.Attn },
            { "Shipping Instructions", data.Shipping.Instructions },
            { "Allmoxy Customer Id", data.Customer.CompanyId.ToString() }
        };

        DateTime orderDate = ParseOrderDate(data.OrderDate);

        return new OrderData() {
            Number = data.Number.ToString(),
            Name = data.Name,
            Comment = data.Description,
            Tax = data.Invoice.Tax,
            Shipping = data.Invoice.Shipping,
            PriceAdjustment = 0M,
            OrderDate = orderDate,
            CustomerId = (Guid)customerId,
            VendorId = Guid.Parse(_configuration.VendorId),
            AdditionalItems = new(),
            Products = MapProductsFromData(data),
            Rush = data.Shipping.Method.Contains("Rush"),
            Info = info
        };

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
            .PieCutCornerCabinets
            .ForEach(c => MapAndAddProduct(c, MapToPieCutCabinet, products));

        data.Products
            .DiagonalCornerCabinets
            .ForEach(c => MapAndAddProduct(c, MapToDiagonalCabinet, products));

        data.Products
            .SinkCabinets
            .ForEach(c => MapAndAddProduct(c, MapToSinkCabinet, products));

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

    private async Task<Guid?> GetCustomerId(OrderModel data) {
        bool didError = false;
        Company? customer = null;
        var response = await _bus.Send(new GetCompanyByAllmoxyId.Query(data.Customer.CompanyId));

        response.Match(
            c => {
                customer = c;
            },
            error => {
                OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, error.Title);
                didError = true;
            }
        );

        customer ??= await CreateCustomer(data);

        if (customer is null || didError) {
            return null;
        }

        return customer.Id;
    }

    private async Task<Company?> CreateCustomer(OrderModel data) {
        
        var adderes = new Address() {
            Line1 = data.Shipping.Address.Line1,
            Line2 = data.Shipping.Address.Line2,
            Line3 = data.Shipping.Address.Line3,
            City = data.Shipping.Address.City,
            State = data.Shipping.Address.State,
            Zip = data.Shipping.Address.Zip,
            Country = data.Shipping.Address.Country
        };

        // TODO: get customer contact info from allmoxy order data
        var createResponse = await _bus.Send(new CreateCompany.Command(data.Customer.Company, adderes, "", "", "", ""));

        Company? customer = null;
        createResponse.Match(
            async c => {
                customer = c;
                await _bus.Send(new CreateAllmoxyIdCompanyIdMapping.Command(data.Customer.CompanyId, customer.Id));
            },
            error => {
                OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, error.Title);
                customer = null;
            }
        );

        return customer;

    }

    private BaseCabinet MapToBaseCabinet(BaseCabinetModel model) {

        CabinetMaterialCore boxCore = GetMaterialCore(model.Cabinet.BoxMaterial.Type);

        MDFDoorOptions? mdfOptions = null;
        if (model.Cabinet.Fronts.Type != "Slab") mdfOptions = new(model.Cabinet.Fronts.Style, model.Cabinet.Fronts.Color);

        var data = new BaseCabinetData() {
            Qty = model.Cabinet.Qty,
            UnitPrice = model.Cabinet.UnitPrice,
            Room = model.Cabinet.Room,
            Assembled = (model.Cabinet.Assembled == "Yes"),
            Height = Dimension.FromMillimeters(model.Cabinet.Height),
            Width = Dimension.FromMillimeters(model.Cabinet.Width),
            Depth = Dimension.FromMillimeters(model.Cabinet.Depth),
            BoxMaterialFinish = model.Cabinet.BoxMaterial.Finish,
            BoxMaterialCore = boxCore,
            FinishMaterialFinish = model.Cabinet.FinishMaterial.Finish,
            FinishMaterialCore = GetFinishedSideMaterialCore(model.Cabinet.FinishMaterial.Type, boxCore),
            EdgeBandingColor = (model.Cabinet.EdgeBandColor == "Match Finish" ? model.Cabinet.FinishMaterial.Finish : model.Cabinet.EdgeBandColor),
            LeftSideType = GetCabinetSideType(model.Cabinet.LeftSide),
            RightSideType = GetCabinetSideType(model.Cabinet.RightSide),
            DoorType = model.Cabinet.Fronts.Type,
            DoorStyle = mdfOptions,
            DoorQty = model.DoorQty,
            HingeLeft = (model.HingeSide == "Left"),
            ToeType = GetToeType(model.ToeType),
            DrawerQty = model.DrawerQty,
            DrawerFaceHeight = Dimension.FromMillimeters(model.DrawerFaceHeight),
            DrawerBoxMaterial = GetDrawerMaterial(model.DrawerMaterial),
            DrawerBoxSlideType = GetDrawerSlideType(model.DrawerSlide),
            VerticalDividerQty = model.VerticalDividerQty,
            AdjustableShelfQty = model.AdjShelfQty,
            RollOutBoxPositions = GetRollOutPositions(model.RollOuts.Pos1, model.RollOuts.Pos2, model.RollOuts.Pos3, "", ""),
            RollOutBlocks = GetRollOutBlockPositions(model.RollOuts.Blocks),
            ScoopFrontRollOuts = true,
            ShelfDepth = GetShelfDepth(model.ShelfDepth)
        };

        BaseCabinetDoors doors = data.DoorQty switch {
            0 => BaseCabinetDoors.NoDoors(),
            1 => new(data.HingeLeft ? HingeSide.Left : HingeSide.Right, data.DoorStyle),
            2 => new(data.DoorStyle),
            _ => new(data.HingeLeft ? HingeSide.Left : HingeSide.Right, data.DoorStyle)
        };
        CabinetMaterial boxMaterial = new(data.BoxMaterialFinish, data.BoxMaterialCore);
        CabinetMaterial finishMaterial = new(data.FinishMaterialFinish, data.FinishMaterialCore);
        CabinetSide leftSide = new(data.LeftSideType, data.DoorStyle);
        CabinetSide rightSide = new(data.RightSideType, data.DoorStyle);
        HorizontalDrawerBank drawers = new() {
            BoxMaterial = data.DrawerBoxMaterial,
            FaceHeight = data.DrawerFaceHeight,
            Quantity = data.DrawerQty,
            SlideType = data.DrawerBoxSlideType
        };

        BaseCabinetInside inside;
        if (data.RollOutBoxPositions.Length != 0) {
            var rollOutOptions = new RollOutOptions(data.RollOutBoxPositions, true, data.RollOutBlocks, data.DrawerBoxSlideType, data.DrawerBoxMaterial);
            inside = new(data.AdjustableShelfQty, rollOutOptions, data.ShelfDepth);
        } else inside = new(data.AdjustableShelfQty, data.VerticalDividerQty, data.ShelfDepth);

        return BaseCabinet.Create(
            data.Qty,
            data.UnitPrice,
            data.Room,
            data.Assembled,
            data.Height,
            data.Width,
            data.Depth,
            boxMaterial,
            finishMaterial,
            data.EdgeBandingColor,
            rightSide,
            leftSide,
            doors,
            data.ToeType,
            drawers,
            inside
        );

    }

    private WallCabinet MapToWallCabinet(WallCabinetModel model) {

        CabinetMaterialCore boxCore = GetMaterialCore(model.Cabinet.BoxMaterial.Type);

        MDFDoorOptions? mdfOptions = null;
        if (model.Cabinet.Fronts.Type != "Slab") mdfOptions = new(model.Cabinet.Fronts.Style, model.Cabinet.Fronts.Color);

        var data = new WallCabinetData() {
            Qty = model.Cabinet.Qty,
            UnitPrice = model.Cabinet.UnitPrice,
            Room = model.Cabinet.Room,
            Assembled = (model.Cabinet.Assembled == "Yes"),
            Height = Dimension.FromMillimeters(model.Cabinet.Height),
            Width = Dimension.FromMillimeters(model.Cabinet.Width),
            Depth = Dimension.FromMillimeters(model.Cabinet.Depth),
            BoxMaterialFinish = model.Cabinet.BoxMaterial.Finish,
            BoxMaterialCore = boxCore,
            FinishMaterialFinish = model.Cabinet.FinishMaterial.Finish,
            FinishMaterialCore = GetFinishedSideMaterialCore(model.Cabinet.FinishMaterial.Type, boxCore),
            EdgeBandingColor = (model.Cabinet.EdgeBandColor == "Match Finish" ? model.Cabinet.FinishMaterial.Finish : model.Cabinet.EdgeBandColor),
            LeftSideType = GetCabinetSideType(model.Cabinet.LeftSide),
            RightSideType = GetCabinetSideType(model.Cabinet.RightSide),
            DoorType = model.Cabinet.Fronts.Type,
            DoorStyle = mdfOptions,
            DoorQty = model.DoorQty,
            HingeLeft = (model.HingeSide == "Left"),
            VerticalDividerQty = model.VerticalDividerQty,
            AdjustableShelfQty = model.AdjShelfQty,
            ExtendDoorDown = Dimension.FromMillimeters(model.ExtendDoorDown),
            FinishedBottom = (model.FinishedBottom == "Yes")
        };

        WallCabinetDoors doors = data.DoorQty switch {
            0 => WallCabinetDoors.NoDoors(),
            1 => new(data.HingeLeft ? HingeSide.Left : HingeSide.Right, data.ExtendDoorDown, data.DoorStyle),
            2 => new(data.ExtendDoorDown, data.DoorStyle),
            _ => new(data.HingeLeft ? HingeSide.Left : HingeSide.Right, data.ExtendDoorDown, data.DoorStyle)
        };
        CabinetMaterial boxMaterial = new(data.BoxMaterialFinish, data.BoxMaterialCore);
        CabinetMaterial finishMaterial = new(data.FinishMaterialFinish, data.FinishMaterialCore);
        CabinetSide leftSide = new(data.LeftSideType, data.DoorStyle);
        CabinetSide rightSide = new(data.RightSideType, data.DoorStyle);

        WallCabinetInside inside = new(data.AdjustableShelfQty, data.VerticalDividerQty);

        return WallCabinet.Create(
            data.Qty,
            data.UnitPrice,
            data.Room,
            data.Assembled,
            data.Height,
            data.Width,
            data.Depth,
            boxMaterial,
            finishMaterial,
            data.EdgeBandingColor,
            rightSide,
            leftSide,
            doors,
            inside,
            data.FinishedBottom);

    }

    private DrawerBaseCabinet MapToDrawerBaseCabinet(DrawerBaseCabinetModel model) {

        CabinetMaterialCore boxCore = GetMaterialCore(model.Cabinet.BoxMaterial.Type);

        MDFDoorOptions? mdfOptions = null;
        if (model.Cabinet.Fronts.Type != "Slab") mdfOptions = new(model.Cabinet.Fronts.Style, model.Cabinet.Fronts.Color);

        var drawerFaces = new Dimension[model.DrawerQty == 1 ? 0 : model.DrawerQty];
        if (model.DrawerQty > 1) drawerFaces[0] = Dimension.FromMillimeters(model.DrawerFace1); // For 1 drawer box cabinets, the drawer box size is calculated
        if (model.DrawerQty >= 2) drawerFaces[1] = Dimension.FromMillimeters(model.DrawerFace2);
        if (model.DrawerQty >= 3) drawerFaces[2] = Dimension.FromMillimeters(model.DrawerFace3);
        if (model.DrawerQty >= 4) drawerFaces[3] = Dimension.FromMillimeters(model.DrawerFace4);
        if (model.DrawerQty >= 5) drawerFaces[4] = Dimension.FromMillimeters(model.DrawerFace5);

        var data = new DrawerBaseCabinetData() {
            Qty = model.Cabinet.Qty,
            UnitPrice = model.Cabinet.UnitPrice,
            Room = model.Cabinet.Room,
            Assembled = (model.Cabinet.Assembled == "Yes"),
            Height = Dimension.FromMillimeters(model.Cabinet.Height),
            Width = Dimension.FromMillimeters(model.Cabinet.Width),
            Depth = Dimension.FromMillimeters(model.Cabinet.Depth),
            BoxMaterialFinish = model.Cabinet.BoxMaterial.Finish,
            BoxMaterialCore = boxCore,
            FinishMaterialFinish = model.Cabinet.FinishMaterial.Finish,
            FinishMaterialCore = GetFinishedSideMaterialCore(model.Cabinet.FinishMaterial.Type, boxCore),
            EdgeBandingColor = (model.Cabinet.EdgeBandColor == "Match Finish" ? model.Cabinet.FinishMaterial.Finish : model.Cabinet.EdgeBandColor),
            LeftSideType = GetCabinetSideType(model.Cabinet.LeftSide),
            RightSideType = GetCabinetSideType(model.Cabinet.RightSide),
            DoorType = model.Cabinet.Fronts.Type,
            DoorStyle = mdfOptions,
            ToeType = GetToeType(model.ToeType),
            DrawerBoxMaterial = GetDrawerMaterial(model.DrawerMaterial),
            DrawerBoxSlideType = GetDrawerSlideType(model.DrawerSlide),
            DrawerFaces = drawerFaces.ToArray()
        };

        CabinetMaterial boxMaterial = new(data.BoxMaterialFinish, data.BoxMaterialCore);
        CabinetMaterial finishMaterial = new(data.FinishMaterialFinish, data.FinishMaterialCore);
        CabinetSide leftSide = new(data.LeftSideType, data.DoorStyle);
        CabinetSide rightSide = new(data.RightSideType, data.DoorStyle);

        VerticalDrawerBank verticalDrawerBank = new() {
            BoxMaterial = data.DrawerBoxMaterial,
            FaceHeights = data.DrawerFaces,
            SlideType = data.DrawerBoxSlideType
        };

        return DrawerBaseCabinet.Create(
            data.Qty,
            data.UnitPrice,
            data.Room,
            data.Assembled,
            data.Height,
            data.Width,
            data.Depth,
            boxMaterial,
            finishMaterial,
            data.EdgeBandingColor,
            rightSide,
            leftSide,
            data.ToeType,
            verticalDrawerBank,
            data.DoorStyle);

    }

    private TallCabinet MapToTallCabinet(TallCabinetModel model) {

        CabinetMaterialCore boxCore = GetMaterialCore(model.Cabinet.BoxMaterial.Type);

        MDFDoorOptions? mdfOptions = null;
        if (model.Cabinet.Fronts.Type != "Slab") mdfOptions = new(model.Cabinet.Fronts.Style, model.Cabinet.Fronts.Color);

        var data = new TallCabinetData() {
            Qty = model.Cabinet.Qty,
            UnitPrice = model.Cabinet.UnitPrice,
            Room = model.Cabinet.Room,
            Assembled = (model.Cabinet.Assembled == "Yes"),
            Height = Dimension.FromMillimeters(model.Cabinet.Height),
            Width = Dimension.FromMillimeters(model.Cabinet.Width),
            Depth = Dimension.FromMillimeters(model.Cabinet.Depth),
            BoxMaterialFinish = model.Cabinet.BoxMaterial.Finish,
            BoxMaterialCore = boxCore,
            FinishMaterialFinish = model.Cabinet.FinishMaterial.Finish,
            FinishMaterialCore = GetFinishedSideMaterialCore(model.Cabinet.FinishMaterial.Type, boxCore),
            EdgeBandingColor = (model.Cabinet.EdgeBandColor == "Match Finish" ? model.Cabinet.FinishMaterial.Finish : model.Cabinet.EdgeBandColor),
            LeftSideType = GetCabinetSideType(model.Cabinet.LeftSide),
            RightSideType = GetCabinetSideType(model.Cabinet.RightSide),
            DoorType = model.Cabinet.Fronts.Type,
            DoorStyle = mdfOptions,
            DrawerBoxMaterial = GetDrawerMaterial(model.DrawerMaterial),
            DrawerBoxSlideType = GetDrawerSlideType(model.DrawerSlide),
            HingeLeft = (model.HingeSide == "Left"),
            ToeType = GetToeType(model.ToeType),
            RollOutBoxPositions = GetRollOutPositions(model.RollOuts.Pos1, model.RollOuts.Pos2, model.RollOuts.Pos3, model.RollOuts.Pos4, model.RollOuts.Pos5),
            RollOutBlocks = GetRollOutBlockPositions(model.RollOuts.Blocks),
            ScoopFrontRollOuts = true,
            AdjustableShelfLowerQty = model.LowerAdjShelfQty,
            AdjustableShelfUpperQty = model.UpperAdjShelfQty,
            VerticalDividerLowerQty = model.LowerVerticalDividerQty,
            VerticalDividerUpperQty = model.UpperVerticalDividerQty,
            LowerDoorQty = model.LowerDoorQty,
            UpperDoorQty = model.UpperDoorQty,
            LowerDoorHeight = Dimension.FromMillimeters(model.LowerDoorHeight),
        };

        CabinetMaterial boxMaterial = new(data.BoxMaterialFinish, data.BoxMaterialCore);
        CabinetMaterial finishMaterial = new(data.FinishMaterialFinish, data.FinishMaterialCore);
        CabinetSide leftSide = new(data.LeftSideType, data.DoorStyle);
        CabinetSide rightSide = new(data.RightSideType, data.DoorStyle);

        TallCabinetInside inside;
        if (data.RollOutBoxPositions.Length != 0) {
            var rollOutOptions = new RollOutOptions(data.RollOutBoxPositions, true, data.RollOutBlocks, data.DrawerBoxSlideType, data.DrawerBoxMaterial);
            inside = new(data.AdjustableShelfUpperQty, data.AdjustableShelfLowerQty, data.VerticalDividerUpperQty, rollOutOptions);
        } else inside = new(data.AdjustableShelfUpperQty, data.AdjustableShelfLowerQty, data.VerticalDividerUpperQty, data.VerticalDividerLowerQty);

        TallCabinetDoors doors;
        HingeSide hingeSide = data.LowerDoorQty == 1 ? (data.HingeLeft ? HingeSide.Left : HingeSide.Right) : HingeSide.NotApplicable;
        if (data.LowerDoorQty == 0) {
            doors = TallCabinetDoors.NoDoors();
        } else if (data.UpperDoorQty != 0) {
            doors = new(data.LowerDoorHeight, hingeSide, data.DoorStyle);
        } else {
            doors = new(hingeSide, data.DoorStyle);
        }

        return TallCabinet.Create(
            data.Qty,
            data.UnitPrice,
            data.Room,
            data.Assembled,
            data.Height,
            data.Width,
            data.Depth,
            boxMaterial,
            finishMaterial,
            data.EdgeBandingColor,
            rightSide,
            leftSide,
            doors,
            data.ToeType,
            inside);

    }

    public PieCutCornerCabinet MapToPieCutCabinet(PieCutCornerCabinetModel model) {

        CabinetMaterialCore boxCore = GetMaterialCore(model.Cabinet.BoxMaterial.Type);

        MDFDoorOptions? mdfOptions = null;
        if (model.Cabinet.Fronts.Type != "Slab") mdfOptions = new(model.Cabinet.Fronts.Style, model.Cabinet.Fronts.Color);

        var data = new PieCutCornerCabinetData() {
            Qty = model.Cabinet.Qty,
            UnitPrice = model.Cabinet.UnitPrice,
            Room = model.Cabinet.Room,
            Assembled = (model.Cabinet.Assembled == "Yes"),
            Height = Dimension.FromMillimeters(model.Cabinet.Height),
            Width = Dimension.FromMillimeters(model.Cabinet.Width),
            Depth = Dimension.FromMillimeters(model.Cabinet.Depth),
            BoxMaterialFinish = model.Cabinet.BoxMaterial.Finish,
            BoxMaterialCore = boxCore,
            FinishMaterialFinish = model.Cabinet.FinishMaterial.Finish,
            FinishMaterialCore = GetFinishedSideMaterialCore(model.Cabinet.FinishMaterial.Type, boxCore),
            EdgeBandingColor = (model.Cabinet.EdgeBandColor == "Match Finish" ? model.Cabinet.FinishMaterial.Finish : model.Cabinet.EdgeBandColor),
            LeftSideType = GetCabinetSideType(model.Cabinet.LeftSide),
            RightSideType = GetCabinetSideType(model.Cabinet.RightSide),
            DoorType = model.Cabinet.Fronts.Type,
            DoorStyle = mdfOptions,
            HingeLeft = (model.HingeSide == "Left"),
            ToeType = GetToeType(model.ToeType),
            AdjustableShelfQty = model.AdjShelfQty,
            RightDepth = Dimension.FromMillimeters(model.RightDepth),
            RightWidth = Dimension.FromMillimeters(model.RightWidth)
        };

        CabinetMaterial boxMaterial = new(data.BoxMaterialFinish, data.BoxMaterialCore);
        CabinetMaterial finishMaterial = new(data.FinishMaterialFinish, data.FinishMaterialCore);
        CabinetSide leftSide = new(data.LeftSideType, data.DoorStyle);
        CabinetSide rightSide = new(data.RightSideType, data.DoorStyle);

        return PieCutCornerCabinet.Create(data.Qty,
            data.UnitPrice,
            data.Room,
            data.Assembled,
            data.Height,
            data.Width,
            data.Depth,
            boxMaterial,
            finishMaterial,
            data.EdgeBandingColor,
            rightSide,
            leftSide,
            data.RightWidth,
            data.RightDepth,
            data.ToeType,
            data.AdjustableShelfQty,
            (data.HingeLeft ? HingeSide.Left : HingeSide.Right),
            (data.DoorType == "Slab" ? null : data.DoorStyle));

    }

    public DiagonalCornerCabinet MapToDiagonalCabinet(DiagonalCornerCabinetModel model) {

        CabinetMaterialCore boxCore = GetMaterialCore(model.Cabinet.BoxMaterial.Type);

        MDFDoorOptions? mdfOptions = null;
        if (model.Cabinet.Fronts.Type != "Slab") mdfOptions = new(model.Cabinet.Fronts.Style, model.Cabinet.Fronts.Color);

        var data = new DiagonalCornerCabinetData() {
            Qty = model.Cabinet.Qty,
            UnitPrice = model.Cabinet.UnitPrice,
            Room = model.Cabinet.Room,
            Assembled = (model.Cabinet.Assembled == "Yes"),
            Height = Dimension.FromMillimeters(model.Cabinet.Height),
            Width = Dimension.FromMillimeters(model.Cabinet.Width),
            Depth = Dimension.FromMillimeters(model.Cabinet.Depth),
            BoxMaterialFinish = model.Cabinet.BoxMaterial.Finish,
            BoxMaterialCore = boxCore,
            FinishMaterialFinish = model.Cabinet.FinishMaterial.Finish,
            FinishMaterialCore = GetFinishedSideMaterialCore(model.Cabinet.FinishMaterial.Type, boxCore),
            EdgeBandingColor = (model.Cabinet.EdgeBandColor == "Match Finish" ? model.Cabinet.FinishMaterial.Finish : model.Cabinet.EdgeBandColor),
            LeftSideType = GetCabinetSideType(model.Cabinet.LeftSide),
            RightSideType = GetCabinetSideType(model.Cabinet.RightSide),
            DoorType = model.Cabinet.Fronts.Type,
            DoorStyle = mdfOptions,
            HingeLeft = (model.HingeSide == "Left"),
            ToeType = GetToeType(model.ToeType),
            AdjustableShelfQty = model.AdjShelfQty,
            DoorQty = model.DoorQty,
            RightDepth = Dimension.FromMillimeters(model.RightDepth),
            RightWidth = Dimension.FromMillimeters(model.RightWidth)
        };

        CabinetMaterial boxMaterial = new(data.BoxMaterialFinish, data.BoxMaterialCore);
        CabinetMaterial finishMaterial = new(data.FinishMaterialFinish, data.FinishMaterialCore);
        CabinetSide leftSide = new(data.LeftSideType, data.DoorStyle);
        CabinetSide rightSide = new(data.RightSideType, data.DoorStyle);

        return DiagonalCornerCabinet.Create(data.Qty,
            data.UnitPrice,
            data.Room,
            data.Assembled,
            data.Height,
            data.Width,
            data.Depth,
            boxMaterial,
            finishMaterial,
            data.EdgeBandingColor,
            rightSide,
            leftSide,
            data.RightWidth,
            data.RightDepth,
            data.ToeType,
            data.AdjustableShelfQty,
            (data.HingeLeft ? HingeSide.Left : HingeSide.Right),
            data.DoorQty,
            (data.DoorType == "Slab" ? null : data.DoorStyle));

    }

    public SinkCabinet MapToSinkCabinet(SinkCabinetModel model) {

        CabinetMaterialCore boxCore = GetMaterialCore(model.Cabinet.BoxMaterial.Type);

        MDFDoorOptions? mdfOptions = null;
        if (model.Cabinet.Fronts.Type != "Slab") mdfOptions = new(model.Cabinet.Fronts.Style, model.Cabinet.Fronts.Color);

        var data = new SinkCabinetData() {
            Qty = model.Cabinet.Qty,
            UnitPrice = model.Cabinet.UnitPrice,
            Room = model.Cabinet.Room,
            Assembled = (model.Cabinet.Assembled == "Yes"),
            Height = Dimension.FromMillimeters(model.Cabinet.Height),
            Width = Dimension.FromMillimeters(model.Cabinet.Width),
            Depth = Dimension.FromMillimeters(model.Cabinet.Depth),
            BoxMaterialFinish = model.Cabinet.BoxMaterial.Finish,
            BoxMaterialCore = boxCore,
            FinishMaterialFinish = model.Cabinet.FinishMaterial.Finish,
            FinishMaterialCore = GetFinishedSideMaterialCore(model.Cabinet.FinishMaterial.Type, boxCore),
            EdgeBandingColor = (model.Cabinet.EdgeBandColor == "Match Finish" ? model.Cabinet.FinishMaterial.Finish : model.Cabinet.EdgeBandColor),
            LeftSideType = GetCabinetSideType(model.Cabinet.LeftSide),
            RightSideType = GetCabinetSideType(model.Cabinet.RightSide),
            DoorType = model.Cabinet.Fronts.Type,
            DoorStyle = mdfOptions,
            HingeLeft = (model.HingeSide == "Left"),
            ToeType = GetToeType(model.ToeType),
            AdjustableShelfQty = model.AdjShelfQty,
            DoorQty = model.DoorQty,
            FalseDrawerQty = model.DrawerQty,
            DrawerFaceHeight = Dimension.FromMillimeters(model.DrawerFaceHeight),
            DrawerBoxMaterial = GetDrawerMaterial(model.DrawerMaterial),
            DrawerBoxSlideType = GetDrawerSlideType(model.DrawerSlide)
        };

        CabinetMaterial boxMaterial = new(data.BoxMaterialFinish, data.BoxMaterialCore);
        CabinetMaterial finishMaterial = new(data.FinishMaterialFinish, data.FinishMaterialCore);
        CabinetSide leftSide = new(data.LeftSideType, data.DoorStyle);
        CabinetSide rightSide = new(data.RightSideType, data.DoorStyle);

        var rollOutOptions = new RollOutOptions(Array.Empty<Dimension>(), true, RollOutBlockPosition.None, data.DrawerBoxSlideType, data.DrawerBoxMaterial);

        return SinkCabinet.Create(data.Qty,
            data.UnitPrice,
            data.Room,
            data.Assembled,
            data.Height,
            data.Width,
            data.Depth,
            boxMaterial,
            finishMaterial,
            data.EdgeBandingColor,
            rightSide,
            leftSide,
            data.ToeType,
            (data.HingeLeft ? HingeSide.Left : HingeSide.Right),
            data.DoorQty,
            data.FalseDrawerQty,
            data.DrawerFaceHeight,
            data.AdjustableShelfQty,
            rollOutOptions,
            (data.DoorType == "Slab" ? null : data.DoorStyle));

    }

    private static OrderModel? DeserializeData(string xmlString) {
        var serializer = new XmlSerializer(typeof(OrderModel));
        using var reader = new StringReader(xmlString);
        if (serializer.Deserialize(reader) is OrderModel data) {
            return data;
        }
        return null;
    }

    private static CabinetDrawerBoxMaterial GetDrawerMaterial(string name) => name switch {
        "Pre-Finished Birch" => CabinetDrawerBoxMaterial.SolidBirch,
        "Economy Birch" => CabinetDrawerBoxMaterial.FingerJointBirch,
        _ => CabinetDrawerBoxMaterial.FingerJointBirch
    };

    private static DrawerSlideType GetDrawerSlideType(string name) => name switch {
        "Under Mount" => DrawerSlideType.UnderMount,
        "Side Mount" => DrawerSlideType.SideMount,
        _ => DrawerSlideType.UnderMount
    };

    private static RollOutBlockPosition GetRollOutBlockPositions(string name) => name switch {
        "Left" => RollOutBlockPosition.Left,
        "Right" => RollOutBlockPosition.Right,
        "Both" => RollOutBlockPosition.Both,
        _ => RollOutBlockPosition.None
    };

    private static IToeType GetToeType(string name) => name switch {
        "Leg Levelers" => new LegLevelers(Dimension.FromMillimeters(102)),
        "Full Height Sides" => new FurnitureBase(Dimension.FromMillimeters(102)),
        "No Toe" => new NoToe(),
        "Notched" => new Notched(Dimension.FromMillimeters(102)),
        _ => new LegLevelers(Dimension.FromMillimeters(102))
    };

    private static CabinetSideType GetCabinetSideType(string name) => name switch {
        "Unfinished" => CabinetSideType.Unfinished,
        "Finished" => CabinetSideType.Finished,
        "Integrated" => CabinetSideType.IntegratedPanel,
        "Applied" => CabinetSideType.AppliedPanel,
        _ => CabinetSideType.Unfinished
    };

    private static CabinetMaterialCore GetMaterialCore(string name) => name switch {
        "pb" => CabinetMaterialCore.Flake,
        "ply" => CabinetMaterialCore.Plywood,
        _ => CabinetMaterialCore.Flake
    };

    private static ShelfDepth GetShelfDepth(string name) => name switch {
        "Full" => ShelfDepth.Full,
        "Half" => ShelfDepth.Half,
        "3/4" => ShelfDepth.ThreeQuarters,
        _ => ShelfDepth.Default
    };

    private static Dimension[] GetRollOutPositions(string pos1, string pos2, string pos3, string pos4, string pos5) {

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

    private static CabinetMaterialCore GetFinishedSideMaterialCore(string name, CabinetMaterialCore boxMaterial) {

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
