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
using ApplicationCore.Infrastructure.Bus;
using ApplicationCore.Features.Orders.Shared.Domain.Products;

namespace ApplicationCore.Features.Orders.OrderExport.Handlers.DoorOrderExport;

public class ExportDoorOrder {

    public record Command(Order Order, string TemplateFilePath, string OutputDirectory) : ICommand<DoorOrderExportResult>;

    public class Handler : CommandHandler<Command, DoorOrderExportResult> {

        private readonly ILogger<ExportDoorOrder> _logger;
        private readonly IFileReader _fileReader;
        private readonly ComponentBuilderFactory _factory;
        private readonly CompanyDirectory.GetCustomerByIdAsync _getCustomerByIdAsync;

        public Handler(ILogger<ExportDoorOrder> logger, IFileReader fileReader, ComponentBuilderFactory factory, CompanyDirectory.GetCustomerByIdAsync getCustomerByIdAsync) {
            _logger = logger;
            _fileReader = fileReader;
            _factory = factory;
            _getCustomerByIdAsync = getCustomerByIdAsync;
        }

        public override async Task<Response<DoorOrderExportResult>> Handle(Command command) {

            var doors = command.Order.Products
                                .Where(p => p is IDoorContainer)
                                .SelectMany(SelectDoorsFromProduct)
                                .ToList();

            if (!doors.Any()) {
                return Response<DoorOrderExportResult>.Error(new() {
                    Title = "Cannot Export Door Order",
                    Details = "The provided order does not contain any doors"
                });
            }

            if (!File.Exists(command.TemplateFilePath)) {
                return Response<DoorOrderExportResult>.Error(new() {
                    Title = "Cannot Export Door Order",
                    Details = "The provided door order template file path does not exist"
                });
            }

            if (!Directory.Exists(command.OutputDirectory)) {
                return Response<DoorOrderExportResult>.Error(new() {
                    Title = "Cannot Export Door Order",
                    Details = "The provided output directory does not exist"
                });
            }

            try {

                var result = await GenerateOrderForms(command.Order, doors, command.TemplateFilePath, command.OutputDirectory);
                return Response<DoorOrderExportResult>.Success(result);

            } catch (Exception ex) {

                _logger.LogError(ex, "Exception thrown while filling door order");
                return Response<DoorOrderExportResult>.Error(new() {
                    Title = "Cannot Export Door Order",
                    Details = $"Exception thrown while filling door order - {ex.Message}"
                });

            }

        }

        private IEnumerable<MDFDoorComponent> SelectDoorsFromProduct(IProduct p) {
            try {
            
                return ((IDoorContainer)p).GetDoors(_factory.CreateMDFDoorBuilder)
                                                .Select(d => new MDFDoorComponent() {
                                                    ProductId = p.Id,
                                                    Door = d
                                                });
            
            } catch {
                return Enumerable.Empty<MDFDoorComponent>();
            }
        }

        private async Task<DoorOrderExportResult> GenerateOrderForms(Order order, IEnumerable<MDFDoorComponent> doors, string template, string outputDirectory) {

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
            bool wasExceptionThrown = false;
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
                    wasExceptionThrown = true;

                    if (workbook is not null) {
                        workbook.Close(SaveChanges: false);
                }

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

            string? error = wasExceptionThrown ? "An error occurred while trying to write one or more door orders" : null;
            return new(filesGenerated, error);

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

}
