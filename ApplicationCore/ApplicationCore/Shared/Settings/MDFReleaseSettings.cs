namespace ApplicationCore.Shared.Settings;

public class MDFReleaseSettings {

    public string DefaultOutputDirectory { get; set; } = string.Empty;
    public string WSXMLReportDirectory { get; set; } = string.Empty;
    public string CSVFileDirectory { get; set; } = string.Empty;

    public Dictionary<string, MDFReleaseProfile> ReleaseProfilesByVendor { get; set; } = new();

}