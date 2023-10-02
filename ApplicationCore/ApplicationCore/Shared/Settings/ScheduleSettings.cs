using Microsoft.Extensions.Configuration;
using System.Text.Json.Serialization;

namespace ApplicationCore.Shared.Settings;

public class ScheduleSettings {

    [ConfigurationKeyName("workbook_path")]
    [JsonPropertyName("workbook_path")]
    public string ScheduleWorkbookPath { get; set; } = string.Empty;

}