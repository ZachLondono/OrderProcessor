namespace ApplicationCore.Features.Orders.Release.Handlers.CutListing.Models;

public class Header {

    public string CustomerName { get; set; } = string.Empty;

    public string VendorName { get; set; } = string.Empty;

    public string OrderNumber { get; set; } = string.Empty;

    public string OrderName { get; set; } = string.Empty;

    public string OrderDate { get; set; } = string.Empty;

    public int BoxCount { get; set; }

    public string Note { get; set; } = string.Empty;

    public string PostFinish { get; set; } = string.Empty;

    public string Notch { get; set; } = string.Empty;

    public string Clips { get; set; } = string.Empty;

    public string MountingHoles { get; set; } = string.Empty;

}
