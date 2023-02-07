using ApplicationCore.Features.ExcelTemplates.Domain;
using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Shared;
using ApplicationCore.Infrastructure;

namespace ApplicationCore.Features.Orders.Release.Handlers.JobSummary;

internal class GenerateCabinetJobSummary : DomainListener<TriggerOrderReleaseNotification> {

    private readonly IUIBus _uibus;
    private readonly IFileReader _fileReader;

    public GenerateCabinetJobSummary(IUIBus uibus, IFileReader fileReader) {
        _uibus = uibus;
        _fileReader = fileReader;
    }

    public override Task Handle(TriggerOrderReleaseNotification notification) {

        if (!notification.ReleaseProfile.GenerateCabinetJobSummary) {
            _uibus.Publish(new OrderReleaseInfoNotification("Not generating cabinet job summary, because option was disabled"));
            return Task.CompletedTask;
        }

        var order = notification.Order;
        string templatePath = notification.ReleaseProfile.CabinetJobSummaryTemplateFilePath;
        string outputDirectory = notification.ReleaseProfile.CabinetJobSummaryOutputDirectory;

        if (!File.Exists(templatePath)) {
            _uibus.Publish(new OrderReleaseErrorNotification($"Cabinet job summary template file cannot be found '{templatePath}'"));
            return Task.CompletedTask;
        }

        if (!Directory.Exists(outputDirectory)) {
            _uibus.Publish(new OrderReleaseErrorNotification($"Cabinet job summary output directory cannot be found '{outputDirectory}'"));
            return Task.CompletedTask;
        }

        try {

            FillTemplate(order, templatePath, outputDirectory);

        } catch (Exception ex) {

            _uibus.Publish(new OrderReleaseErrorNotification($"Exception thrown while generating job summary. {ex.Message}"));

        }

        return Task.CompletedTask;

    }

    public void FillTemplate(Order order, string templatePath, string outputDirectory) {

        IEnumerable<Model.Room> rooms = order.Products
                                                .Where(p => p is Cabinet)
                                                .Cast<Cabinet>()
                                                .GroupBy(c => new Model.Room() {
                                                    Name = c.Room,
                                                    Series = GetCabinetSeries(c),
                                                    CabFinish = GetCabientFinish(c.FinishMaterial),
                                                    DoorStyle = c.MDFDoorOptions?.StyleName ?? "",
                                                    DoorFinish = c.MDFDoorOptions?.Color ?? GetCabientFinish(c.FinishMaterial)
                                                })
                                                .Select(g => g.Key);

        var model = new Model() {
            JobName = order.Name,
            JobNumber = order.Number,
            CustomerName = order.Customer.Name,
            Rooms = rooms
        };

        using var stream = _fileReader.OpenReadFileStream(templatePath);
        var template = new ClosedXMLTemplate(stream);
        template.AddVariable(model);
        var result = template.Generate();

        if (result.HasErrors) {

            foreach (var error in result.ParsingErrors) {
                _uibus.Publish(new OrderReleaseErrorNotification($"Parsing error [{error.Range}] - {error.Message}"));
            }

        }

        string outputFile = _fileReader.GetAvailableFileName(outputDirectory, $"{order.Number} - Job Summary", "xlsx");

        template.SaveAs(outputFile);

    }

    public static string GetCabinetSeries(Cabinet cabinet) {
        
        if (cabinet.BoxMaterial.Core == CabinetMaterialCore.Plywood) return "Sterling 18_5";
        else if (cabinet.BoxMaterial.Core == CabinetMaterialCore.Flake && cabinet.FinishMaterial.Core == CabinetMaterialCore.Flake) return "Crown Paint";
        else if (cabinet.BoxMaterial.Core == CabinetMaterialCore.Flake && cabinet.FinishMaterial.Core == CabinetMaterialCore.Plywood) return "Crown Veneer";

        return "Unknown";

    }

    public static string GetCabientFinish(CabinetFinishMaterial material) {

        if (material.PaintColor is not null) {

            return "Paint " + material.PaintColor;

        }

        string suffix = material.Core switch {
            CabinetMaterialCore.Flake => "Mela",
            CabinetMaterialCore.Plywood => "Veneer",
            _ => ""
        };

        return material.Finish + " " + suffix;

    }

    public class Model {

        public required string JobName { get; set; }
        public required string JobNumber { get; set; }
        public required string CustomerName { get; set; }
        public required IEnumerable<Room> Rooms { get; set; }

        public record Room {

            public required string Name { get; set; }
            public required string Series { get; set; }
            public required string CabFinish { get; set; }
            public required string DoorStyle { get; set; }
            public required string DoorFinish { get; set; }

        }

    }

}
