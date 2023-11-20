namespace ApplicationCore.Features.Orders.OrderLoading.PickOrderSource;

public class OrderProvidersConfiguration {

    public bool AllmoxyWebXML { get; set; }
    public bool AllmoxyFileXML { get; set; }
    public bool DoorOrder { get; set; }
    public bool ClosetProFileCSV { get; set; }
    public bool ClosetProWebCSV { get; set; }
    public bool DoweledDBOrderForm { get; set; }
    public bool ClosetOrderForm { get; set; }
    public bool HafeleDBOrderFor { get; set; }

}
