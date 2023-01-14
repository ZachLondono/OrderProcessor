using ApplicationCore.Features.Orders.Shared.Domain;
using ApplicationCore.Infrastructure;
using ApplicationCore.Shared;
using Microsoft.Extensions.Logging;
using Microsoft.Office.Interop.Excel;

namespace ApplicationCore.Features.Orders.Release.Handlers;

internal class DoorOrderHandler : DomainListener<TriggerOrderReleaseNotification> {

    private readonly IUIBus _uibus;
    private readonly ILogger<DoorOrderHandler> _logger;
    private readonly IFileReader _fileReader;

    public DoorOrderHandler(IUIBus uibus, ILogger<DoorOrderHandler> logger, IFileReader fileReader) {
        _uibus = uibus;
        _logger = logger;
        _fileReader = fileReader;
    }

    public override Task Handle(TriggerOrderReleaseNotification notification) {

        if (!notification.ReleaseProfile.FillDoorOrder) {
            _uibus.Publish(new OrderReleaseInfoNotification("Not filling door order because option was disabled"));
            return Task.CompletedTask;
        }

        var doors = notification.Order
                            .Products
                            .Where(p => p is IDoorContainer)
                            .Cast<IDoorContainer>()
                            .SelectMany(c => c.GetDoors());

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

            var path = FillOrderForm(notification.Order, doors, template, outputDirectory, notification.ReleaseProfile.GenerateDoorCNCPrograms);
            _uibus.Publish(new OrderReleaseFileCreatedNotification("Door order created", path));

        } catch (Exception ex) {

            _logger.LogError("Exception thrown while filling door order {Exception}", ex);
            _uibus.Publish(new OrderReleaseErrorNotification($"Error occurred while trying to fill door order"));

        }


        return Task.CompletedTask;

    }

    private string FillOrderForm(Order order, IEnumerable<MDFDoor> doors, string template, string outputDirectory, bool runCC) {

        Application app = new() {
            DisplayAlerts = false,
            Visible = false
        };

        Workbook workbook = app.Workbooks.Open(template, ReadOnly: true);
        string fileName = $"{order.Number} - {order.Name} MDF DOORS.xlsm";
        string finalPath = Path.Combine(outputDirectory, fileName);

        try {

            Worksheet ws = workbook.Worksheets["MDF"];
            ws.Range["Company"].Value2 = "Test123";
            ws.Range["JobNumber"].Value2 = order.Number;
            ws.Range["JobName"].Value2 = order.Name;
            ws.Range["Material"].Value2 = "MDF-3/4\"";
            ws.Range["FramingBead"].Value2 = "Shaker";
            ws.Range["EdgeDetail"].Value2 = "Eased";
            ws.Range["PanelDetail"].Value2 = "Flat";

            var descRng = ws.Range["DescriptionStart"];
            var qtyRng = ws.Range["QtyStart"];
            var widthRng = ws.Range["WidthStart"];
            var heightRng = ws.Range["HeightStart"];
            var typeRng = ws.Range["DoorTypeStart"];

            int offset = 1;
            descRng.Offset[offset].Value2 = "Door";
            qtyRng.Offset[offset].Value2 = "1";
            widthRng.Offset[offset].Value2 = "12";
            heightRng.Offset[offset].Value2 = "15";
            typeRng.Offset[offset].Value2 = "Door";

            workbook.SaveAs2(finalPath);

            if (runCC) {
                app.Run($"'{fileName}'!DoorProcessing");
                app.Run($"'{fileName}'!ReleaseOrder");
            }

        } catch {

            throw;

        } finally {

            workbook.Close(SaveChanges: false);
            app.Quit();

        }

        return finalPath;

    }

}
