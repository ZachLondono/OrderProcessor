namespace ApplicationCore.Features.FivePieceOrderRelease;

public class FivePieceOrderReleaseOptions {

    public string FileName { get; set; } = string.Empty;
    public string OutputDirectory { get; set; } = string.Empty;

    public bool SendEmail { get; set; }
    public bool PreviewEmail { get; set; }
    public string EmailRecipients { get; set; } = string.Empty;

}
