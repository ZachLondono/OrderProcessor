using System.Data;
using Dapper;

namespace ApplicationCore.Infrastructure.Data;

public class SqliteDecimalTypeHandler : SqlMapper.TypeHandler<decimal> {

    public override void SetValue(IDbDataParameter parameter, decimal value) {
        parameter.Value = value;
    }

    public override decimal Parse(object value) {
        // This is neccessary, otherwise whole number values are read as doubles from SQLite database and throws an invalid cast exception when trying to cast to decimal
        return Convert.ToDecimal(value);
    }

}