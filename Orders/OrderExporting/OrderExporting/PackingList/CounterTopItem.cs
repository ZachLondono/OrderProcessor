using Domain.ValueObjects;

namespace OrderExporting.PackingList;

public class CounterTopItem {

    public int Line { get; set; }

    public int Qty { get; set; }

    public string Finish { get; set; } = string.Empty;

    public Dimension Width { get; set; }

    public Dimension Length { get; set; }

}
