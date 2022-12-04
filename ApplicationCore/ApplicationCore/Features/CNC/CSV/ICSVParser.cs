namespace ApplicationCore.Features.CNC.CSV;

internal interface ICSVParser
{

    public Task<CSVParseResult> ParsePartsAsync(string filepath);

}