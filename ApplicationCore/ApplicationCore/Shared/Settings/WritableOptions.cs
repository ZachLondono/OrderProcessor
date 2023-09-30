using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ApplicationCore.Shared.Settings;

public class WritableOptions<T> : IWritableOptions<T> where T : class, new() {

    private readonly IOptionsMonitor<T> _options;
    private readonly string _section;
    private readonly string _file;

    public WritableOptions(IOptionsMonitor<T> options, string section, string file) {
        _options = options;
        _section = section;
        _file = file;
    }

    public T Value => _options.CurrentValue;
    public T Get(string? name) => _options.Get(name);

    public void Update(Action<T> applyChanges) {

        var filePath = Path.GetFullPath(_file);

        var obj = JsonSerializer.Deserialize<JsonObject>(File.ReadAllText(filePath));

        if (obj is null || !obj.TryGetPropertyValue(_section, out JsonNode? node)) {
            return;
        }

        var section = JsonSerializer.Deserialize<T>(node);

        if (section is null) {
            return;
        }

        applyChanges(section);

        obj[_section] = JsonNode.Parse(JsonSerializer.Serialize(section));
        File.WriteAllText(filePath, JsonSerializer.Serialize(obj, new JsonSerializerOptions() {
            WriteIndented = true
        }));

    }

}
