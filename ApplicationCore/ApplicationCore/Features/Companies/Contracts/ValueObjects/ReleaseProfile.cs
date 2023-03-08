namespace ApplicationCore.Features.Companies.Contracts.ValueObjects;

public record ReleaseProfile {

    public bool Invoice { get; set; }
    public bool PackingList { get; set; }
    public bool JobSummary { get; set; }
    public bool SendEmail { get; set; }
    public string EmailRecipients { get; set; } = string.Empty;
    public string OutputDirectory { get; set; } = string.Empty;

};
