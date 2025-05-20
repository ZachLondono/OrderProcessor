namespace ApplicationCore.Features.ProcessHafeleMDFOrder;

public class Settings {

    public string InvoicePDFOutputDirectory { get; set; } = string.Empty;
    public string[] InvoiceEmailRecipients { get; set; } = [];

    public string OrderSheetTemplatePath { get; set; } = string.Empty;
    public string OrderSheetOutputDirectory { get; set; } = string.Empty;

}
