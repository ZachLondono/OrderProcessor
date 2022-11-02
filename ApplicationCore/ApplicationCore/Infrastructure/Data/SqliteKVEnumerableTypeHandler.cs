using System.Data;
using System.Text.Json;
using Dapper;

namespace ApplicationCore.Infrastructure.Data;

public class SqliteKVEnumerableTypeHandler : SqlMapper.TypeHandler<IEnumerable<KeyValuePair<string, string>>> {
    
    public override IEnumerable<KeyValuePair<string, string>> Parse(object value) {
        var val =  JsonSerializer.Deserialize<IEnumerable<KeyValuePair<string, string>>>((string)value);
        if (val is null) return new Dictionary<string, string>();
        return val;
    }

    public override void SetValue(IDbDataParameter parameter, IEnumerable<KeyValuePair<string, string>> value) {
        parameter.Value = JsonSerializer.Serialize(value);
    }

}
