using Domain.ValueObjects;

namespace ApplicationCore.Features.Orders.OrderRelease.Handlers.JobSummary;

internal class ZargenDrawerItem {

    public int Line { get; set; }
    public int Qty { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Dimension OpeningWidth { get; set; }
    public Dimension Height { get; set; }
    public Dimension Depth { get; set; }

}
