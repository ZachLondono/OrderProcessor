using Domain.Orders.Components;
using Domain.Orders.Entities;
using Domain.Orders.Enums;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;
using Microsoft.Office.Interop.Excel;
using OneOf.Types;

namespace OrderExporting.DoorOrderExport;

public record DoorOrder {

    public required Guid ProcessorOrderId { get; set; }
    public required Optional<string> VendorName { get; set; }

    public required DateTime OrderDate { get; set; }
    public required Optional<DateTime> DueDate { get; set; }
    public required string Company { get; set; } = string.Empty;
    public required string TrackingNumber { get; set; } = string.Empty;
    public required string JobName { get; set; } = string.Empty;

    public required string Units { get; set; }

    public required GeneralSpecs Specs { get; set; }
    public required IEnumerable<LineItem> LineItems { get; set; }

    public static string METRIC_UNITS = "Metric (mm)";

    public void WriteToWorksheet(Worksheet worksheet) {

        VendorName.Switch(
            name => {
                worksheet.Range["Vendor"].Value2 = name;
            },
            none => { });
        worksheet.Range["Processor_Order_Id"].Value2 = ProcessorOrderId.ToString();
        worksheet.Range["OrderDate"].Value2 = OrderDate.ToString();
        DueDate.Switch(
            date => {
                worksheet.Range["DueDate"].Value2 = date.ToString();
            },
            none => { });
        worksheet.Range["Company"].Value2 = Company;
        worksheet.Range["JobNumber"].Value2 = TrackingNumber;
        worksheet.Range["JobName"].Value2 = JobName; 
        worksheet.Range["units"].Value2 = Units;

        Specs.WriteToWorksheet(worksheet);

        var writer = new LineItemWriter(worksheet);
        foreach (var lineItem in LineItems) {
            writer.WriteLine(lineItem);
        }

    }

    public static IEnumerable<DoorOrder> FromOrder(Order order, IEnumerable<MDFDoor> doors, string customerName, string vendorName) {

        var groups = GeneralSpecs.SeparateDoorsBySpecs(doors);

        for (int i = 0; i < groups.Length; i++) {

            // TODO: Optimization - find the most common frame width and set that to the default for the group. Then do not overwrite those values in the line items.

            var group = groups[i];

            yield return new() {
                OrderDate = order.OrderDate,
                DueDate = order.DueDate ?? Optional<DateTime>.None,
                Company = customerName,
                TrackingNumber = $"{order.Number}{(groups.Length == 0 ? string.Empty : $"-{i + 1}")}",
                JobName = order.Name,
                ProcessorOrderId = order.Id,
                Units = METRIC_UNITS,
                VendorName = vendorName,
                Specs = group.Key,
                LineItems = group.Select(LineItem.FromDoor)
            };

        }

    }

}
