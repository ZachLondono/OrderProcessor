using ApplicationCore.Features.Companies.Queries;
using ApplicationCore.Features.Orders.Domain.ValueObjects;
using ApplicationCore.Features.Orders.Loader.Providers.DTO;
using ApplicationCore.Infrastructure;
using ApplicationCore.Shared;
using ClosedXML.Excel;
using ApplicationCore.Features.Companies.Commands;
using ApplicationCore.Features.Companies.Domain.ValueObjects;
using ApplicationCore.Features.Companies.Domain;
using ApplicationCore.Features.Orders.Loader.Providers.Results;

namespace ApplicationCore.Features.Orders.Loader.Providers;

internal class OTExcelProvider : OrderProvider {

    private readonly IFileReader _fileReader;
    private readonly IBus _bus;
    private readonly IUIBus _uiBus;
    private readonly OTConfiguration _configuration;

    public OTExcelProvider(IFileReader fileReader, IBus bus, IUIBus uiBus, OTConfiguration configuration) {
        _fileReader = fileReader;
        _bus = bus;
        _uiBus = uiBus;
        _configuration = configuration;
    }

    public override async Task<OrderData?> LoadOrderData(string source) {

        using var stream = _fileReader.OpenReadFileStream(source);
        using var wb = new XLWorkbook(stream);

        var sheet = wb.Worksheet("Order");

        if (sheet is null) return null;

        var customer = await GetCustomerFromWorksheet(sheet);
        
        if (customer is null) return null;

        if (!DateTime.TryParse(sheet.Cell("Date").ReadString(), out DateTime orderDate)) {
            orderDate = DateTime.Today;
        }

        string trackingNumber = sheet.Cell("OrderName").ReadString();
        string jobName = sheet.Cell("JobName").ReadString();
        string vendorName = sheet.Cell("J7").ReadString();

        string material = sheet.Cell("Material").ReadString();
        string notch = sheet.Cell("Notch").ReadString();
        string bottom = sheet.Cell("BotThickness").ReadString();
        string sides = sheet.Cell("SideOption").ReadString();
        string clips = sheet.Cell("C7").ReadString();

        string postFinishStr = sheet.Cell("C8").ReadString();
        bool postFinish = false;
        switch (postFinishStr) {
            case "Yes":
                postFinish = true;
                break;
            case "No":
                postFinish = false;
                break;
            default:
                _uiBus.Publish(new OrderLoadMessage() {
                    Severity = MessageSeverity.Warning,
                    Message = $"Unkown post finish value '{postFinishStr}'"
                });

                postFinish = false;
                break;
        }

        var options = new GlobalOptions() {
            BoxMaterialId = GetMaterialId(material, _configuration.MaterialMap),
            BottomMaterialId = GetMaterialId(bottom, _configuration.MaterialMap),
            Clips = clips,
            Sides = sides,
            Notch = notch,
            PostFinish = postFinish
        };

        string orderNotes = sheet.Cell("G11").ReadString();

        string production = sheet.Cell("Rush").ReadString();
        bool rush = false;
        switch (production) {
            case "Standard - 1 Week":
            case "2 Week Production":
                rush = false;
                break;
            case "4 Day Rush":
            case "3 Day Rush":
            case "2 Day Rush":
            case "1 Day Rush":
                rush = true;
                break;
            default:
                _uiBus.Publish(new OrderLoadMessage() {
                    Severity = MessageSeverity.Warning,
                    Message = $"Unkown production time value '{production}'"
                });
                rush = false;
                break;
        }

        var columns = new DataColumns() {
            LineNumber = sheet.Cell("A16"),
            Qty = sheet.Cell("B16"),
            Height = sheet.Cell("C16"),
            Width = sheet.Cell("D16"),
            Depth = sheet.Cell("E16"),
            PullOut = sheet.Cell("F16"),
            Logo = sheet.Cell("J16"),
            Accessory = sheet.Cell("K16"),
            UnitPrice = sheet.Cell("L16"),
            Note = sheet.Cell("Q16"),
            UBoxA = sheet.Cell("T16"),
            UBoxB = sheet.Cell("U16"),
            UBoxC = sheet.Cell("V16"),
        };

        var shipping = sheet.Cell("Freight").ReadDecimal();

        // TODO: discount percent is not read correctly because the cached value is in scientific notation
        var discountPercent = sheet.Cell("R12").ReadDecimal();
        var subtotal = sheet.Cell("D13").ReadDecimal();
        var priceAdj = discountPercent * subtotal;

        var tax = wb.Worksheet("Invoice").Cell("I7").ReadDecimal();

        var boxes = new List<DrawerBoxData>();

        int offset = 0;
        while (offset < 198) {

            var qtyCell = columns.Qty.GetOffsetCell(rowOffset: offset);
            if (qtyCell.ValueIsNullOrWhitespace()) {
                offset++;
                continue;
            }

            var wasRead = TryReadBox(columns, offset, out DrawerBoxData box, options);

            boxes.Add(box);

            offset++;

        }

        var vendorId = new Guid(_configuration.VendorIds[vendorName]);

        return new OrderData() {
            Number = trackingNumber,
            Name = jobName,
            Comment = orderNotes,
            OrderDate = orderDate,
            CustomerId = customer.Id,
            VendorId = vendorId,
            Rush = rush,

            PriceAdjustment = priceAdj,
            Shipping = shipping,
            Tax = tax,

            Boxes = boxes,
            AdditionalItems = new(),
            Info = new(),
        };

    }

    public override Task<ValidationResult> ValidateSource(string source) {

        if (!File.Exists(source)) 
            return Task.FromResult(new ValidationResult() {
                    IsValid = false,
                    ErrorMessage = $"Given file does not exist\n'{source}'"
                });

        var extension = Path.GetExtension(source);
        if (extension != ".xlsm")
            return Task.FromResult(new ValidationResult() {
                IsValid = false,
                ErrorMessage = $"Invalid file type '{extension}'"
            });

        return Task.FromResult(new ValidationResult() { IsValid = true });

    }

    private bool TryReadBox(DataColumns data, int offset, out DrawerBoxData box, GlobalOptions options) {
        
        box = new() {
            BoxMaterialOptionId = options.BoxMaterialId,
            BottomMaterialOptionId= options.BottomMaterialId,
            Notch = options.Notch,
            PostFinish = options.PostFinish,
            Clips = options.Clips,
            Assembled = true,
            FixedDividers = false,
            FaceMountingHoles = false
        };

        try {

            box.Line = data.LineNumber.GetOffsetCell(offset).ReadInt();
            box.Qty = data.Qty.GetOffsetCell(offset).ReadInt();

            double height = data.Height.GetOffsetCell(offset).ReadDouble();
            double width = data.Width.GetOffsetCell(offset).ReadDouble();
            double depth = data.Depth.GetOffsetCell(offset).ReadDouble();
            box.Height = Dimension.FromInches(height);
            box.Width = Dimension.FromInches(width);
            box.Depth = Dimension.FromInches(depth);

            string pullout = data.PullOut.GetOffsetCell(offset).ReadString();

            if (pullout.Equals("Scoop Front")) {
                box.ScoopFront = true;
            } else if (pullout.Equals(string.Empty)) {
                box.ScoopFront = false;
            } else {
                box.ScoopFront = false;
                // TODO: warn about unkown value
            }

            string logo = data.Logo.GetOffsetCell(offset).ReadString();
            if (logo.Equals("Yes")) {
                box.Logo = LogoPosition.Inside;
            } else if (logo.Equals("No") || logo.Equals(string.Empty)) {
                box.Logo = LogoPosition.None;
            } else { 
                box.Logo = LogoPosition.None;
                // TODO: warn about unkown value
            }

            string accessory = data.Accessory.GetOffsetCell(offset).ReadString();
            if (accessory.Equals("U-Box")) {
                box.UBox = true;
                double uboxA = data.UBoxA.GetOffsetCell(offset).ReadDouble();
                double uboxB = data.UBoxB.GetOffsetCell(offset).ReadDouble();
                double uboxC = data.UBoxC.GetOffsetCell(offset).ReadDouble();
                box.UBoxA = Dimension.FromInches(uboxA);
                box.UBoxB = Dimension.FromInches(uboxB);
                box.UBoxC = Dimension.FromInches(uboxC);
            } else {
                box.Accessory = accessory;
            }

            box.Note = data.Note.GetOffsetCell(offset).ReadString();
            box.UnitPrice = data.UnitPrice.GetOffsetCell(offset).ReadDecimal();

            return true;

        } catch {

            return false;

        }

    }

    private async Task<Company?> GetCustomerFromWorksheet(IXLWorksheet sheet) {

        // TODO: move reading from sheet outside of function

        string customerName = sheet.Cell("CustomerName").ReadString();
        var customerResponse = await _bus.Send(new GetCompanyByName.Query(customerName));

        Company? customer = null;
        bool hasError = false;
        customerResponse.Match(
            c => {
                customer = c;
            },
            error => {
                hasError = true;
                // TODO: log error
                _uiBus.Publish(new OrderLoadMessage() {
                    Severity = MessageSeverity.Error,
                    Message = error.Title
                });
            }
        );

        if (hasError) return null;

        if (customer is null) {

            string addrLine1 = sheet.Cell("Address1").ReadString();
            string addrLine2 = sheet.Cell("Address2").ReadString();
            string city = sheet.Cell("City").ReadString();
            string state = sheet.Cell("State").ReadString();
            string zip = sheet.Cell("Zip").ReadString();

            string contact = "";
            string phone = "";
            string invoiceEmail = "";
            string confirmationEmail = "";

            var address = new Address() {
                Line1 = addrLine1,
                Line2 = addrLine2,
                Line3 = string.Empty,
                City = city,
                State = state,
                Zip = zip,
                Country = "USA"
            };

            var createResponse = await _bus.Send(new CreateCompany.Command(customerName, address, phone, invoiceEmail, confirmationEmail, contact));

            createResponse.Match(
                c => {
                    customer = c;
                },
                error => {
                    hasError = true;
                    // TODO: log error
                    _uiBus.Publish(new OrderLoadMessage() {
                        Severity = MessageSeverity.Error,
                        Message = error.Title
                    });
                }
            );

        }

        if (hasError || customer is null) return null;

        return customer;

    }

    private static Guid GetMaterialId(string name, Dictionary<string, string> materialMap) {
        if (!materialMap.ContainsKey(name)) return Guid.Empty;
        try {
            return Guid.Parse(materialMap[name]);
        } catch {
            return Guid.Empty;
        }
    }

    struct GlobalOptions {

        public Guid BoxMaterialId { get; set; }
        public Guid BottomMaterialId { get; set; }
        public string Notch { get; set; }
        public string Sides { get; set; } // TODO: add this option to drawerboxes
        public string Clips { get; set; }
        public bool PostFinish { get; set; }

    }

    struct DataColumns {

        public IXLCell LineNumber { get; set; }
        public IXLCell Qty { get; set; }
        public IXLCell Height { get; set; }
        public IXLCell Width { get; set; }
        public IXLCell Depth { get; set; }
        public IXLCell PullOut { get; set; }
        public IXLCell Logo { get; set; }
        public IXLCell Accessory { get; set; }
        public IXLCell UnitPrice { get; set; }
        public IXLCell Note { get; set; }
        public IXLCell UBoxA { get; set; }
        public IXLCell UBoxB { get; set; }
        public IXLCell UBoxC { get; set; }

    }

}
