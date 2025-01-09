namespace ApplicationCore.Shared.Settings;

public class MDFReleaseProfile {

    public string[] EmailRecipients { get; set; } = [];
    public bool IncludeCover { get; set; } = false;
    public bool IncludePackingList { get; set; } = false;
    public bool IncludeInvoice { get; set; } = false;
    public bool IncludeOrderForm { get; set; } = false;
    public bool Print { get; set; } = false;

}