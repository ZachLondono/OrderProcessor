using Microsoft.Extensions.Configuration;

namespace OrderExporting.FivePieceDoorCutList;

public class FivePieceDoorCutListSettings {

    [ConfigurationKeyName("template_file_path")]
    public string TemplateFilePath { get; set; } = string.Empty;

}
