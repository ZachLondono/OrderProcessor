using System.Data;
using System.Text.Json;
using ApplicationCore.Features.Shared.Domain;
using Dapper;

namespace ApplicationCore.Features.Orders.Data;

public class DimensionArrayTypeHandler : SqlMapper.TypeHandler<Dimension[]> {

    public override Dimension[] Parse(object? value) {

        if (value is not string) {
            throw new ArgumentException("Value is incorrect type for dimension array");
        }

        var val = JsonSerializer.Deserialize<double[]>((string)value);

        if (val is null) {
            throw new ArgumentException("Value is not valid dimension array");
        }

        Dimension[] dimensions = new Dimension[val.Length];
        for (int i = 0; i < val.Length; i++ ) {
            dimensions[i] = Dimension.FromMillimeters(val[i]);
        }
        return dimensions;

    }

    public override void SetValue(IDbDataParameter parameter, Dimension[] value) {

        double[] doubles = new double[value.Length];
        for (int i = 0; i < value.Length; i++) {
            doubles[i] = value[i].AsMillimeters();
        }

        parameter.Value = JsonSerializer.Serialize(doubles);

    }

}
