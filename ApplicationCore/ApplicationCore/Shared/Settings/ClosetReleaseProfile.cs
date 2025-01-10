namespace ApplicationCore.Shared.Settings;

public class ClosetReleaseProfile {

    public string[] AcknowledgementEmailRecipients { get; set; } = [];
    public string[] InvoiceEmailRecipients { get; set; } = [];
    public bool IncludeCover { get; set; } = false;
    public bool IncludePackingList { get; set; } = false;
    public bool IncludeSummary { get; set; } = false;

}