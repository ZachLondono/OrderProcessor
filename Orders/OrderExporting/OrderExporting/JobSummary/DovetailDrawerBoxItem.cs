using Domain.ValueObjects;

namespace OrderExporting.JobSummary;

public class DovetailDrawerBoxItem {

    public int Line { get; set; }
    public int Qty { get; set; }
    public string Description { get; set; } = string.Empty;
    public Dimension Height { get; set; }
    public Dimension Width { get; set; }
    public Dimension Depth { get; set; }
    public bool Logo { get; set; }
    public bool Scoop { get; set; }

}
