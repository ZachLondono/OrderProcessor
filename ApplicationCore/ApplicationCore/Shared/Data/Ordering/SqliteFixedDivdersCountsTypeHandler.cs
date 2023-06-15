using System.Data;
using System.Text.Json;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using Dapper;

namespace ApplicationCore.Features.Shared.Data.Ordering;

public class SqliteFixedDividersCountsTypeHandler : SqlMapper.TypeHandler<FixedDivdersCounts?> {

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