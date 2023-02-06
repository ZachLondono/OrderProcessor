namespace ApplicationCore.Features.Orders.Release.Handlers.CutListing.Models;

public class PartRow {

    public string CabNumbers { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public int Qty { get; set; }
    public double Width { get; set; }
    public double Length { get; set; }
    public string Material { get; set; } = string.Empty;
    public int Line { get; set; }
    public string PartSize { get; set; } = string.Empty;

}