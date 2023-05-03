using System.Text.Json.Serialization;

namespace ApplicationCore.Features.Configuration;

public class AppConfiguration {

    [JsonPropertyName("ordering_db_path")]
    public string OrderingDBPath { get; set; } = string.Empty;

    [JsonPropertyName("companies_db_path")]
    public string CompaniesDBPath { get; set; } = string.Empty;

    [JsonPropertyName("work_orders_db_path")]
    public string WorkOrdersDBPath { get; set; } = string.Empty;

    public delegate Task<AppConfiguration?> GetConfiguration();
    
}