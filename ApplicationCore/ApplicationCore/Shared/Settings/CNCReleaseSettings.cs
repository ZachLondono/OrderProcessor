namespace ApplicationCore.Shared.Settings;

public class CNCReleaseSettings {

    public string WSXMLReportsDirectory { get; set; } = string.Empty;
    public string DefaultOutputDirectory { get; set; } = string.Empty;
    public bool Print { get; set; }
    public bool SendEmail { get; set; }

}