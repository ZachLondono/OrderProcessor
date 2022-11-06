using System.Text.Json.Serialization;
using System.Text.Json;
using ApplicationCore.Features.CADCode.Services.Domain.CADCode.Configuration;

namespace ApplicationCore.Features.CADCode.Services.Services.CADCodeGCode.Configuration;

public class JSONCADCodeConfigurationProvider : ICADCodeConfigurationProvider {

    private readonly string _filename;

    public JSONCADCodeConfigurationProvider(string filename) {
        _filename = filename;
    }

    public CADCodeConfiguration GetConfiguration() {

        var options = new JsonSerializerOptions {
            Converters = {
                new JsonStringEnumConverter( JsonNamingPolicy.CamelCase)
            },
        };

        CADCodeConfiguration? config;
        using (var stream = File.OpenRead(_filename)) {
            config = JsonSerializer.Deserialize<CADCodeConfiguration>(stream, options);
        }
        if (config is null) {
            throw new InvalidDataException("Could not read config file");
        }

        return config;

    }
}

public class JSONCADCodeMachineConfigurationProvider : ICADCodeMachineConfigurationProvider {

    private readonly string _filename;

    public JSONCADCodeMachineConfigurationProvider(string filename) {
        _filename = filename;
    }

    public IEnumerable<CADCodeMachineConfiguration> GetConfigurations() {

        var options = new JsonSerializerOptions {
            Converters = {
                new JsonStringEnumConverter( JsonNamingPolicy.CamelCase)
            },
        };

        IEnumerable<CADCodeMachineConfiguration>? config;
        using (var stream = File.OpenRead(_filename)) {
            config = JsonSerializer.Deserialize<IEnumerable<CADCodeMachineConfiguration>>(stream, options);
        }
        if (config is null) {
            throw new InvalidDataException("Could not read config file");
        }

        return config;
    }

}
