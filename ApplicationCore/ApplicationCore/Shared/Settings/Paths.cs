using Microsoft.Extensions.Configuration;

namespace ApplicationCore.Shared.Settings;

public class Paths {

    [ConfigurationKeyName("schedule_path")]
    public string ScheduleWorkbookPath { get; set; } = string.Empty;

}