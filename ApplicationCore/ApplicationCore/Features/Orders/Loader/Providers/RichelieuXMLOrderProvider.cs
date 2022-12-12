using ApplicationCore.Features.Companies.Commands;
using ApplicationCore.Features.Companies.Domain;
using ApplicationCore.Features.Companies.Domain.ValueObjects;
using ApplicationCore.Features.Companies.Queries;
using ApplicationCore.Features.Orders.Domain.ValueObjects;
using ApplicationCore.Features.Orders.Loader.Providers.DTO;
using ApplicationCore.Features.Orders.Loader.Providers.Results;
using ApplicationCore.Features.Orders.Loader.Providers.RichelieuXMLModels;
using ApplicationCore.Infrastructure;
using ApplicationCore.Shared;
using ApplicationCore.Shared.Domain;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace ApplicationCore.Features.Orders.Loader.Providers;

internal partial class RichelieuXMLOrderProvider : OrderProvider {

    private readonly IFileReader _fileReader;
    private readonly IBus _bus;
    private readonly LoadingMessagePublisher _publisher;
    private readonly RichelieuConfiguration _configuration;

    public RichelieuXMLOrderProvider(IFileReader fileReader, IBus bus, LoadingMessagePublisher publisher, RichelieuConfiguration config) {
        _fileReader = fileReader;
        _bus = bus;
        _publisher = publisher;
        _configuration = config;
    }

    public override async Task<OrderData?> LoadOrderData(string source) {
        
        // TODO: get order data from http api
        using var fileStream = _fileReader.OpenReadFileStream(source);
        var serializer = new XmlSerializer(typeof(ResponseModel));
        if (serializer.Deserialize(fileStream) is not ResponseModel data) {
            _publisher.PublishError("Could not find order information in given data");
            return null;
        }

        var customer = await GetCustomerId(data.Order.ShipTo);

        if (customer is null) {
            _publisher.PublishError("Could not read/save customer information");
            return null;
        }

        var order = new OrderData {
            Name = data.Order.Header.ClientPO,
            Number = data.Order.Header.RichelieuPO,
            Comment = data.Order.Header.Note,
            OrderDate = DateTime.Parse(data.Order.Header.OrderDate),
            VendorId = _configuration.VendorId,
            CustomerId = customer.Id,

            Tax = 0M,
            Shipping = 0M,
            PriceAdjustment = 0M
        };

        order.Info.Add("Customer #", data.Order.ShipTo.RichelieuNumber);
        order.Info.Add("Web #", data.Order.Header.WebOrder);
        order.Info.Add("Richelieu #", data.Order.Header.RichelieuOrder);

        bool isRush = false;

        int lineNum = 0;
        foreach (var line in data.Order.Lines) {

            var options = ParseSku(line.Sku);

            isRush = isRush || options.Rush;

            foreach (var dimension in line.Dimensions) {

                Dimension widthDim = ParseComplexNumber(dimension.Width);
                Dimension depthDim = ParseComplexNumber(dimension.Depth);

                Dimension heightDim = Dimension.FromMillimeters(0);
                if (double.TryParse(dimension.Height, out double height)) {

                    if (_configuration.StandardHeightMap.TryGetValue(height.ToString(), out double actualHeight)) {
                        heightDim = Dimension.FromMillimeters(actualHeight);
                    } else {
                        heightDim = Dimension.FromMillimeters(height);
                    }

                } else {
                    _publisher.PublishWarning($"Could not read dimension value '{dimension.Width}'");
                }

                if (!decimal.TryParse(dimension.Price, out decimal unitPrice)) {
                    _publisher.PublishWarning($"Could not read unit price '{dimension.Price}'");
                }

                var box = new DrawerBoxData() {
                    Height = heightDim,
                    Width = widthDim,
                    Depth = depthDim,
                    Note = dimension.Note,
                    Qty = int.Parse(dimension.Qty),
                    Line = lineNum++,
                    UnitPrice = unitPrice,

                    PostFinish = options.PostFinish,
                    ScoopFront = options.ScoopFront,
                    FaceMountingHoles = options.FaceMountingHoles,
                    Logo = options.Logo ? LogoPosition.Inside : LogoPosition.None,

                    UBox = false,
                    FixedDividers = false,

                    BoxMaterialOptionId = options.BoxMaterialId,
                    BottomMaterialOptionId = options.BottomMaterialId,
                    Clips = options.Clips,
                    Notch = options.Notches,
                    Accessory = options.Accessory
                };

                order.Boxes.Add(box);

            }

        }

        if (isRush) order.Rush = true;

        return order;

	}

    private Dimension ParseComplexNumber(string value) {
        Dimension dimension = Dimension.FromMillimeters(0);
        if (TryParseComplexNumber(value, out double depth)) {
            dimension = Dimension.FromInches(depth);
        } else {
            _publisher.PublishWarning($"Could not read dimension value '{value}'");
        }

        return dimension;
    }

    public override Task<ValidationResult> ValidateSource(string source) {

        try {

            using var stream = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            XDocument doc = XDocument.Load(stream);

            var schemas = new XmlSchemaSet();
            schemas.Add("", _configuration.Schema);

            var errors = new List<string>();
            doc.Validate(schemas, (s, e) => errors.Add(e.Message));

            return Task.FromResult(new ValidationResult() {
                IsValid = !errors.Any(),
                ErrorMessage = string.Join('\n', errors)
            });

        } catch {

            return Task.FromResult(new ValidationResult() {
                IsValid = false,
                ErrorMessage = "An error occurred while trying to validate order. Make sure schema is valid."
            });

        }

    }

    public SkuValues ParseSku(string sku) {

        /*
         * Example: RCT08114ISHNX3R0
         * 
         * 0-2		Company Code	|	RCT	
         * 3-4		MaterialType	|	08->EconomyBirch, 09->Hybrid/Solid Birch, 13->Baltic Birch
         * 5
         * 6-7		BottomMaterial	|	12->1/2", 14->1/4", 38->3/8"
         * 8		Assembly		|	I->Included
         * 9-10		Notch			|	NN->No Notch, SH->Std Notch, WH->Wide Notch, FB->Front & Back
         * 11-12	Fasteners		|	NO->Without Fasteners, R4->4 way clips, R6->6 way clips
         * 13		Front			|	X->Regular, H->Extra 1" at top
         * 14		Pull-Out		|	R->No Pull, N->Clear Front, 1/2/3->Scoop Front
         * 15-16	Rush			|	R0->No Rush, R3->3 Day Rush
         */

        if (sku.Length != 15) {
            _publisher.PublishError($"SKU length is not 15 characters '{sku}'");
        }

        string specie = sku.Substring(3, 2);
        string botCode = sku.Substring(6, 2);
        string notchCode = sku.Substring(9, 2);
        string fastenerCode = sku.Substring(11, 2);
        string frontCode = sku.Substring(13, 1);
        string pullOutCode = sku.Substring(14, 1);
        string rushCode = sku.Substring(15, 2);

		return new SkuValues() {
            BoxMaterialId = GetMaterialId(specie),
            BottomMaterialId = GetMaterialId(botCode),
            Notches = GetOptionName(notchCode),
            Clips = GetOptionName(fastenerCode),
			Accessory = GetOptionName(frontCode),
			ScoopFront = pullOutCode.Equals("1") || pullOutCode.Equals("2") || pullOutCode.Equals("3"),
            Rush = rushCode.Equals("R3"),

            Logo = false,
            PostFinish = false,
            FaceMountingHoles = false,
        };

    }

    private string GetOptionName(string optionkey) {
        if (_configuration.OptionMap.TryGetValue(optionkey, out string? optionstr) && optionstr is not null) {
            return optionstr;
		}
        _publisher.PublishWarning($"Unrecognized option code '{optionkey}'");
        return string.Empty;
    }

    private Guid GetMaterialId(string optionname) {
        if (_configuration.MaterialMap.TryGetValue(optionname, out string? optionidstr) && optionidstr is not null) {
            var optionid = Guid.Parse(optionidstr);
            return optionid;
        }
        _publisher.PublishWarning($"Unrecognized material code '{optionname}'");
        return Guid.Empty;
    }

    private async Task<Company?> GetCustomerId(ShipToModel shipTo) {

        Company? customer = null;
        var response = await _bus.Send(new GetCompanyByRichelieuId.Query(shipTo.RichelieuNumber));

        response.Match(
            c => {
                customer = c;
            },
            error => {
                _publisher.PublishWarning(error.Title);
            }
        );

        customer ??= await CreateCustomer(shipTo);

        return customer;

    }

    private async Task<Company?> CreateCustomer(ShipToModel shipTo) {

        var adderes = new Address() {
            Line1 = shipTo.Address1,
            Line2 = shipTo.Address2,
            Line3 = string.Empty,
            City = shipTo.City,
            State = shipTo.Province,
            Zip = shipTo.PostalCode,
            Country = shipTo.Country
        };

        var createResponse = await _bus.Send(new CreateCompany.Command(shipTo.Company, adderes, shipTo.Phone, "", "", ""));

        Company? customer = null;
        createResponse.Match(
            async c => {
                customer = c;
                await _bus.Send(new CreateRichelieuIdCompanyIdMapping.Command(shipTo.RichelieuNumber, customer.Id));
            },
            error => {
                _publisher.PublishWarning(error.Title);
                customer = null;
            }
        );

        return customer;

    }

    public static bool TryParseComplexNumber(string input, out double value) {

        if (double.TryParse(input, out value)) return true;

        var trimmed = RemoveDuplicateWhiteSpace().Replace(input.Trim(), " ");
        var parts = trimmed.Split(' ');

        switch (parts.Length) {

            case 1:
                if (TryParseFraction(parts[0], out value)) return true;
                else return false;

            case 2:
                if (double.TryParse(parts[0], out double whole) && TryParseFraction(parts[1], out double fractional)) {
                    value = whole + fractional;
                    return true;
                }

                return false;

            default:
                return false;

        }

    }

    private static bool TryParseFraction(string input, out double value) {

        var parts = input.Trim().Split('/');

        if (parts.Length != 2) {
            value = default;
            return false;
        }

        if (double.TryParse(parts[0], out double numerator) && double.TryParse(parts[1], out double denominator)) {
            value = numerator / denominator;
            return true;
        }

        value = default;
        return false;

    }

    [GeneratedRegex("\\s+")]
    private static partial Regex RemoveDuplicateWhiteSpace();

    public class SkuValues {

        public bool Rush { get; set; }
        public bool Logo { get; set; }
        public bool PostFinish { get; set; }
        public bool ScoopFront { get; set; }
        public bool FaceMountingHoles { get; set; }
        public Guid BoxMaterialId { get; set;}
        public Guid BottomMaterialId { get; set;}
        public string Clips { get; set; } = string.Empty;
        public string Notches { get; set;} = string.Empty;
        public string Accessory { get; set; } = string.Empty;

    }

}