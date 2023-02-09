namespace ApplicationCore.Features.Orders.Release.Handlers.CutListing.Models;

public class CutList {

    public string Name { get; set; } = string.Empty;

    public Header Header { get; set; } = new();

    public List<PartRow> Parts { get; set; } = new();

}
