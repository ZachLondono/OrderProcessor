namespace ApplicationCore.Shared.Settings;

public class ClosetReleaseSettings {

    public string CutListOutputDirectory { get; set; } = string.Empty;
    public string WSXMLReportDirectory { get; set; } = string.Empty;
    public string[] ReleaseEmailRecipients { get; set; } = [];
    public string[] DovetailDBReleaseEmailRecipients { get; set; } = [];
    public string[] InvoiceEmailRecipients { get; set; } = [];

    public Dictionary<string, ClosetReleaseProfile> ReleaseProfilesByCustomer { get; set; } = [];

}
