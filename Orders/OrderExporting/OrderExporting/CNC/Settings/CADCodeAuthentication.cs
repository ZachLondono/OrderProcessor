using Microsoft.Extensions.Configuration;

namespace OrderExporting.CNC.Settings;

public class CADCodeAuthentication {

    [ConfigurationKeyName("username")]
    public string Username { get; set; } = string.Empty;

    [ConfigurationKeyName("password")]
    public string Password { get; set; } = string.Empty;

}