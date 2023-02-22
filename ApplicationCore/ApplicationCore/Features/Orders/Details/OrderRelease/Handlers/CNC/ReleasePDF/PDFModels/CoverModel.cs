using ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.ReleasePDF.Styling;

namespace ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.ReleasePDF.PDFModels;

public class CoverModel {

    public string Title { get; init; } = string.Empty;
    public string WorkOrderId { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, string> Info { get; init; } = new Dictionary<string, string>();
    public IReadOnlyList<Table> Tables { get; init; } = new List<Table>();

}
