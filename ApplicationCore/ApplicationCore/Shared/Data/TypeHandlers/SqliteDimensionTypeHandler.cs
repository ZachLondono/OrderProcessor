using System.Data;
using Domain.ValueObjects;
using Dapper;

namespace ApplicationCore.Shared.Data.TypeHandlers;

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
