using Microsoft.Extensions.Configuration;
using System.Text.Json.Serialization;

namespace OrderExporting.CNC.Settings;

public class MachineSettings {

    [ConfigurationKeyName("tool_file")]
    [JsonPropertyName("tool_file")]
    public string ToolFile { get; set; } = string.Empty;

    [ConfigurationKeyName("single_part_tool_file")]
    [JsonPropertyName("single_part_tool_file")]
    public string SinglePartToolFile { get; set; } = string.Empty;

    [ConfigurationKeyName("is_table_rotated")]
    [JsonPropertyName("is_table_rotated")]
    public bool IsTableRotated { get; set; } = false;

    [ConfigurationKeyName("nest_output_directory")]
    [JsonPropertyName("nest_output_directory")]
    public string NestOutputDirectory { get; set; } = string.Empty;

    [ConfigurationKeyName("single_program_output_directory")]
    [JsonPropertyName("single_program_output_directory")]
    public string SingleProgramOutputDirectory { get; set; } = string.Empty;

    [ConfigurationKeyName("picture_output_directory")]
    [JsonPropertyName("picture_output_directory")]
    public string PictureOutputDirectory { get; set; } = string.Empty;

    [ConfigurationKeyName("label_database_output_directory")]
    [JsonPropertyName("label_database_output_directory")]
    public string LabelDatabaseOutputDirectory { get; set; } = string.Empty;

}