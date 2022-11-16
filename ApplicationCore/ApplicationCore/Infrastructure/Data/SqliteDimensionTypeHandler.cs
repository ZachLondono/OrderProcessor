using System.Data;
using ApplicationCore.Features.Orders.Domain.ValueObjects;
using Dapper;

namespace ApplicationCore.Infrastructure.Data;

public class SqliteDimensionTypeHandler : SqlMapper.TypeHandler<Dimension> {
    public override void SetValue(IDbDataParameter parameter, Dimension dimension) {
        parameter.Value = dimension.AsMillimeters();
    }
    public override Dimension Parse(object value) {
        if (value is double || value is float)
            return Dimension.FromMillimeters((double)value);
        else return Dimension.FromMillimeters(double.Parse(value.ToString()));
    }
}
