using Microsoft.Extensions.Configuration;
using System.Text.Json.Serialization;

namespace ApplicationCore.Shared.Data;

public class DataFilePaths {

    [JsonPropertyName("ordering_db_path")]
    [ConfigurationKeyName("ordering_db_path")]
    public string OrderingDBPath { get; set; } = string.Empty;

    [JsonPropertyName("companies_db_path")]
    [ConfigurationKeyName("companies_db_path")]
    public string CompaniesDBPath { get; set; } = string.Empty;

}