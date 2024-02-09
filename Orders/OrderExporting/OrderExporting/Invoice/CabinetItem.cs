using Domain.ValueObjects;

namespace OrderExporting.Invoice;

public class CabinetItem {

    public int Line { get; set; }

    public int Qty { get; set; }

    public string Description { get; set; } = string.Empty;

    public Dimension Width { get; set; }

    public Dimension Height { get; set; }

    public Dimension Depth { get; set; }

    public decimal UnitPrice { get; set; }

}
