using ApplicationCore.Features.Companies.Commands;
using ApplicationCore.Features.Companies.Domain;
using ApplicationCore.Features.Companies.Domain.ValueObjects;
using ApplicationCore.Features.Companies.Queries;
using ApplicationCore.Features.Orders.Commands;
using ApplicationCore.Features.Orders.Domain;
using ApplicationCore.Features.Orders.Domain.ValueObjects;
using ApplicationCore.Features.Orders.Loader.Providers.AllmoxyXMLModels;
using ApplicationCore.Features.Orders.Providers.AllmoxyXMLModels;
using ApplicationCore.Features.Orders.Queries;
using ApplicationCore.Infrastructure;
using ApplicationCore.Shared;
using System.Xml.Serialization;
using DrawerBoxModel = ApplicationCore.Features.Orders.Loader.Providers.AllmoxyXMLModels.DrawerBoxModel;

namespace ApplicationCore.Features.Orders.Loader.Providers;

internal class AllmoxyXMLOrderProvider : IOrderProvider {

    private readonly IFilePicker _filePicker;
    private readonly IFileReader _fileReader;
    private readonly IBus _bus;
    private readonly AllmoxyConfiguration _configuration;

    private readonly Dictionary<Guid, DrawerBoxOption> _optionCache = new();

    public AllmoxyXMLOrderProvider(IFilePicker filePicker, IFileReader fileReader, IBus bus, AllmoxyConfiguration configuration) {
        _filePicker = filePicker;
        _fileReader = fileReader;
        _bus = bus;
        _configuration = configuration;
    }

    public async Task<Order?> LoadOrderData() {
        bool wasPicked = _filePicker.TryPickFile("Select Allmoxy Order File", _configuration.DefaultDirectory, new("XML files (*.xml)", ".xml"), out string filepath);
        if (!wasPicked) throw new InvalidDataException("No file was picked");

        using var fileStream = _fileReader.OpenReadFileStream(filepath);
        var serializer = new XmlSerializer(typeof(OrderModel));

        if (serializer.Deserialize(fileStream) is not OrderModel data) throw new InvalidDataException($"Could not parse order from given file {filepath}");

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
        var mappingTasks = data.DrawerBoxes.Select(async b => await MapToDrawerBox(b, line++)).ToList();
        var boxes = await Task.WhenAll(mappingTasks);
        var tax = data.Invoice.Tax;
        var shipping = data.Invoice.Shipping;
        var priceAdjustment = 0m;

        var info = new Dictionary<string, string>() {
            { "Description", data.Description },
            { "Shipping Attn", data.Shipping.Attn },
            { "Shipping Instructions", data.Shipping.Instructions },
            { "Allmoxy Customer Id", data.CustomerId.ToString() }
        };

        var metroVendorId = Guid.Parse(_configuration.VendorId);

        if (!DateTime.TryParse(data.OrderDate, out DateTime orderDate)) {
            orderDate = DateTime.Now;
        }

        var result = await _bus.Send(new CreateNewOrder.Command(data.Name, data.Id.ToString(), customer.Id, metroVendorId, data.Note, orderDate, tax, shipping, priceAdjustment, info, boxes, Enumerable.Empty<AdditionalItem>()));
            result = await _bus.Send(new CreateNewOrder.Command(source, data.Id.ToString(), data.Name, customer.Id, metroVendorId, data.Note, orderDate, tax, shipping, priceAdjustment, info, boxes, Enumerable.Empty<AdditionalItem>()));

        Order? order = null;

        result.Match(
            o => order = o,
            error => {
                // TODO: log error
            }
        );

        return order;

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

    private async Task<DrawerBox> MapToDrawerBox(DrawerBoxModel data, int line) {

        return DrawerBox.Create(
            line,
            data.Price,
            data.Qty,
            Dimension.FromInches(data.Dimensions.Height),
            Dimension.FromInches(data.Dimensions.Width),
            Dimension.FromInches(data.Dimensions.Depth),
            new(
                boxMaterial: await GetOption(data.Material),
                bottomMaterial: await GetOption(data.Bottom),
                clips: await GetOption(data.Clips),
                notches: await GetOption(data.Notch),
                accessory: await GetOption(data.Insert),
                logo: false,
                facemountingholes: false,
                postFinish: false,
                scoopFront: false,
                uBoxDimensions: null, //new() { A = Dimension.FromInches(5), B = Dimension.FromInches(5), C = Dimension.FromInches(5) },
                fixedDivdersCounts: null //new() { WideCount = 2, DeepCount = 2 }
                )
            );
    }

    private async Task<DrawerBoxOption> GetOption(string optionname) {

        if (_configuration.OptionMap.TryGetValue(optionname, out string? optionidstr) && optionidstr is not null) {
            var optionid = Guid.Parse(optionidstr);

            if (_optionCache.TryGetValue(optionid, out DrawerBoxOption? option)) return option;

            var response = await _bus.Send(new GetDrawerBoxOptionById.Query(optionid));

            response.Match(
                o => option = o,
                error => {
                    // TODO: log error
                }
            );

            if (option is null) return new DrawerBoxOption(Guid.Parse("d3030d0a-8992-4b6b-8577-9d4ac43b7cf7"), "UNKNOWN");

            _optionCache.Add(optionid, option);
            return option;
        }

        return new DrawerBoxOption(Guid.Parse("d3030d0a-8992-4b6b-8577-9d4ac43b7cf7"), "UNKNOWN");

    }

}
