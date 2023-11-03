using Microsoft.Extensions.Configuration;

namespace ApplicationCore.Shared.Settings;

public class DoweledDrawerBoxCutListSettings {

    [ConfigurationKeyName("template_file_path")]
    public string TemplateFilePath { get; set; } = string.Empty;

}
