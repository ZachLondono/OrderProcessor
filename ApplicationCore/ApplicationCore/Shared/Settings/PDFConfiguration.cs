using System.Text.Json.Serialization;
using ApplicationCore.Shared.CNC.ReleasePDF.Styling;

namespace ApplicationCore.Shared.Settings;

public class PDFConfiguration {

    [JsonPropertyName("infoLabelColumnWidth")]
    public float InfoLabelColumnWidth { get; set; }

    [JsonPropertyName("headerStyle")]
    public SectionFontStyle HeaderStyle { get; set; } = new();

    [JsonPropertyName("titleStyle")]
    public SectionFontStyle TitleStyle { get; set; } = new();

    [JsonPropertyName("tableHeaderStyle")]
    public SectionFontStyle TableHeaderStyle { get; set; } = new();

    [JsonPropertyName("tableCellStyle")]
    public SectionFontStyle TableCellStyle { get; set; } = new();

}
