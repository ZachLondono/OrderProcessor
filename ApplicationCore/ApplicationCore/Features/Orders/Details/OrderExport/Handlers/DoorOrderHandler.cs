using ApplicationCore.Features.Orders.Shared.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Office.Interop.Excel;
using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Features.Shared.Services;
using ApplicationCore.Features.Companies.Contracts;
using System.Runtime.InteropServices;

namespace ApplicationCore.Features.Orders.Details.OrderExport.Handlers;

internal class DoorOrderHandler {

    private readonly ILogger<DoorOrderHandler> _logger;
    private readonly IFileReader _fileReader;
    private readonly ComponentBuilderFactory _factory;
    private readonly CompanyDirectory.GetCustomerByIdAsync _getCustomerByIdAsync;

    public DoorOrderHandler(ILogger<DoorOrderHandler> logger, IFileReader fileReader, ComponentBuilderFactory factory, CompanyDirectory.GetCustomerByIdAsync getCustomerByIdAsync) {
        _logger = logger;
        _fileReader = fileReader;
        _factory = factory;
        _getCustomerByIdAsync = getCustomerByIdAsync;
    }

    public async Task<IEnumerable<string>> Handle(Order order, string template, string outputDirectory) {

        var doors = order.Products
                            .Where(p => p is IDoorContainer)
                            .SelectMany(p => {
                                try {

                                    return ((IDoorContainer)p).GetDoors(_factory.CreateMDFDoorBuilder)
                                                                    .Select(d => new MDFDoorComponent() {
                                                                        ProductId = p.Id,
                                                                        Door = d
                                                                    });

                                } catch {
                                    return Enumerable.Empty<MDFDoorComponent>();
                                }
                            })
                            .ToList();

        if (!doors.Any()) {
            _logger.LogInformation("No doors in order, not filling door order");
            return Enumerable.Empty<string>();
        }

        if (!File.Exists(template)) {
            _logger.LogError("Door order template file does not exist, not filling door order");
            return Enumerable.Empty<string>();
        }

        if (!Directory.Exists(outputDirectory)) {
            _logger.LogError("Door order output directory does not exist, not filling door order");
            return Enumerable.Empty<string>();
        }

        try {

            return await GenerateOrderForms(order, doors, template, outputDirectory);

        } catch (Exception ex) {

            _logger.LogError(ex, "Exception thrown while filling door order");

        }


        return Enumerable.Empty<string>();

    }

    private async Task<IEnumerable<string>> GenerateOrderForms(Order order, IEnumerable<MDFDoorComponent> doors, string template, string outputDirectory) {

        var groups = doors.GroupBy(d => new DoorStyleGroupKey() {
            Material = d.Door.Material,
            FramingBead = d.Door.FramingBead,
            EdgeDetail = d.Door.EdgeDetail,
            PanelDrop = d.Door.PanelDrop.AsMillimeters(),
            PanelDetail = d.Door.PanelDetail,
            FinishType = d.Door.PaintColor is null ? "None" : "Std. Color",
            FinishColor = d.Door.PaintColor ?? ""
        });

        Application app = new() {
            DisplayAlerts = false,
            Visible = false
        };

        List<string> filesGenerated = new();

        int index = 0;
        var workbooks = app.Workbooks;
        foreach (var group in groups) {

            Workbook? workbook = null;

            try {

                workbook = workbooks.Open(template, ReadOnly: true);

                string orderNumber = $"{order.Number}{(groups.Count() == 1 ? "" : $"-{++index}")}";

                var customer = await _getCustomerByIdAsync(order.CustomerId);
                var customerName = customer?.Name ?? "";

                var worksheets = workbook.Worksheets;
                Worksheet worksheet = worksheets["MDF"];
                FillOrderSheet(order, customerName, group, worksheet, orderNumber);
                Marshal.ReleaseComObject(worksheet);
                Marshal.ReleaseComObject(worksheets);

                string fileName = _fileReader.GetAvailableFileName(outputDirectory, $"{orderNumber} - {order.Name} MDF DOORS", ".xlsm");
                string finalPath = Path.GetFullPath(fileName);

                workbook.SaveAs2(finalPath);
                workbook?.Close(SaveChanges: false);
                Marshal.ReleaseComObject(workbook);

                filesGenerated.Add(finalPath);

            } catch (Exception ex) {

                _logger.LogError(ex, "Exception thrown while filling door order group");

            }

        }

        workbooks.Close();
        app?.Quit();

        Marshal.ReleaseComObject(workbooks);
        Marshal.ReleaseComObject(app);

        // Clean up COM objects, calling these twice ensures it is fully cleaned up.
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        GC.WaitForPendingFinalizers();

        return filesGenerated;

    }

    private static void FillOrderSheet(Order order, string customerName, IGrouping<DoorStyleGroupKey, MDFDoorComponent> doors, Worksheet ws, string orderNumber) {

        ws.Range["OrderDate"].Value2 = order.OrderDate;
        ws.Range["Company"].Value2 = customerName;
        ws.Range["JobNumber"].Value2 = orderNumber;
        ws.Range["JobName"].Value2 = order.Name;

        ws.Range["Material"].Value2 = doors.Key.Material;
        ws.Range["FramingBead"].Value2 = doors.Key.FramingBead;
        ws.Range["EdgeDetail"].Value2 = doors.Key.EdgeDetail;
        ws.Range["PanelDetail"].Value2 = doors.Key.PanelDetail;
        ws.Range["PanelDrop"].Value2 = doors.Key.PanelDrop;
        ws.Range["FinishOption"].Value2 = doors.Key.FinishType;
        ws.Range["FinishColor"].Value2 = doors.Key.FinishColor;

        ws.Range["units"].Value2 = "Metric (mm)";

        var data = CreateMainDoorData(doors);
        WriteRectangularArray(ws, CreateRectangularArray(data), "A", 16, "G");

        var frameData = CreateFrameData(doors);
        WriteRectangularArray(ws, CreateRectangularArray(frameData), "P", 16, "S");

        var ids = CreateIdData(doors);
        WriteRectangularArray(ws, CreateRectangularArray(ids), "BJ", 16, "BJ");

    }

    private static dynamic[][] CreateIdData(IGrouping<DoorStyleGroupKey, MDFDoorComponent> doors) {
        return doors.Select(d => new dynamic[] { d.ProductId.ToString() }).ToArray();
    }

    private static dynamic[][] CreateFrameData(IGrouping<DoorStyleGroupKey, MDFDoorComponent> doors) {
        return doors.Select(d => new dynamic[] {
                                d.Door.FrameSize.LeftStile,
                                d.Door.FrameSize.RightStile,
                                d.Door.FrameSize.TopRail,
                                d.Door.FrameSize.BottomRail
                            })
                            .ToArray();
    }

    private static dynamic[][] CreateMainDoorData(IGrouping<DoorStyleGroupKey, MDFDoorComponent> doors) {
        int offset = 1;
        return doors.Select(d => new dynamic[] {
                            d.Door.ProductNumber,
                            d.Door.Type switch {
                                DoorType.Door => "Door",
                                DoorType.DrawerFront => "Drawer Front",
                                _ => "Door"
                            },
                            offset++,
                            d.Door.Qty,
                            d.Door.Width.AsMillimeters(),
                            d.Door.Height.AsMillimeters(),
                            d.Door.Note
                        })
                        .ToArray();
    }

    private static void WriteRectangularArray(Worksheet ws, dynamic[,] rows, string colStart, int rowStart, string colEnd) {
        ws.Range[$"{colStart}{rowStart}:{colEnd}{rowStart - 1 + rows.GetLength(0)}"].Value2 = rows;
    }

    private static dynamic[,] CreateRectangularArray(IList<dynamic>[] arrays) {

        int minorLength = arrays[0].Count;
        dynamic[,] ret = new dynamic[arrays.Length, minorLength];

        for (int i = 0; i < arrays.Length; i++) {
            var array = arrays[i];

            if (array.Count != minorLength) {
                throw new ArgumentException("All arrays must be the same length");
            }

            for (int j = 0; j < minorLength; j++) {
                ret[i, j] = array[j];
            }
        }

        return ret;

    }

    public record MDFDoorComponent {

        public required Guid ProductId { get; set; }
        public required MDFDoor Door { get; set; }

    }

    public record DoorStyleGroupKey {

        public required string FramingBead { get; init; }
        public required string EdgeDetail { get; init; }
        public required string PanelDetail { get; init; }
        public required string Material { get; init; }
        public required double PanelDrop { get; init; }
        public required string FinishType { get; init; }
        public required string FinishColor { get; init; }

    }

}
