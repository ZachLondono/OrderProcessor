namespace ApplicationCore.Features.Orders.Release.Handlers.Invoice.Models;

internal class Invoice {

    public DateTime Date { get; set; }

    public string OrderName { get; set; } = string.Empty;

    public string OrderNumber { get; set; } = string.Empty;

    public decimal SubTotal { get; set; }

    public decimal Discount { get; set; }

    public decimal NetAmount { get; set; }

    public decimal SalesTax { get; set; }

    public decimal Shipping { get; set; }

    public decimal Total { get; set; }

    public Company Customer { get; set; } = new();

    public Company Vendor { get; set; } = new();

    public List<DrawerBoxItem> DrawerBoxes { get; set; } = new();

    public List<DoorItem> Doors { get; set; } = new();

    public List<ClosetPartItem> ClosetParts { get; set; } = new();

    public List<CabinetItem> Cabinets { get; set; } = new();

}
