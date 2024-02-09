using Domain.ValueObjects;

namespace OrderExporting.DovetailDBPackingList;

public class DovetailDrawerBox {
    public int Line { get; set; }
    public int Qty { get; set; }
    public string Description { get; set; } = string.Empty;
    public Dimension Height { get; set; }
    public Dimension Width { get; set; }
    public Dimension Depth { get; set; }
}
