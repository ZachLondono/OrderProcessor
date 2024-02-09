using Domain.ValueObjects;

namespace OrderExporting.PackingList;

public class ClosetPartItem {

    public int Line { get; set; }

    public int Qty { get; set; }

    public string Description { get; set; } = string.Empty;

    public Dimension Length { get; set; }

    public Dimension Width { get; set; }

}


