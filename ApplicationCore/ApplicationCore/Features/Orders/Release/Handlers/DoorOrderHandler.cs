using ApplicationCore.Features.Orders.Shared.Domain;
using ApplicationCore.Infrastructure;
using ApplicationCore.Features.Shared;
using Microsoft.Extensions.Logging;
using Microsoft.Office.Interop.Excel;
using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Entities;

namespace ApplicationCore.Features.Orders.Release.Handlers;

internal class DoorOrderHandler : DomainListener<TriggerOrderReleaseNotification> {

    private readonly IUIBus _uibus;
    private readonly ILogger<DoorOrderHandler> _logger;
    private readonly IFileReader _fileReader;
    private readonly ComponentBuilderFactory _factory;

    public DoorOrderHandler(IUIBus uibus, ILogger<DoorOrderHandler> logger, IFileReader fileReader, ComponentBuilderFactory factory) {
        _uibus = uibus;
        _logger = logger;
        _fileReader = fileReader;
        _factory = factory;
    }

    public override Task Handle(TriggerOrderReleaseNotification notification) {

        if (!notification.ReleaseProfile.FillDoorOrder) {
            _uibus.Publish(new OrderReleaseInfoNotification("Not filling door order because option was disabled"));
            return Task.CompletedTask;
        }

        var doors = notification.Order
                            .Products
                            .Where(p => p is IDoorContainer)
                            .SelectMany(p => {
                                try {
                                    
                                    return ((IDoorContainer) p).GetDoors(_factory.CreateMDFDoorBuilder)
                                                                    .Select(d => new MDFDoorComponent() {
                                                                        ProductId = p.Id,
                                                                        Door = d
                                                                    });

                                } catch (Exception ex) {
                                    _uibus.Publish(new OrderReleaseErrorNotification($"Error getting doors from product '{ex.Message}'"));
                                    return Enumerable.Empty<MDFDoorComponent>();
                                }
                            })
                            .ToList();

        if (!doors.Any()) {
            _uibus.Publish(new OrderReleaseInfoNotification("Door order not created because there are no doors in order"));
            return Task.CompletedTask;
        }

        var template = notification.ReleaseProfile.DoorOrderTemplateFilePath;
        var outputDirectory = notification.ReleaseProfile.DoorOrderOutputDirectory;

        if (!File.Exists(template)) {
            _uibus.Publish(new OrderReleaseErrorNotification($"Door order template file cannot be found '{template}'"));
            return Task.CompletedTask;
        }

        if (!Directory.Exists(outputDirectory)) {
            _uibus.Publish(new OrderReleaseErrorNotification($"Door order output directory is not valid '{outputDirectory}'"));
            return Task.CompletedTask;
        }

        try {

            GenerateOrderForms(notification.Order, doors, template, outputDirectory, notification.ReleaseProfile.GenerateDoorCNCPrograms);

        } catch (Exception ex) {

            _logger.LogError("Exception thrown while filling door order {Exception}", ex);
            _uibus.Publish(new OrderReleaseErrorNotification($"Error occurred while trying to fill door order"));

        } finally {

            // Clean up COM objects, calling these twice ensures it is fully cleaned up.
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();

        }


        return Task.CompletedTask;

    }

    private void GenerateOrderForms(Order order, IEnumerable<MDFDoorComponent> doors, string template, string outputDirectory, bool generateTokens) {

        var groups = doors.GroupBy(d => new DoorStyleGroupKey() {
            Material = d.Door.Material,
            FramingBead = d.Door.FramingBead,
            EdgeDetail = d.Door.EdgeDetail,
            PanelDrop = d.Door.PanelDrop.AsMillimeters(),
            PanelDetail = "Flat",
            FinishType = d.Door.PaintColor is null ? "None" : "Std. Color",
            FinishColor = d.Door.PaintColor ?? ""
        });

        Application app = new() {
            DisplayAlerts = false,
            Visible = false
        };

        int index = 0;
        foreach (var group in groups) {

            Workbook? workbook = null;

            try {

                workbook = app.Workbooks.Open(template, ReadOnly: true);

                string orderNumber = $"{order.Number}{(groups.Count() == 1 ? "" : $"-{++index}")}";

                FillOrderSheet(order, group, workbook, orderNumber);

                string fileName = _fileReader.GetAvailableFileName(outputDirectory, $"{orderNumber} {order.Name} MDF DOORS", ".xlsm");
                string finalPath = Path.Combine(outputDirectory, fileName);

                workbook.SaveAs2(finalPath);
                _uibus.Publish(new OrderReleaseFileCreatedNotification("Door order created", finalPath));

                if (generateTokens) {
                    
                    _uibus.Publish(new OrderReleaseInfoNotification("Generating mdf door CADCode tokens"));
                    
                    app.Run($"'{finalPath}'!DoorProcessing");
                    //app.Run($"'{fileName}'!ReleaseOrder");
                    workbook.Save();

                    _uibus.Publish(new OrderReleaseSuccessNotification($"CADCode tokens generated for {orderNumber}"));

                }

            } catch (Exception ex) {

                _uibus.Publish(new OrderReleaseErrorNotification($"Error generating MDF door order '{ex.Message}'"));

            } finally {
                
                workbook?.Close(SaveChanges: false);

            }

        }

        app.Quit();

    }

    private static void FillOrderSheet(Order order, IGrouping<DoorStyleGroupKey, MDFDoorComponent> doors, Workbook workbook, string orderNumber) {

        Worksheet ws = workbook.Worksheets["MDF"];
        ws.Range["OrderDate"].Value2 = order.OrderDate;
        ws.Range["Company"].Value2 = order.Customer.Name;
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

        int offset = 1;

        var data = doors.Select(d => new dynamic[] {
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

        var rows = CreateRectangularArray(data);
        var outputRng = ws.Range[$"A16:G{15 + rows.GetLength(0)}"];
        outputRng.Value2 = rows;

        var ids = doors.Select(d => new dynamic[] { d.ProductId.ToString() } ).ToArray();
        rows = CreateRectangularArray(ids);
        var idsRng = ws.Range[$"BJ16:BJ{15 + rows.GetLength(0)}"];
        idsRng.Value2 = rows;

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
