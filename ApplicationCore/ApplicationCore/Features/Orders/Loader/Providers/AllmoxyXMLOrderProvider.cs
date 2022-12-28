using ApplicationCore.Features.Companies.Commands;
using ApplicationCore.Features.Companies.Domain;
using ApplicationCore.Features.Companies.Domain.ValueObjects;
using ApplicationCore.Features.Companies.Queries;
using ApplicationCore.Features.Orders.Domain.ValueObjects;
using ApplicationCore.Features.Orders.Loader.Providers.AllmoxyXMLModels;
using ApplicationCore.Features.Orders.Loader.Providers.DTO;
using ApplicationCore.Features.Orders.Loader.Providers.Results;
using ApplicationCore.Infrastructure;
using ApplicationCore.Shared;
using ApplicationCore.Shared.Domain;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using DrawerBoxModel = ApplicationCore.Features.Orders.Loader.Providers.AllmoxyXMLModels.DrawerBoxModel;

namespace ApplicationCore.Features.Orders.Loader.Providers;

internal class AllmoxyXMLOrderProvider : OrderProvider {

    private readonly IFileReader _fileReader;
    private readonly IBus _bus;
    private readonly LoadingMessagePublisher _publisher;
    private readonly AllmoxyConfiguration _configuration;

    public AllmoxyXMLOrderProvider(IFileReader fileReader, IBus bus, LoadingMessagePublisher publisher, AllmoxyConfiguration configuration) {
        _fileReader = fileReader;
        _bus = bus;
        _publisher = publisher;
        _configuration = configuration;
    }

    public override Task<ValidationResult> ValidateSource(string source) {

        try {

			using var stream = _fileReader.OpenReadFileStream(source);
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
                ErrorMessage = "Could not validate order data"
            });

        }
        
    }

    public override async Task<OrderData?> LoadOrderData(string source) {

        using var fileStream = _fileReader.OpenReadFileStream(source);
        var serializer = new XmlSerializer(typeof(OrderModel));
        if (serializer.Deserialize(fileStream) is not OrderModel data) {
            _publisher.PublishError("Could not find order information in given data");
            return null;
        }

        bool didError = false;
        Company? customer = null;
        var response = await _bus.Send(new GetCompanyByAllmoxyId.Query(data.CustomerId));

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

        int line = 1;
        var boxes = data.DrawerBoxes
                        .Select(b => MapToDrawerBox(b, line++))
                        .ToList();

        var tax = data.Invoice.Tax;
        var shipping = data.Invoice.Shipping;
        var priceAdjustment = 0m;

        var info = new Dictionary<string, string>() {
            { "Description", data.Description },
            { "Shipping Attn", data.Shipping.Attn },
            { "Shipping Instructions", data.Shipping.Instructions },
            { "Allmoxy Customer Id", data.CustomerId.ToString() }
        };

        var additionalItems = new List<AdditionalItemData>();

        var metroVendorId = Guid.Parse(_configuration.VendorId);

        string orderDateStr = data.OrderDate;
        if (!DateTime.TryParse(orderDateStr, out DateTime orderDate)) {
            _publisher.PublishWarning($"Could not parse order data '{orderDateStr}'");
            orderDate = DateTime.Now;
        }

        return new OrderData() {
            Number = data.Id.ToString(),
            Name = data.Name,
            Comment = data.Note,
            Tax = tax,
            Shipping = shipping,
            PriceAdjustment = priceAdjustment,
            OrderDate = orderDate,
            CustomerId = customer.Id,
            VendorId = metroVendorId,
            AdditionalItems = additionalItems,
            Boxes = boxes,
            Info = info
        };

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
        var createResponse = await _bus.Send(new CreateCompany.Command(data.Customer, adderes, "", "", "", ""));

        Company? customer = null;
        createResponse.Match(
            async c => {
                customer = c;
                await _bus.Send(new CreateAllmoxyIdCompanyIdMapping.Command(data.CustomerId, customer.Id));
            },
            error => {
                _publisher.PublishError(error.Title);
                customer = null;
            }
        );

        return customer;

    }

    private DrawerBoxData MapToDrawerBox(DrawerBoxModel data, int line) {

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
    }

    private Guid GetMaterialId(string optionname) {
        if (_configuration.MaterialMap.TryGetValue(optionname, out string? optionidstr) && optionidstr is not null) {
            var optionid = Guid.Parse(optionidstr);
            return optionid;
        }
        _publisher.PublishWarning($"Unrecognized material name '{optionname}'");
        return Guid.Empty;
    }

}
