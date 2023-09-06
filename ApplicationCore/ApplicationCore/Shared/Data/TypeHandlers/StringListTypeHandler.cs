using System.Data;
using System.Text.Json;
using Dapper;

namespace ApplicationCore.Shared.Data.TypeHandlers;

public class StringListTypeHandler : SqlMapper.TypeHandler<List<string>> {

    public override List<string> Parse(object? value) {

        if (value is not string) {
            throw new ArgumentException("Value is incorrect type for string list");
        }

        var val = JsonSerializer.Deserialize<List<string>>((string)value);

        if (val is null) {
            throw new ArgumentException("Value is not valid sstring list");
        }

        return val;

    }

    public override void SetValue(IDbDataParameter parameter, List<string> value) {
        parameter.Value = JsonSerializer.Serialize(value);
    }

}
