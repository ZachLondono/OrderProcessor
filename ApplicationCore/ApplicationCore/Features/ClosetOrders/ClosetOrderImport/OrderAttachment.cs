namespace ApplicationCore.Features.ClosetOrders.ClosetOrderImport;

public class OrderAttachment {

    public int Index { get; }
    public string FileName { get; }
    public bool CopyToOrders { get; set; }

    public OrderAttachment(int index, string fileName, bool copyToOrders) {
        Index = index;
        FileName = fileName;
        CopyToOrders = copyToOrders;
    }

}
