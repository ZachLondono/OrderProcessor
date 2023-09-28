using System.Text.Json.Serialization;

namespace ApplicationCore.Shared.Settings;

public class Paths {

    [JsonPropertyName("schedule_path")]
    public string ScheduleWorkbookPath { get; set; } = string.Empty;

}