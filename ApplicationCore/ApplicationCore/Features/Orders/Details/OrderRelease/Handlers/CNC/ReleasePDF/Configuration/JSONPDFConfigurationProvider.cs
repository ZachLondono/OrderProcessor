using System.Text.Json.Serialization;
using System.Text.Json;

namespace ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.ReleasePDF.Configuration;

public class JSONPDFConfigurationProvider : IPDFConfigurationProvider {

    private readonly string _filepath;

    public JSONPDFConfigurationProvider(string filepath) => _filepath = filepath;

    public PDFConfiguration GetConfiguration() {

        using var stream = File.OpenRead(_filepath);

        var options = new JsonSerializerOptions {
            Converters = {
                new JsonStringEnumConverter( JsonNamingPolicy.CamelCase)
            },
        };

        var config = JsonSerializer.Deserialize<PDFConfiguration>(stream, options);

        if (config is null) return new();
        return config;

    }
}