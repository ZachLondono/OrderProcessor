using System.Data;
using System.Text.Json;
using Dapper;

namespace ApplicationCore.Shared.Data.TypeHandlers;

public class SqliteDictionaryEnumerableTypeHandler : SqlMapper.TypeHandler<IDictionary<string, string>> {

    public override IDictionary<string, string> Parse(object value) {
        var val = JsonSerializer.Deserialize<IDictionary<string, string>>((string)value);
        if (val is null) return new Dictionary<string, string>();
        return val;
    }

    public override void SetValue(IDbDataParameter parameter, IDictionary<string, string> value) {
        parameter.Value = JsonSerializer.Serialize(value);
    }

}
