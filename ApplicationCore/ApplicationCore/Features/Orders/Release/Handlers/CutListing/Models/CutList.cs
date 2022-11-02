namespace ApplicationCore.Features.Orders.Release.Handlers.CutListing.Models;

public class CutList {

    public Header Header { get; set; } = new();

    public List<Item> Items { get; set; } = new();

}
