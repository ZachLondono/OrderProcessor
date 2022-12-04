namespace ApplicationCore.Features.CNC.Domain.CSV;

internal class CSVPart {

    public CSVBorder Border { get; set; } = new();
    public List<CSVToken> Tokens { get; set; } = new();

}