using ApplicationCore.Features.CNC.CSV.Contracts;
using ApplicationCore.Features.Shared.Services;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace ApplicationCore.Features.CNC.CSV;

internal class CSVReader : ICSVReader {

    private readonly ILogger<CSVReader> _logger;
    private readonly IFileReader _fileReader;

    public CSVReader(ILogger<CSVReader> logger, IFileReader fileReader) {
        _logger = logger;
        _fileReader = fileReader;
    }

    public async Task<CSVReadResult> ReadTokensFromFilesAsync(string filepath) {

        _logger.LogInformation("Reading CSV tokens from file: {FilePath}", filepath);

        var config = new CsvConfiguration(CultureInfo.InvariantCulture) {
            PrepareHeaderForMatch = args => args.Header.Replace(" ", "")
        };

        using var fs = _fileReader.OpenReadFileStream(filepath);
        using var reader = new StreamReader(fs);
        using var csv = new CsvReader(reader, config);

        List<CSVToken> tokens = new();

        await csv.ReadAsync();
        csv.ReadHeader();

        while (await csv.ReadAsync()) {

            try {

                var token = csv.GetRecord<CSVToken>();

                if (token is null) {
                    _logger.LogWarning("Null value read from CSV file");
                    continue;
                }

                tokens.Add(token);

            } catch (Exception ex) {
                _logger.LogError("Exception thrown while reading token from CSV file: {Exception}", ex);
            }


        }

        _logger.LogInformation("CSV records read: {Count}", tokens.Count);

        return new() {
            Tokens = tokens
        };

    }

}
