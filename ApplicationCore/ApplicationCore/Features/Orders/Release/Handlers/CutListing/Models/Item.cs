namespace ApplicationCore.Features.Orders.Release.Handlers.CutListing.Models;

public class Item {

    public int GroupNumber { get; set; }

    public int CabNumber { get; set; }

    public string PartName { get; set; } = string.Empty;

    public string Comment { get; set; } = string.Empty;

    public int Qty { get; set; }

    public double Width { get; set; }

    public double Length { get; set; }

    public string Material { get; set; } = string.Empty;

    public int LineNumber { get; set; }

    public string Size { get; set; } = string.Empty;

}