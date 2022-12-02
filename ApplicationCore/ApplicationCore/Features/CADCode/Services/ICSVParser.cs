using ApplicationCore.Features.CADCode.Domain.CSV;

namespace ApplicationCore.Features.CADCode.Services;

internal interface ICSVParser {

    public Task<CSVParseResult> ParsePartsAsync(string filepath);

}