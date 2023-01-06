using ApplicationCore.Features.Companies.Commands;
using ApplicationCore.Features.Companies.Domain;
using ApplicationCore.Features.Companies.Domain.ValueObjects;
using ApplicationCore.Features.Companies.Queries;
using ApplicationCore.Features.Orders.Domain;
using ApplicationCore.Features.Orders.Domain.ValueObjects;
using ApplicationCore.Features.Orders.Loader.Providers.AllmoxyXMLModels;
using ApplicationCore.Features.Orders.Loader.Providers.DTO;
using ApplicationCore.Features.Orders.Loader.Providers.Results;
using ApplicationCore.Infrastructure;
using ApplicationCore.Shared.Domain;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace ApplicationCore.Features.Orders.Loader.Providers;

internal class AllmoxyXMLOrderProvider : OrderProvider {

    private readonly IBus _bus;
    private readonly LoadingMessagePublisher _publisher;
    private readonly AllmoxyConfiguration _configuration;
    private readonly AllmoxyCredentials _credentials;
    private string? _data = null;

    public AllmoxyXMLOrderProvider(IBus bus, LoadingMessagePublisher publisher, AllmoxyConfiguration configuration, AllmoxyCredentials credentials) {
        _bus = bus;
        _publisher = publisher;
        _configuration = configuration;
        _credentials = credentials;
    }

    public override Task<ValidationResult> ValidateSource(string source) {

        try {

            if (_data is null) LoadData(source);
            if (_data is null) {
                return Task.FromResult(new ValidationResult() {
                    ErrorMessage = "Could not load order data from Allmoxy",
                    IsValid = false
                });
            }

            using var reader = new StringReader(_data);
            XDocument doc = XDocument.Load(reader);

			var schemas = new XmlSchemaSet();
			schemas.Add("", _configuration.Schema);

			var errors = new List<string>();
			doc.Validate(schemas, (s, e) => errors.Add(e.Message));

            foreach (var error in errors) {
				_publisher.PublishError($"[XML Schema] {error}");
			}

			return Task.FromResult(new ValidationResult() {
                IsValid = !errors.Any(),
                ErrorMessage = string.Join('\n', errors)
            });

        } catch {

            return Task.FromResult(new ValidationResult() {
                IsValid = false,
                ErrorMessage = "Could not validate order data"
            });

        }
        
    }

    public override async Task<OrderData?> LoadOrderData(string source) {

        if (_data is null) LoadData(source);
        if (_data is null) {
            _publisher.PublishError("Could not load order data from Allmoxy");
            return null;
        }
        
        var serializer = new XmlSerializer(typeof(OrderModel));
        using var reader = new StringReader(_data);
        if (serializer.Deserialize(reader) is not OrderModel data) {
            _publisher.PublishError("Could not find order information in given data");
            return null;
        }

        bool didError = false;
        Company? customer = null;
        var response = await _bus.Send(new GetCompanyByAllmoxyId.Query(data.Customer.CompanyId));

        response.Match(
            c => {
                customer = c;
            },
            error => {
                _publisher.PublishError(error.Title);
                didError = true;
            }
        );

        customer ??= await CreateCustomer(data);

        if (customer is null || didError) {
            _publisher.PublishError("Could not find/save customer information");
            return null;
        }

        var baseCabinets = data.Products
                                .BaseCabinets
                                .Select(MapToBaseCabinet)
                                .ToList();

        var wallCabinets = data.Products
                                .WallCabinets
                                .Select(MapToWallCabinet)
                                .ToList();

        var dbCabinets = data.Products
                                .DrawerBaseCabinets
                                .Select(MapToDrawerBaseCabinet)
                                .ToList();

        var boxes = new List<DrawerBoxModel>();

        var subTotal = baseCabinets.Sum(c => c.UnitPrice) + wallCabinets.Sum(c => c.UnitPrice) + dbCabinets.Sum(c => c.UnitPrice) + boxes.Sum(b => b.UnitPrice);
		if (subTotal != data.Invoice.Subtotal) {
			_publisher.PublishWarning($"Order data subtotal '${data.Invoice.Subtotal:0.00}' does not match calculated subtotal '${subTotal:0.00}'. There may be missing products.");
		}

        var tax = data.Invoice.Tax;
        var shipping = data.Invoice.Shipping;
        var priceAdjustment = 0M;

        var info = new Dictionary<string, string>() {
            { "Notes", data.Note },
            { "Shipping Attn", data.Shipping.Attn },
            { "Shipping Instructions", data.Shipping.Instructions },
            { "Allmoxy Customer Id", data.Customer.CompanyId.ToString() }
        };

        var additionalItems = new List<AdditionalItemData>();

        var metroVendorId = Guid.Parse(_configuration.VendorId);

        string orderDateStr = data.OrderDate;
        if (!DateTime.TryParse(orderDateStr, out DateTime orderDate)) {
            _publisher.PublishWarning($"Could not parse order date '{(orderDateStr == "" ? "[BLANK]" : orderDateStr)}'");
            orderDate = DateTime.Now;
        }

        return new OrderData() {
            Number = data.Number.ToString(),
            Name = data.Name,
            Comment = data.Description,
            Tax = tax,
            Shipping = shipping,
            PriceAdjustment = priceAdjustment,
            OrderDate = orderDate,
            CustomerId = customer.Id,
            VendorId = metroVendorId,
            AdditionalItems = additionalItems,
            BaseCabinets = baseCabinets,
            WallCabinets = wallCabinets,
            DrawerBaseCabinets = dbCabinets,
            DrawerBoxes = new(),
            Rush = data.Shipping.Method.Contains("Rush"),
            Info = info
        };

	}

	private void LoadData(string source) {
		var client = new AllmoxyClient(_credentials.Instance, _credentials.Username, _credentials.Password);
		_data = client.GetExport(source, 6);
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
                _publisher.PublishError(error.Title);
                customer = null;
            }
        );

        return customer;

    }

	private BaseCabinetData MapToBaseCabinet(BaseCabinetModel data) {

        CabinetMaterialCore boxCore = GetMaterialCore(data.Cabinet.BoxMaterial.Type);

        MDFDoorOptions? mdfOptions = null;
        if (data.Cabinet.Fronts.Type != "Slab") mdfOptions = new(data.Cabinet.Fronts.Style, data.Cabinet.Fronts.Color);

		return new BaseCabinetData() {
            Qty = data.Cabinet.Qty,
            UnitPrice = data.Cabinet.UnitPrice,
            Room = data.Cabinet.Room,
            Assembled = (data.Cabinet.Assembled == "Yes"),
            Height = Dimension.FromMillimeters(data.Cabinet.Height),
            Width = Dimension.FromMillimeters(data.Cabinet.Width),
            Depth = Dimension.FromMillimeters(data.Cabinet.Depth),
            BoxMaterialFinish = data.Cabinet.BoxMaterial.Finish,
            BoxMaterialCore = boxCore,
            FinishMaterialFinish = data.Cabinet.FinishMaterial.Finish,
            FinishMaterialCore = GetFinishedSideMaterialCore(data.Cabinet.FinishMaterial.Type, boxCore),
            EdgeBandingColor = (data.Cabinet.EdgeBandColor == "Match Finish" ? data.Cabinet.FinishMaterial.Finish : data.Cabinet.EdgeBandColor),
            LeftSideType = GetCabinetSideType(data.Cabinet.LeftSide),
			RightSideType = GetCabinetSideType(data.Cabinet.RightSide),
            DoorType = data.Cabinet.Fronts.Type,
            DoorStyle = mdfOptions,
            DoorQty = data.DoorQty,
			HingeLeft = (data.HingeSide == "Left"),
			ToeType = GetToeType(data.ToeType),
			DrawerQty = data.DrawerQty,
			DrawerFaceHeight = Dimension.FromMillimeters(data.DrawerFaceHeight),
            DrawerBoxMaterial = GetDrawerMaterial(data.DrawerMaterial),
            DrawerBoxSlideType = GetDrawerSlideType(data.DrawerSlide),
			VerticalDividerQty = data.VerticalDividerQty,
			AdjustableShelfQty = data.AdjShelfQty,
			RollOutBoxPositions = GetRollOutPositions(data.RollOuts.Pos1, data.RollOuts.Pos2, data.RollOuts.Pos3),
			RollOutBlocks = GetRollOutBlockPositions(data.RollOuts.Blocks),
			ScoopFrontRollOuts = true
		};

	}

    private WallCabinetData MapToWallCabinet(WallCabinetModel data) {

        CabinetMaterialCore boxCore = GetMaterialCore(data.Cabinet.BoxMaterial.Type);

        MDFDoorOptions? mdfOptions = null;
        if (data.Cabinet.Fronts.Type != "Slab") mdfOptions = new(data.Cabinet.Fronts.Style, data.Cabinet.Fronts.Color);

        return new WallCabinetData() {
            Qty = data.Cabinet.Qty,
            UnitPrice = data.Cabinet.UnitPrice,
            Room = data.Cabinet.Room,
            Assembled = (data.Cabinet.Assembled == "Yes"),
            Height = Dimension.FromMillimeters(data.Cabinet.Height),
            Width = Dimension.FromMillimeters(data.Cabinet.Width),
            Depth = Dimension.FromMillimeters(data.Cabinet.Depth),
            BoxMaterialFinish = data.Cabinet.BoxMaterial.Finish,
            BoxMaterialCore = boxCore,
            FinishMaterialFinish = data.Cabinet.FinishMaterial.Finish,
            FinishMaterialCore = GetFinishedSideMaterialCore(data.Cabinet.FinishMaterial.Type, boxCore),
            EdgeBandingColor = (data.Cabinet.EdgeBandColor == "Match Finish" ? data.Cabinet.FinishMaterial.Finish : data.Cabinet.EdgeBandColor),
            LeftSideType = GetCabinetSideType(data.Cabinet.LeftSide),
            RightSideType = GetCabinetSideType(data.Cabinet.RightSide),
            DoorType = data.Cabinet.Fronts.Type,
            DoorStyle = mdfOptions,
            DoorQty = data.DoorQty,
            HingeLeft = (data.HingeSide == "Left"),
            VerticalDividerQty = data.VerticalDividerQty,
            AdjustableShelfQty = data.AdjShelfQty
        };

    }

    private DrawerBaseCabinetData MapToDrawerBaseCabinet(DrawerBaseCabinetModel data) {

        CabinetMaterialCore boxCore = GetMaterialCore(data.Cabinet.BoxMaterial.Type);

        MDFDoorOptions? mdfOptions = null;
        if (data.Cabinet.Fronts.Type != "Slab") mdfOptions = new(data.Cabinet.Fronts.Style, data.Cabinet.Fronts.Color);

        var drawerFaces = new Dimension[data.DrawerQty];
        if (data.DrawerQty > 1) drawerFaces[0] = Dimension.FromMillimeters(data.DrawerFace1); // For 1 drawer box cabinets, the drawer box size is calculated
        if (data.DrawerQty >= 2) drawerFaces[1] = Dimension.FromMillimeters(data.DrawerFace2);
        if (data.DrawerQty >= 3) drawerFaces[2] = Dimension.FromMillimeters(data.DrawerFace3);
        if (data.DrawerQty >= 4) drawerFaces[3] = Dimension.FromMillimeters(data.DrawerFace4);
        if (data.DrawerQty >= 5) drawerFaces[4] = Dimension.FromMillimeters(data.DrawerFace5);

        return new DrawerBaseCabinetData() {
            Qty = data.Cabinet.Qty,
            UnitPrice = data.Cabinet.UnitPrice,
            Room = data.Cabinet.Room,
            Assembled = (data.Cabinet.Assembled == "Yes"),
            Height = Dimension.FromMillimeters(data.Cabinet.Height),
            Width = Dimension.FromMillimeters(data.Cabinet.Width),
            Depth = Dimension.FromMillimeters(data.Cabinet.Depth),
            BoxMaterialFinish = data.Cabinet.BoxMaterial.Finish,
            BoxMaterialCore = boxCore,
            FinishMaterialFinish = data.Cabinet.FinishMaterial.Finish,
            FinishMaterialCore = GetFinishedSideMaterialCore(data.Cabinet.FinishMaterial.Type, boxCore),
            EdgeBandingColor = (data.Cabinet.EdgeBandColor == "Match Finish" ? data.Cabinet.FinishMaterial.Finish : data.Cabinet.EdgeBandColor),
            LeftSideType = GetCabinetSideType(data.Cabinet.LeftSide),
            RightSideType = GetCabinetSideType(data.Cabinet.RightSide),
            DoorType = data.Cabinet.Fronts.Type,
            DoorStyle = mdfOptions,
            DrawerBoxMaterial = GetDrawerMaterial(data.DrawerMaterial),
            DrawerBoxSlideType = GetDrawerSlideType(data.DrawerSlide),
            DrawerFaces = drawerFaces.ToArray()
        };

    }

    private CabinetDrawerBoxMaterial GetDrawerMaterial(string name) => name switch {
        "Pre-Finished Birch" => CabinetDrawerBoxMaterial.SolidBirch,
        "Economy Birch" => CabinetDrawerBoxMaterial.FingerJointBirch,
        _ => CabinetDrawerBoxMaterial.FingerJointBirch
    };

    private DrawerSlideType GetDrawerSlideType(string name) => name switch {
        "Under Mount" => DrawerSlideType.UnderMount,
        "Side Mount" => DrawerSlideType.SideMount,
        _ => DrawerSlideType.UnderMount
    };

    private RollOutBlockPosition GetRollOutBlockPositions(string name) => name switch {
        "Left" => RollOutBlockPosition.Left,
		"Right" => RollOutBlockPosition.Right,
		"Both" => RollOutBlockPosition.Both,
		_ => RollOutBlockPosition.None
    };

    private Dimension[] GetRollOutPositions(string pos1, string pos2, string pos3) {

        int count = 0;
        if (pos3 == "Yes") count = 3;
        else if (pos2 == "Yes") count = 2;
        else if (pos1 == "Yes") count = 1;

        if (count == 0) return Array.Empty<Dimension>();

        var positions = new Dimension[count];
        if (count >= 1) positions[0] = pos1 == "Yes" ? Dimension.FromMillimeters(19) : Dimension.Zero;
        if (count >= 2) positions[1] = pos2 == "Yes" ? Dimension.FromMillimeters(300) : Dimension.Zero;
		if (count == 3) positions[2] = pos3 == "Yes" ? Dimension.FromMillimeters(497) : Dimension.Zero;

		return positions;

	}

    private IToeType GetToeType(string name) => name switch {
        "Leg Levelers" => new LegLevelers(Dimension.FromMillimeters(102)),
        "Full Height Sides" => new FurnitureBase(Dimension.FromMillimeters(102)),
        "No Toe" => new NoToe(),
        "Notched" => new Notched(Dimension.FromMillimeters(102)),
		_ => new LegLevelers(Dimension.FromMillimeters(102))
	};

    private CabinetSideType GetCabinetSideType(string name) => name switch {
        "Unfinished" => CabinetSideType.Unfinished,
        "Finished" => CabinetSideType.Finished,
        "Integrated" => CabinetSideType.IntegratedPanel,
        "Applied" => CabinetSideType.AppliedPanel,
        _ => CabinetSideType.Unfinished
    };

    private CabinetMaterialCore GetMaterialCore(string name) => name switch {
        "pb" => CabinetMaterialCore.Flake,
		"ply" => CabinetMaterialCore.Plywood,
		_ => CabinetMaterialCore.Flake
    };

    private CabinetMaterialCore GetFinishedSideMaterialCore(string name, CabinetMaterialCore boxMaterial) {

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

    /*private DrawerBoxData MapToDrawerBox(DrawerBoxModel data, int line) {

        LogoPosition logo = LogoPosition.None;
        switch (data.Logo) {
            case "No":
                logo = LogoPosition.None;
                break;
            case "Yes":
                logo = LogoPosition.Inside;
                break;
            default:
                _publisher.PublishWarning($"Unrecognized logo option '{data.Logo}'");
                break;
        }

        bool scoopFront = false;
        switch (data.Scoop) {
            case "No":
                scoopFront = false;
                break;
            case "Yes":
                scoopFront = true;
                break;
            default:
                _publisher.PublishWarning($"Unrecognized scoop option '{data.Scoop}'");
                break;
        }

        return new() {
            Line = line,
            UnitPrice = data.Price,
            Qty = data.Qty,
            Height = Dimension.FromInches(data.Dimensions.Height),
            Width = Dimension.FromInches(data.Dimensions.Width),
            Depth = Dimension.FromInches(data.Dimensions.Depth),
            Logo = logo,
            PostFinish = false,
            ScoopFront = scoopFront,
            FaceMountingHoles = false,
            UBox = false,
            FixedDividers = false,
            BoxMaterialOptionId = GetMaterialId(data.Material),
            BottomMaterialOptionId = GetMaterialId(data.Bottom),
            Clips = data.Clips,
            Notch = data.Notch,
            Accessory = data.Insert,
        };
    }*/

    /*private Guid GetMaterialId(string optionname) {
        if (_configuration.MaterialMap.TryGetValue(optionname, out string? optionidstr) && optionidstr is not null) {
            var optionid = Guid.Parse(optionidstr);
            return optionid;
        }
        _publisher.PublishWarning($"Unrecognized material name '{optionname}'");
        return Guid.Empty;
    }*/

}
