using ApplicationCore.Features.CADCode.Domain.CSV;
using ApplicationCore.Shared;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using System.Globalization;
using static ApplicationCore.Features.CADCode.Domain.CSV.CSVParseResult;

namespace ApplicationCore.Features.CADCode.Services;

internal class CSVParser : ICSVParser {

    private readonly ILogger<CSVParser> _logger;
    private readonly IFileReader _fileReader;

    public CSVParser(ILogger<CSVParser> logger, IFileReader fileReader) {
        _logger = logger;
        _fileReader = fileReader;
    }

    public async Task<CSVParseResult> ParsePartsAsync(string filepath) {

        var config = new CsvConfiguration(CultureInfo.InvariantCulture) {
            PrepareHeaderForMatch = args => args.Header.Replace(" ", "")
        };

        List<ParseMessage> messages = new();
        Stack<CSVPart> parts = new();

        using var fs = _fileReader.OpenReadFileStream(filepath);
        using var reader = new StreamReader(fs);
        using var csv = new CsvReader(reader, config);

        await csv.ReadAsync();
        csv.ReadHeader();

        while (await csv.ReadAsync()) {

            var tokenName = csv.GetField(5)?.ToLower() ?? string.Empty;

            if (tokenName.Equals("border")) {

                try {

                    var border = csv.GetRecord<CSVBorder>();
                    if (border is null) continue;
                    parts.Push(new() {
                        Border = border
                    });
					_logger.LogInformation("Border record read: {border}", border);

				} catch (Exception ex) {
                    _logger.LogError("Exception while parsing border {EX}", ex);
                    messages.Add(new(MessageSeverity.Warning, "Could not parse border"));
                }

            } else if (!string.IsNullOrWhiteSpace(tokenName)) {

                try {

                    var token = csv.GetRecord<CSVToken>();
                    if (token is null) continue;
                    parts.Peek().Tokens.Add(token);
					_logger.LogInformation("Token record read: {Token}", token);

				} catch (Exception ex) {
                    _logger.LogError("Exception while parsing token {EX}", ex);
                    messages.Add(new(MessageSeverity.Warning, "Could not parse token"));
                }
            }

        }

        return new() {
            Messages = messages,
            Parts = parts
        };

	}

}
