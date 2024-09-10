namespace ApplicationCore.Shared.Settings;

public class EmailSettings {

    public string SenderName { get; set; } = string.Empty;

    public string SenderEmail { get; set; } = string.Empty;

    public string ProtectedPassword { get; set; } = string.Empty;

    public string Host { get; set; } = string.Empty;

    public int Port { get; set; }

    public string BccRecipients { get; set; } = string.Empty;

}