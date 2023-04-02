using ApplicationCore.Features.Companies.Contracts;
using ApplicationCore.Features.Orders.Shared.Domain;
using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Shared.Services;
using ClosedXML.Excel;

namespace ApplicationCore.Features.Orders.Details.OrderExport.Handlers;

internal class DovetailOrderHandler {

    private readonly IFileReader _fileReader;
    private readonly ComponentBuilderFactory _factory;
    private readonly CompanyDirectory.GetCustomerByIdAsync _getCustomerById;

    public DovetailOrderHandler(IFileReader fileReader, ComponentBuilderFactory factory, CompanyDirectory.GetCustomerByIdAsync getCustomerById) {
        _fileReader = fileReader;
        _factory = factory;
        _getCustomerById = getCustomerById;
    }

    public async Task Handle(Order order, string template, string outputDirectory) {

        if (!File.Exists(template)) {
            return;
        }

        if (!Directory.Exists(outputDirectory)) {
            return;
        }

        var groups = order.Products.OfType<IDrawerBoxContainer>().SelectMany(p => p.GetDrawerBoxes(_factory.CreateDovetailDrawerBoxBuilder)).GroupBy(b => b.DrawerBoxOptions.Assembled);

        var customer = await _getCustomerById(order.CustomerId);
        foreach (var group in groups) {

            using var stream = _fileReader.OpenReadFileStream(template);
            var workbook = new XLWorkbook(stream);

            var data = MapData(order, customer?.Name ?? "", group);
            WriteData(workbook, data);

            var filename = _fileReader.GetAvailableFileName(outputDirectory, $"{order.Number} - {order.Name} Drawerboxes", ".xlsm");
            workbook.SaveAs(filename);

        }

    }

    public static DBOrder MapData(Order order, string customerName, IGrouping<bool, DovetailDrawerBox> group) {
        return new() {
            OrderNumber = order.Number,
            OrderDate = order.OrderDate,
            OrderSource = order.Source,
            BoxCount = group.Count(),
            RushMessage = order.Rush ? "RUSH" : "",
            OrderSourceLink = "",
            SubTotal = order.SubTotal,
            Tax = order.Tax,
            ShippingCost = order.Shipping.Price,
            TotalCost = order.Total,
            Comment = order.CustomerComment,
            Assembly = group.Key ? "Assembled" : "UNASSEMBLED - FLAT PACK",
            ShippingInstructions = order.Shipping.Method,
            Customer = new() {
                Name = customerName,
                Line1 = order.Shipping.Address.Line1,
                Line2 = order.Shipping.Address.Line2,
                City = order.Shipping.Address.City,
                State = order.Shipping.Address.State,
                Zip = order.Shipping.Address.Zip,
                Phone = order.Shipping.PhoneNumber,
            },
            Vendor = new(),
            Supplier = new(),
            Items = group.Select(b => new DBOrder.LineItem() {
                Line = b.ProductNumber,
                Qty = b.Qty,
                Width = b.Width.AsMillimeters(),
                Height = b.Height.AsMillimeters(),
                Depth = b.Depth.AsMillimeters(),
                A = "",
                B = "",
                C = "",
                Material = b.DrawerBoxOptions.FrontMaterial,
                Bottom = b.DrawerBoxOptions.BottomMaterial,
                Notch = b.DrawerBoxOptions.Notches,
                Insert = b.DrawerBoxOptions.Accessory,
                Clips = b.DrawerBoxOptions.Clips,
                MountingHoles = b.DrawerBoxOptions.FaceMountingHoles ? "Yes" : "No",
                PostFinish = b.DrawerBoxOptions.PostFinish ? "Yes" : "No",
                ScoopFront = b.DrawerBoxOptions.ScoopFront ? "Yes" : "No",
                Logo = b.DrawerBoxOptions.Logo.ToString(),
                LevelName = "",
                Note = b.Note,
                Name = "",
                Description = "",
                UnitPrice = "",
            })
        };
    }

    public static void WriteData(IXLWorkbook workbook, DBOrder order) {

        var cells = new LineItemCells() {
            Line = workbook.Cell("LineCol"),
            Qty = workbook.Cell("QtyCol"),
            Width = workbook.Cell("WidthCol"),
            Height = workbook.Cell("HeightCol"),
            Depth = workbook.Cell("DepthCol"),
            A = workbook.Cell("DimACol"),
            B = workbook.Cell("DimBCol"),
            C = workbook.Cell("DimCCol"),
            Material = workbook.Cell("MaterialCol"),
            Bottom = workbook.Cell("BottomCol"),
            Notch = workbook.Cell("NotchCol"),
            Insert = workbook.Cell("InsertCol"),
            Clips = workbook.Cell("ClipCol"),
            MountingHoles = workbook.Cell("MountingHolesCol"),
            PostFinish = workbook.Cell("FinishCol"),
            ScoopFront = workbook.Cell("ScoopCol"),
            Logo = workbook.Cell("LogoCol"),
            LevelName = workbook.Cell("LevelCol"),
            Note = workbook.Cell("NoteCol"),
            Name = workbook.Cell("NameCol"),
            Description = workbook.Cell("DescriptionCol"),
            UnitPrice = workbook.Cell("UnitPriceCol")
        };

        cells.MoveToNextCellBellow();

        foreach (var line in order.Items) {

            cells.WriteLineItem(line);
            cells.MoveToNextCellBellow();

        }

        workbook.Cell("OrderNumber").Value = order.OrderNumber;
        workbook.Cell("OrderDate").Value = order.OrderDate;
        workbook.Cell("OrderSource").Value = order.OrderSource;
        workbook.Cell("BoxCount").Value = order.BoxCount;

        workbook.Cell("SubTotal").Value = order.SubTotal;
        workbook.Cell("Tax").Value = order.Tax;
        workbook.Cell("ShippingCost").Value = order.ShippingCost;
        workbook.Cell("TotalCost").Value = order.TotalCost;

        workbook.Cell("RushMessage").Value = order.RushMessage;
        workbook.Cell("OrderSourceLink").Value = order.OrderSourceLink;
        workbook.Cell("OrderComment").Value = order.Comment;
        workbook.Cell("Assembly").Value = order.Assembly;
        workbook.Cell("ShippingInstructions").Value = order.ShippingInstructions;

        WriteCompany(workbook, order.Customer, "Customer");
        WriteCompany(workbook, order.Vendor, "Vendor");
        WriteCompany(workbook, order.Supplier, "Supplier");

    }

    public static void WriteCompany(IXLWorkbook workbook, DBOrder.Company company, string prefix) {
        workbook.Cells($"{prefix}Name").Value = company.Name;
        workbook.Cells($"{prefix}Line1").Value = company.Line1;
        workbook.Cells($"{prefix}Line2").Value = company.Line2;
        workbook.Cells($"{prefix}City").Value = company.City;
        workbook.Cells($"{prefix}State").Value = company.State;
        workbook.Cells($"{prefix}Zip").Value = company.Zip;
        workbook.Cells($"{prefix}Phone").Value = company.Phone;
    }

    public class DBOrder {

        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public string OrderSource { get; set; } = string.Empty;
        public int BoxCount { get; set; }
        public string RushMessage { get; set; } = string.Empty;
        public string OrderSourceLink { get; set; } = string.Empty;
        public decimal SubTotal { get; set; }
        public decimal Tax { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal TotalCost { get; set; }
        public string Comment { get; set; } = string.Empty;
        public string Assembly { get; set; } = string.Empty;
        public string ShippingInstructions { get; set; } = string.Empty;
        public Company Customer { get; set; } = new();
        public Company Vendor { get; set; } = new();
        public Company Supplier { get; set; } = new();
        public IEnumerable<LineItem> Items { get; set; } = Enumerable.Empty<LineItem>();

        public class Company {

            public string Name { get; set; } = string.Empty;
            public string Line1 { get; set; } = string.Empty;
            public string Line2 { get; set; } = string.Empty;
            public string City { get; set; } = string.Empty;
            public string State { get; set; } = string.Empty;
            public string Zip { get; set; } = string.Empty;
            public string Phone { get; set; } = string.Empty;

        }

        public class LineItem {

            public object? Line { get; set; }
            public object? Qty { get; set; }
            public object? Width { get; set; }
            public object? Height { get; set; }
            public object? Depth { get; set; }
            public object? A { get; set; }
            public object? B { get; set; }
            public object? C { get; set; }
            public object? Material { get; set; }
            public object? Bottom { get; set; }
            public object? Notch { get; set; }
            public object? Insert { get; set; }
            public object? Clips { get; set; }
            public object? MountingHoles { get; set; }
            public object? PostFinish { get; set; }
            public object? ScoopFront { get; set; }
            public object? Logo { get; set; }
            public object? LevelName { get; set; }
            public object? Note { get; set; }
            public object? Name { get; set; }
            public object? Description { get; set; }
            public object? UnitPrice { get; set; }

        }

    }

    public class LineItemCells {

        public IXLCell? Line { get; set; }
        public IXLCell? Qty { get; set; }
        public IXLCell? Width { get; set; }
        public IXLCell? Height { get; set; }
        public IXLCell? Depth { get; set; }
        public IXLCell? A { get; set; }
        public IXLCell? B { get; set; }
        public IXLCell? C { get; set; }
        public IXLCell? Material { get; set; }
        public IXLCell? Bottom { get; set; }
        public IXLCell? Notch { get; set; }
        public IXLCell? Insert { get; set; }
        public IXLCell? Clips { get; set; }
        public IXLCell? MountingHoles { get; set; }
        public IXLCell? PostFinish { get; set; }
        public IXLCell? ScoopFront { get; set; }
        public IXLCell? Logo { get; set; }
        public IXLCell? LevelName { get; set; }
        public IXLCell? Note { get; set; }
        public IXLCell? Name { get; set; }
        public IXLCell? Description { get; set; }
        public IXLCell? UnitPrice { get; set; }

        public void MoveToNextCellBellow() {
            Line = Line?.CellBelow();
            Qty = Qty?.CellBelow();
            Width = Width?.CellBelow();
            Height = Height?.CellBelow();
            Depth = Depth?.CellBelow();
            A = A?.CellBelow();
            B = B?.CellBelow();
            C = C?.CellBelow();
            Material = Material?.CellBelow();
            Bottom = Bottom?.CellBelow();
            Notch = Notch?.CellBelow();
            Insert = Insert?.CellBelow();
            Clips = Clips?.CellBelow();
            MountingHoles = MountingHoles?.CellBelow();
            PostFinish = PostFinish?.CellBelow();
            ScoopFront = ScoopFront?.CellBelow();
            Logo = Logo?.CellBelow();
            LevelName = LevelName?.CellBelow();
            Note = Note?.CellBelow();
            Name = Name?.CellBelow();
            Description = Description?.CellBelow();
            UnitPrice = UnitPrice?.CellBelow();
        }

        public void WriteLineItem(DBOrder.LineItem lineItem) {
            if (Line is not null) Line.Value = lineItem.Line?.ToString() ?? "";
            if (Qty is not null) Qty.Value = lineItem.Qty?.ToString() ?? "";
            if (Width is not null) Width.Value = lineItem.Width?.ToString() ?? "";
            if (Height is not null) Height.Value = lineItem.Height?.ToString() ?? "";
            if (Depth is not null) Depth.Value = lineItem.Depth?.ToString() ?? "";
            if (A is not null) A.Value = lineItem.A?.ToString() ?? "";
            if (B is not null) B.Value = lineItem.B?.ToString() ?? "";
            if (C is not null) C.Value = lineItem.C?.ToString() ?? "";
            if (Material is not null) Material.Value = lineItem.Material?.ToString() ?? "";
            if (Bottom is not null) Bottom.Value = lineItem.Bottom?.ToString() ?? "";
            if (Notch is not null) Notch.Value = lineItem.Notch?.ToString() ?? "";
            if (Insert is not null) Insert.Value = lineItem.Insert?.ToString() ?? "";
            if (Clips is not null) Clips.Value = lineItem.Clips?.ToString() ?? "";
            if (MountingHoles is not null) MountingHoles.Value = lineItem.MountingHoles?.ToString() ?? "";
            if (PostFinish is not null) PostFinish.Value = lineItem.PostFinish?.ToString() ?? "";
            if (ScoopFront is not null) ScoopFront.Value = lineItem.ScoopFront?.ToString() ?? "";
            if (Logo is not null) Logo.Value = lineItem.Logo?.ToString() ?? "";
            if (LevelName is not null) LevelName.Value = lineItem.LevelName?.ToString() ?? "";
            if (Note is not null) Note.Value = lineItem.Note?.ToString() ?? "";
            if (Name is not null) Name.Value = lineItem.Name?.ToString() ?? "";
            if (Description is not null) Description.Value = lineItem.Description?.ToString() ?? "";
            if (UnitPrice is not null) UnitPrice.Value = lineItem.UnitPrice?.ToString() ?? "";
        }

    }

}
