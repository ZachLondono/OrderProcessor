using System.Data;
using ApplicationCore.Features.Orders.Domain.ValueObjects;
using Dapper;

namespace ApplicationCore.Infrastructure.Data;

public class SqliteDimensionTypeHandler : SqlMapper.TypeHandler<Dimension> {
    public override void SetValue(IDbDataParameter parameter, Dimension dimension) {
        parameter.Value = dimension.AsMillimeters();
    }
    public override Dimension Parse(object value) {
        return Dimension.FromMillimeters((double)value);
    }
}
