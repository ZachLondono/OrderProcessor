using System.Text.Json.Serialization;

namespace ApplicationCore.Features.CADCode.Services.Domain.PDF;

internal class PDFConfiguration {

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
