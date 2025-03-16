namespace OrderLoading.LoadHafeleMDFDoorSpreadsheetOrderData;

public class EmailAttachment {

    public int Index { get; init; }
    public string FileName { get; init; }

    private bool _copyToIncoming = false;
    public bool CopyToIncoming {
        get => _copyToIncoming;
        set {
            _copyToIncoming = value;
            if (!_copyToIncoming) IsOrderForm = false;
        }
    }

    private bool _isOrderForm = false; 
    public bool IsOrderForm {
        get => _isOrderForm;
        set {
            _isOrderForm = value;
            if (_isOrderForm) CopyToIncoming = true;
        }
    }

    public EmailAttachment(int index, string fileName, bool copyToIncoming, bool isOrderForm) {
        Index = index;
        FileName = fileName;
        CopyToIncoming = copyToIncoming;
        IsOrderForm = isOrderForm;
    }

}
