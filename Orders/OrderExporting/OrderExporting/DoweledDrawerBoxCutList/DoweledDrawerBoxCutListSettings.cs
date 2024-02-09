using Microsoft.Extensions.Configuration;

namespace OrderExporting.DoweledDrawerBoxCutList;

public class DoweledDrawerBoxCutListSettings {

    [ConfigurationKeyName("template_file_path")]
    public string TemplateFilePath { get; set; } = string.Empty;

}
