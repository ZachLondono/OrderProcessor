namespace ApplicationCore.Features.ProcessHafeleMDFOrder;

public class ProcessOptions {

    private bool _generateInvoice = false;
    public bool GenerateInvoice {
        get => _generateInvoice;
        set {
            _generateInvoice = value;
            if (value) {
                SendInvoiceEmail = false;
            }
        }
    }
    public string InvoicePDFOutputDirectory { get; set; } = string.Empty;

    public bool SendInvoiceEmail { get; set; } = false;
    public bool PreviewInvoiceEmail { get; set; } = false;
    public List<Email> InvoiceEmailRecipients { get; set; } = [];

    public bool PostToGoogleSheets { get; set; } = false;

    public bool FillOrderSheet { get; set; } = false;
    public string OrderSheetTemplatePath { get; set; } = string.Empty;
    public string OrderSheetOutputDirectory { get; set; } = string.Empty;

    public class Email(string address) {
        public string Address { get; set; } = address;
    }

}
