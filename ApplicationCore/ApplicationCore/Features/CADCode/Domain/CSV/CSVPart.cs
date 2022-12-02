namespace ApplicationCore.Features.CADCode.Domain.CSV;

internal class CSVPart {

    public CSVBorder Border { get; set; } = new();
    public List<CSVToken> Tokens { get; set; } = new();

}