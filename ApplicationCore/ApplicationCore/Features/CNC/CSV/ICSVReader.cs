using ApplicationCore.Features.CNC.CSV.Contracts;

namespace ApplicationCore.Features.CNC.CSV;

public interface ICSVReader {

    public Task<CSVReadResult> ReadTokensFromFilesAsync(string filepath);

}