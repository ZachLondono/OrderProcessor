using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.OrderRelease.Handlers.JobSummary;

internal class ClosetPartItem {

    public int Line { get; set; }
    public int Qty { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Dimension Width { get; set; }
    public Dimension Length { get; set; }

}
