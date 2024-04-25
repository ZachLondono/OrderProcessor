using Domain.Orders.Enums;
using Domain.ValueObjects;

namespace OrderExporting.JobSummary;

public class CounterTopItem {
    public int Qty { get; set; }
    public string Finish { get; set; } = string.Empty;
    public Dimension Width { get; set; }
    public Dimension Length { get; set; }
    public EdgeBandingSides EdgeBanding { get; set; }
}
