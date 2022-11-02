using System.Data;
using System.Text.Json;
using ApplicationCore.Features.Orders.Domain.ValueObjects;
using Dapper;

namespace ApplicationCore.Infrastructure.Data;

public class SqliteUBoxDimensionTypeHandler : SqlMapper.TypeHandler<UBoxDimensions?> {

    public override UBoxDimensions? Parse(object? value) {
        var val = JsonSerializer.Deserialize<UBoxDimensionsModel>((string)value);
        if (val is null) return null;
        return new UBoxDimensions() {
            A = Dimension.FromMillimeters(val.A),
            B = Dimension.FromMillimeters(val.B),
            C = Dimension.FromMillimeters(val.C)
        };
    }

    public override void SetValue(IDbDataParameter parameter, UBoxDimensions? value) {
        if (value is null) {
            parameter.Value = null;
        } else {
            parameter.Value = JsonSerializer.Serialize(new UBoxDimensionsModel() { 
                A = value.A.AsMillimeters(),
                B = value.B.AsMillimeters(),
                C = value.C.AsMillimeters(),
            });
        }
    }

    class UBoxDimensionsModel {
        public double A { get; set; }
        public double B { get; set; }
        public double C { get; set; }
    }

}
