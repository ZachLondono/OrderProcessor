namespace ApplicationCore.Features.Orders.Release.Handlers.CutListing.Models;

public class Header {

    public string CustomerName { get; set; } = string.Empty;
    public string VendorName { get; set; } = string.Empty;
    public string OrderNumber { get; set; } = string.Empty;
    public string JobName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public int BoxCount { get; set; }
    public string Notches { get; set; } = string.Empty;
    public string Clips { get; set; } = string.Empty;
    public bool MountingHoles { get; set; }
    public bool Finish { get; set; }
    public bool Assembly { get; set; }
    public string Note { get; set; } = string.Empty;

}
