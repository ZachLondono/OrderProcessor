namespace ApplicationCore.Features.Orders.OrderRelease.Handlers.JobSummary;

internal class CabinetPartItem {

    public int Line { get; set; }
    public int Qty { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

}
