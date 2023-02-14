using System.Text.Json.Serialization;

namespace ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.ReleasePDF.Styling;

public class SectionFontStyle {

    [JsonPropertyName("family")]
    public string Family { get; set; } = "Arial";

    [JsonPropertyName("weight")]
    public int Weight { get; set; } = 100;

    [JsonPropertyName("size")]
    public int Size { get; set; } = 12;

    [JsonPropertyName("color")]
    public string Color { get; set; } = "#000000";

}