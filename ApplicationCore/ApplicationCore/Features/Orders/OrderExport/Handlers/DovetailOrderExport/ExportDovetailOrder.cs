using Domain.Companies;
using Domain.Orders;
using Domain.Orders.Builders;
using Domain.Orders.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;
using Domain.Orders.Components;
using Error = Domain.Infrastructure.Bus.Error;
using Domain.Infrastructure.Bus;
using Domain.Services;

namespace ApplicationCore.Features.Orders.OrderExport.Handlers.DovetailOrderExport;

public class ExportDovetailOrder {

    public record Command(Order Order, string TemplateFilePath, string OutputDirectory) : ICommand<DovetailOrderExportResult>;

    public class Handler : CommandHandler<Command, DovetailOrderExportResult> {

        private readonly IFileReader _fileReader;
        private readonly ComponentBuilderFactory _factory;
        private readonly CompanyDirectory.GetCustomerByIdAsync _getCustomerById;
        private readonly ILogger<ExportDovetailOrder> _logger;

        public Handler(IFileReader fileReader, ComponentBuilderFactory factory, CompanyDirectory.GetCustomerByIdAsync getCustomerById, ILogger<ExportDovetailOrder> logger) {
            _fileReader = fileReader;
            _factory = factory;
            _getCustomerById = getCustomerById;
            _logger = logger;
        }

        public override async Task<Response<DovetailOrderExportResult>> Handle(Command command) {

            if (!File.Exists(command.TemplateFilePath)) {
                return new Error() {
                    Title = "Could not generate dovetail order",
                    Details = "Dovetail drawer box order template file could not be found"
                };
            }

            if (!Directory.Exists(command.OutputDirectory)) {
                return new Error() {
                    Title = "Could not generate dovetail order",
                    Details = "Dovetail drawer box order output directory could not be found"
                };
            }

            var customer = await _getCustomerById(command.Order.CustomerId);
            string customerName = customer?.Name ?? "";

            IEnumerable<DovetailDrawerBox> allBoxes;

            try {

                allBoxes = command.Order
                                  .Products
                                  .OfType<IDovetailDrawerBoxContainer>()
                                  .SelectMany(p => p.GetDovetailDrawerBoxes(_factory.CreateDovetailDrawerBoxBuilder));

            } catch (Exception ex) {

                _logger.LogError(ex, "Exception thrown while trying to generate drawer boxes for order");
                return new Error() {
                    Title = "Could not generate dovetail order",
                    Details = "Failed to generate drawer boxes for all products in order"
                };

            }

            var groups = allBoxes.GroupBy(b => b.DrawerBoxOptions.Assembled);

            List<string> filesGenerated;
            try {

                filesGenerated = GenerateDrawerBoxOrders(command, customerName, groups);

            } catch (Exception ex) {

                _logger.LogError(ex, "Exception thrown while filling drawer box order");
                return new Error() {
                    Title = "Could not generate dovetail order",
                    Details = "Error occurred while writing dovetail drawer order forms"
                };

            }

            return new(new DovetailOrderExportResult(filesGenerated, null));

        }

        private List<string> GenerateDrawerBoxOrders(Command command, string customerName, IEnumerable<IGrouping<bool, DovetailDrawerBox>> groups) {

            List<string> filesGenerated = new();

            Microsoft.Office.Interop.Excel.Application app = new() {
                DisplayAlerts = false,
                Visible = false
            };

            var workbooks = app.Workbooks;

            try {

                foreach (var group in groups) {

                    Workbook workbook = workbooks.Open(command.TemplateFilePath, ReadOnly: true);

                    var data = MapData(command.Order, customerName, group);
                    var worksheets = workbook.Worksheets;
                    Worksheet worksheet = (Worksheet)worksheets["Order"];
                    WriteData(worksheet, data);
                    _ = Marshal.ReleaseComObject(worksheets);
                    _ = Marshal.ReleaseComObject(worksheet);

                    var filename = _fileReader.GetAvailableFileName(command.OutputDirectory, $"{command.Order.Number} - {command.Order.Name} Drawer Boxes", "xlsm");
                    string finalPath = Path.GetFullPath(filename);

                    workbook.SaveAs(finalPath);
                    workbook.Close(SaveChanges: false);
                    _ = Marshal.ReleaseComObject(workbook);

                    filesGenerated.Add(finalPath);

                }

            } catch {
                throw;
            } finally {

                workbooks.Close();
                app?.Quit();

                _ = Marshal.ReleaseComObject(workbooks);
                if (app is not null) _ = Marshal.ReleaseComObject(app);

                // Clean up COM objects, calling these twice ensures it is fully cleaned up.
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();

            }

            return filesGenerated;

        }

        public static DBOrder MapData(Order order, string customerName, IGrouping<bool, DovetailDrawerBox> group) {
            return new() {
                OrderNumber = order.Number,
                OrderName = order.Name,
                OrderDate = order.OrderDate,
                OrderSource = "Order Processor",
                BoxCount = group.Count(),
                RushMessage = order.Rush ? "RUSH" : "",
                OrderSourceLink = order.Source,
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
                Vendor = new() {
                    Name = "Metro Cabinet Parts",
                    Line1 = "15E Easy St",
                    Line2 = "",
                    City = "Bound Brook",
                    State = "NJ",
                    Zip = "08805"
                },
                Supplier = new() {
                    Name = "Metro Cabinet Parts",
                    Line1 = "15E Easy St",
                    Line2 = "",
                    City = "Bound Brook",
                    State = "NJ",
                    Zip = "08805"
                },
                Items = group.Select(b => new DBOrder.LineItem() {
                    Line = b.ProductNumber,
                    Qty = b.Qty,
                    Width = b.Width.AsMillimeters(),
                    Height = b.Height.AsMillimeters(),
                    Depth = b.Depth.AsMillimeters(),
                    A = "",
                    B = "",
                    C = "",
                    Material = b.DrawerBoxOptions.GetMaterialFriendlyName(),
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

        public static void WriteData(Worksheet ws, DBOrder order) {

            var cells = new LineItemCells() {
                Line = ws.Range["LineCol"],
                Qty = ws.Range["QtyCol"],
                Width = ws.Range["WidthCol"],
                Height = ws.Range["HeightCol"],
                Depth = ws.Range["DepthCol"],
                A = ws.Range["DimACol"],
                B = ws.Range["DimBCol"],
                C = ws.Range["DimCCol"],
                Material = ws.Range["MaterialCol"],
                Bottom = ws.Range["BottomCol"],
                Notch = ws.Range["NotchCol"],
                Insert = ws.Range["InsertCol"],
                Clips = ws.Range["ClipCol"],
                MountingHoles = ws.Range["MountingHolesCol"],
                PostFinish = ws.Range["FinishCol"],
                ScoopFront = ws.Range["ScoopCol"],
                Logo = ws.Range["LogoCol"],
                LevelName = ws.Range["LevelCol"],
                Note = ws.Range["NoteCol"],
                Name = ws.Range["NameCol"],
                Description = ws.Range["DescriptionCol"],
                UnitPrice = ws.Range["UnitPriceCol"]
            };

            cells.MoveToNextCellBellow();

            foreach (var line in order.Items) {

                cells.WriteLineItem(line);
                cells.MoveToNextCellBellow();

            }

            cells.ReleaseObjects();

            ws.Range["OrderNumber"].Value = order.OrderNumber;
            ws.Range["OrderField_Key_1"].Value = "Order Name";
            ws.Range["OrderField_Value_1"].Value = order.OrderName;
            ws.Range["OrderDate"].Value = order.OrderDate;
            ws.Range["OrderSource"].Value = order.OrderSource;
            ws.Range["BoxCount"].Value = order.BoxCount;

            ws.Range["SubTotal"].Value = order.SubTotal;
            ws.Range["Tax"].Value = order.Tax;
            ws.Range["ShippingCost"].Value = order.ShippingCost;
            ws.Range["TotalCost"].Value = order.TotalCost;

            ws.Range["RushMessage"].Value = order.RushMessage;
            ws.Range["OrderSourceLink"].Value = order.OrderSourceLink;
            ws.Range["OrderComment"].Value = order.Comment;
            ws.Range["Assembly"].Value = order.Assembly;
            ws.Range["ShippingInstructions"].Value = order.ShippingInstructions;

            WriteCompany(ws, order.Customer, "Customer");
            ws.Range["CustomerPhone"].Value = order.Customer.Phone;
            WriteCompany(ws, order.Vendor, "Vendor");
            WriteCompany(ws, order.Supplier, "Supplier");

        }

        public static void WriteCompany(Worksheet ws, DBOrder.Company company, string prefix) {
            ws.Range[$"{prefix}Name"].Value = company.Name;
            ws.Range[$"{prefix}Address1"].Value = company.Line1;
            ws.Range[$"{prefix}Address2"].Value = company.Line2;
            ws.Range[$"{prefix}City"].Value = company.City;
            ws.Range[$"{prefix}State"].Value = company.State;
            ws.Range[$"{prefix}Zip"].Value = company.Zip;
        }

        public class DBOrder {

            public string OrderNumber { get; set; } = string.Empty;
            public string OrderName { get; set; } = string.Empty;
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

            public Microsoft.Office.Interop.Excel.Range? Line { get; set; }
            public Microsoft.Office.Interop.Excel.Range? Qty { get; set; }
            public Microsoft.Office.Interop.Excel.Range? Width { get; set; }
            public Microsoft.Office.Interop.Excel.Range? Height { get; set; }
            public Microsoft.Office.Interop.Excel.Range? Depth { get; set; }
            public Microsoft.Office.Interop.Excel.Range? A { get; set; }
            public Microsoft.Office.Interop.Excel.Range? B { get; set; }
            public Microsoft.Office.Interop.Excel.Range? C { get; set; }
            public Microsoft.Office.Interop.Excel.Range? Material { get; set; }
            public Microsoft.Office.Interop.Excel.Range? Bottom { get; set; }
            public Microsoft.Office.Interop.Excel.Range? Notch { get; set; }
            public Microsoft.Office.Interop.Excel.Range? Insert { get; set; }
            public Microsoft.Office.Interop.Excel.Range? Clips { get; set; }
            public Microsoft.Office.Interop.Excel.Range? MountingHoles { get; set; }
            public Microsoft.Office.Interop.Excel.Range? PostFinish { get; set; }
            public Microsoft.Office.Interop.Excel.Range? ScoopFront { get; set; }
            public Microsoft.Office.Interop.Excel.Range? Logo { get; set; }
            public Microsoft.Office.Interop.Excel.Range? LevelName { get; set; }
            public Microsoft.Office.Interop.Excel.Range? Note { get; set; }
            public Microsoft.Office.Interop.Excel.Range? Name { get; set; }
            public Microsoft.Office.Interop.Excel.Range? Description { get; set; }
            public Microsoft.Office.Interop.Excel.Range? UnitPrice { get; set; }

            public void MoveToNextCellBellow() {
                Line = Line?.Offset[1];
                Qty = Qty?.Offset[1];
                Width = Width?.Offset[1];
                Height = Height?.Offset[1];
                Depth = Depth?.Offset[1];
                A = A?.Offset[1];
                B = B?.Offset[1];
                C = C?.Offset[1];
                Material = Material?.Offset[1];
                Bottom = Bottom?.Offset[1];
                Notch = Notch?.Offset[1];
                Insert = Insert?.Offset[1];
                Clips = Clips?.Offset[1];
                MountingHoles = MountingHoles?.Offset[1];
                PostFinish = PostFinish?.Offset[1];
                ScoopFront = ScoopFront?.Offset[1];
                Logo = Logo?.Offset[1];
                LevelName = LevelName?.Offset[1];
                Note = Note?.Offset[1];
                Name = Name?.Offset[1];
                Description = Description?.Offset[1];
                UnitPrice = UnitPrice?.Offset[1];
            }

            public void WriteLineItem(DBOrder.LineItem lineItem) {
                if (Line is not null) Line.Value = lineItem.Line ?? "";
                if (Qty is not null) Qty.Value = lineItem.Qty ?? "";
                if (Width is not null) Width.Value = lineItem.Width ?? "";
                if (Height is not null) Height.Value = lineItem.Height ?? "";
                if (Depth is not null) Depth.Value = lineItem.Depth ?? "";
                if (A is not null) A.Value = lineItem.A ?? "";
                if (B is not null) B.Value = lineItem.B ?? "";
                if (C is not null) C.Value = lineItem.C ?? "";
                if (Material is not null) Material.Value = lineItem.Material ?? "";
                if (Bottom is not null) Bottom.Value = lineItem.Bottom ?? "";
                if (Notch is not null) Notch.Value = lineItem.Notch ?? "";
                if (Insert is not null) Insert.Value = lineItem.Insert ?? "";
                if (Clips is not null) Clips.Value = lineItem.Clips ?? "";
                if (MountingHoles is not null) MountingHoles.Value = lineItem.MountingHoles ?? "";
                if (PostFinish is not null) PostFinish.Value = lineItem.PostFinish ?? "";
                if (ScoopFront is not null) ScoopFront.Value = lineItem.ScoopFront ?? "";
                if (Logo is not null) Logo.Value = lineItem.Logo ?? "";
                if (LevelName is not null) LevelName.Value = lineItem.LevelName ?? "";
                if (Note is not null) Note.Value = lineItem.Note ?? "";
                if (Name is not null) Name.Value = lineItem.Name ?? "";
                if (Description is not null) Description.Value = lineItem.Description ?? "";
                if (UnitPrice is not null) UnitPrice.Value = lineItem.UnitPrice ?? "";
            }

            public void ReleaseObjects() {
                if (Line is not null) _ = Marshal.ReleaseComObject(Line);
                if (Qty is not null) _ = Marshal.ReleaseComObject(Qty);
                if (Width is not null) _ = Marshal.ReleaseComObject(Width);
                if (Height is not null) _ = Marshal.ReleaseComObject(Height);
                if (Depth is not null) _ = Marshal.ReleaseComObject(Depth);
                if (A is not null) _ = Marshal.ReleaseComObject(A);
                if (B is not null) _ = Marshal.ReleaseComObject(B);
                if (C is not null) _ = Marshal.ReleaseComObject(C);
                if (Material is not null) _ = Marshal.ReleaseComObject(Material);
                if (Bottom is not null) _ = Marshal.ReleaseComObject(Bottom);
                if (Notch is not null) _ = Marshal.ReleaseComObject(Notch);
                if (Insert is not null) _ = Marshal.ReleaseComObject(Insert);
                if (Clips is not null) _ = Marshal.ReleaseComObject(Clips);
                if (MountingHoles is not null) _ = Marshal.ReleaseComObject(MountingHoles);
                if (PostFinish is not null) _ = Marshal.ReleaseComObject(PostFinish);
                if (ScoopFront is not null) _ = Marshal.ReleaseComObject(ScoopFront);
                if (Logo is not null) _ = Marshal.ReleaseComObject(Logo);
                if (LevelName is not null) _ = Marshal.ReleaseComObject(LevelName);
                if (Note is not null) _ = Marshal.ReleaseComObject(Note);
                if (Name is not null) _ = Marshal.ReleaseComObject(Name);
                if (Description is not null) _ = Marshal.ReleaseComObject(Description);
                if (UnitPrice is not null) _ = Marshal.ReleaseComObject(UnitPrice);
            }

        }

    }
}
