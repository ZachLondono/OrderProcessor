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
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using DrawerBoxModel = ApplicationCore.Features.Orders.Loader.Providers.AllmoxyXMLModels.DrawerBoxModel;

namespace ApplicationCore.Features.Orders.Loader.Providers;

internal class AllmoxyXMLOrderProvider : OrderProvider {

    private readonly IFileReader _fileReader;
    private readonly IBus _bus;
    private readonly AllmoxyConfiguration _configuration;

    public AllmoxyXMLOrderProvider(IFileReader fileReader, IBus bus, AllmoxyConfiguration configuration) {
        _fileReader = fileReader;
        _bus = bus;
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

    public override async Task<LoadOrderResult> LoadOrderData(string source) {

        using var fileStream = _fileReader.OpenReadFileStream(source);
        var serializer = new XmlSerializer(typeof(OrderModel));
        if (serializer.Deserialize(fileStream) is not OrderModel data) throw new InvalidDataException($"Could not parse order from given file {source}");

        bool didError = false;
        Company? customer = null;
        var response = await _bus.Send(new GetCompanyByAllmoxyId.Query(data.CustomerId));

        response.Match(
            c => {
                customer = c;
            },
            error => {
                // TODO: log error
                didError = true;
            }
        );

        if (customer is null) {
            customer = await CreateCustomer(data);
            if (customer is null) return new() {
				Data = null,
				Messages = new List<LoadMessage>() {
					new() {
						Message = "Could not find or create customer",
						Severity = MessageSeverity.Error
					}
				}
			};
		}

        if (customer is null || didError) return new() {
			Data = null,
			Messages = new List<LoadMessage>() {
					new() {
						Message = "Could not find or create customer",
						Severity = MessageSeverity.Error
					}
				}
		};

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

        if (!DateTime.TryParse(data.OrderDate, out DateTime orderDate)) {
            orderDate = DateTime.Now;
        }

		return new() {
			Data = new OrderData() {
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
			}
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
        var createResponse = await _bus.Send(new CreateCompany.Command(data.Customer, adderes, "", "", ""));

        Company? customer = null;
        createResponse.Match(
            async c => {
                customer = c;
                await _bus.Send(new CreateAllmoxyIdCompanyIdMapping.Command(data.CustomerId, customer.Id));
            },
            error => {
                // TODO: log error
                customer = null;
            }
        );

        return customer;

    }

    private DrawerBoxData MapToDrawerBox(DrawerBoxModel data, int line)
        => new() {
            Line = line,
            UnitPrice = data.Price,
            Qty = data.Qty,
            Height = Dimension.FromInches(data.Dimensions.Height),
            Width = Dimension.FromInches(data.Dimensions.Width),
            Depth = Dimension.FromInches(data.Dimensions.Depth),
            Logo = false,
            PostFinish = false,
            ScoopFront = false,
            FaceMountingHoles = false,
            UBox = false,
            FixedDividers = false,
            BoxMaterialOptionId = GetMaterialId(data.Material),
            BottomMaterialOptionId = GetMaterialId(data.Bottom),
            Clips = data.Clips,
            Notch = data.Notch,
            Accessory = data.Insert,
        };

    private Guid GetMaterialId(string optionname) {
        if (_configuration.MaterialMap.TryGetValue(optionname, out string? optionidstr) && optionidstr is not null) {
            var optionid = Guid.Parse(optionidstr);
            return optionid;
        }
        return Guid.Parse("d3030d0a-8992-4b6b-8577-9d4ac43b7cf7");
    }

}
