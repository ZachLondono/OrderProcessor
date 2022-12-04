using ApplicationCore.Features.CNC.Domain.CSV;

namespace ApplicationCore.Features.CNC.Services;

internal interface ICSVParser {

    public Task<CSVParseResult> ParsePartsAsync(string filepath);

}