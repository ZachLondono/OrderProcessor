namespace ApplicationCore.Features.ClosetOrders.ClosetOrderImport;

public class OrderAttachment {

    public int Index { get; }
    public string FileName { get; }

    private bool _copyToIncoming = false;
    public bool CopyToIncoming {
        get => _copyToIncoming;
        set {
            _copyToIncoming = value;
            if (!_copyToIncoming) CopyToOrders = false;
        }
    }

    private bool _copyToOrders = false; 
    public bool CopyToOrders {
        get => _copyToOrders;
        set {
            _copyToOrders = value;
            if (_copyToOrders) CopyToIncoming = true;
        }
    }

    public OrderAttachment(int index, string fileName, bool copyToIncoming, bool copyToOrders) {
        Index = index;
        FileName = fileName;
        CopyToIncoming = copyToIncoming;
        CopyToOrders = copyToOrders;
    }

}
