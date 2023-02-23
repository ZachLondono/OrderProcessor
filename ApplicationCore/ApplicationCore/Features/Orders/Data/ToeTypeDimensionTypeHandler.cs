using ApplicationCore.Features.Orders.Shared.Domain;
using Dapper;
using System.Data;

namespace ApplicationCore.Features.Orders.Data;

public class ToeTypeDimensionTypeHandler : SqlMapper.TypeHandler<ToeType> {
    public override void SetValue(IDbDataParameter parameter, ToeType toetype) {
        parameter.Value = toetype.PSIParameter;
    }
    public override ToeType Parse(object value) {
        if (value is string psiParam) {
            return ToeType.FromPSIParameter(psiParam);
        }
        throw new ArgumentException("Value is not a valid to type parameter");
    }
}