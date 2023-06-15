using System.Data;
using System.Text.Json;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Shared.Domain;
using Dapper;

namespace ApplicationCore.Shared.Data.Ordering;

public class SqliteUBoxDimensionTypeHandler : SqlMapper.TypeHandler<UBoxDimensions?> {

    public override UBoxDimensions? Parse(object? value) {
        if (value is null) return null;
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
