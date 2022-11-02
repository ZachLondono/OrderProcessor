using System.Data;
using System.Text.Json;
using ApplicationCore.Features.Orders.Domain.ValueObjects;
using Dapper;

namespace ApplicationCore.Infrastructure.Data;

public class SqliteFixedDivdersCountsTypeHandler : SqlMapper.TypeHandler<FixedDivdersCounts?> {

    public override FixedDivdersCounts? Parse(object value) {
        var val = JsonSerializer.Deserialize<FixedDivdersCounts>((string)value);
        return val;
    }

    public override void SetValue(IDbDataParameter parameter, FixedDivdersCounts? value) {
        if (value is null) {
            parameter.Value = null;
        } else {
            parameter.Value = JsonSerializer.Serialize(value);
        }
    }

}