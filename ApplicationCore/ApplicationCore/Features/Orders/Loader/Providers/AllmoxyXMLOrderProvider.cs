using ApplicationCore.Features.Companies.Commands;
using ApplicationCore.Features.Companies.Domain;
using ApplicationCore.Features.Companies.Domain.ValueObjects;
using ApplicationCore.Features.Companies.Queries;
using ApplicationCore.Features.Orders.Domain.ValueObjects;
using ApplicationCore.Features.Orders.Loader.Providers.AllmoxyXMLModels;
using ApplicationCore.Features.Orders.Loader.Providers.DTO;
using ApplicationCore.Infrastructure;
using ApplicationCore.Shared;
using System.Xml.Serialization;
using DrawerBoxModel = ApplicationCore.Features.Orders.Loader.Providers.AllmoxyXMLModels.DrawerBoxModel;

namespace ApplicationCore.Features.Orders.Loader.Providers;

internal class AllmoxyXMLOrderProvider : OrderProvider {

    private readonly IFileReader _fileReader;
    private readonly IBus _bus;
    private readonly AllmoxyConfiguration _configuration;

    private string? _readSource = null;
    private OrderModel? _sourceData = null;

    public AllmoxyXMLOrderProvider(IFileReader fileReader, IBus bus, AllmoxyConfiguration configuration) {
        _fileReader = fileReader;
        _bus = bus;
        _configuration = configuration;
    }

    private void ReadData(string source) {
        using var fileStream = _fileReader.OpenReadFileStream(source);
        var serializer = new XmlSerializer(typeof(OrderModel));
        if (serializer.Deserialize(fileStream) is not OrderModel data) throw new InvalidDataException($"Could not parse order from given file {source}");
        _readSource = source;
        _sourceData = data;
    }

    public override Task<ValidationResult> ValidateSource(string source) {

        try {

            // TODO: instead of actually reading the data, it can be checked for valid XML schema, then there is no need for the private fields
            ReadData(source);

            return Task.FromResult(new ValidationResult() {
                IsValid = true
            });

        } catch {

            return Task.FromResult(new ValidationResult() {
                IsValid = false,
                ErrorMessage = "File does not contain valid order data"
            });

        }
        
    }

    public override async Task<OrderData?> LoadOrderData(string source) {

        if (_readSource is null || _sourceData is null || !_readSource.Equals(source)) {
            // TODO: instead of actually reading the data, it can be checked for valid XML schema, then there is no need for the private fields
            ReadData(source);
        }

        if (_sourceData is null) throw new InvalidDataException("Invalid order data");

        OrderModel data = _sourceData;

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
            if (customer is null) return null;
        }

        if (customer is null || didError) return null;

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
            ClipsOptionId = GetOptionId(data.Clips),
            NotchOptionId = GetOptionId(data.Notch),
            InsertOptionId = GetOptionId(data.Insert),
        };

    private Guid GetOptionId(string optionname) {
        if (_configuration.OptionMap.TryGetValue(optionname, out string? optionidstr) && optionidstr is not null) {
            var optionid = Guid.Parse(optionidstr);
            return optionid;
        }
        return Guid.Parse("d3030d0a-8992-4b6b-8577-9d4ac43b7cf7");
    }

    private Guid GetMaterialId(string optionname) {
        if (_configuration.MaterialMap.TryGetValue(optionname, out string? optionidstr) && optionidstr is not null) {
            var optionid = Guid.Parse(optionidstr);
            return optionid;
        }
        return Guid.Parse("d3030d0a-8992-4b6b-8577-9d4ac43b7cf7");
    }

}
