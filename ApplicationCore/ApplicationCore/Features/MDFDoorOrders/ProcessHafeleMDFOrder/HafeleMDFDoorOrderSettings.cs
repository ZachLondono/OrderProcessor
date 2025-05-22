namespace ApplicationCore.Features.MDFDoorOrders.ProcessHafeleMDFOrder;

public class HafeleMDFDoorOrderSettings {

    public string[] InvoiceEmailRecipients { get; set; } = [];
    public string[] InvoiceEmailCopyRecipients { get; set; } = [];

    public string OrderSheetTemplatePath { get; set; } = string.Empty;

}
