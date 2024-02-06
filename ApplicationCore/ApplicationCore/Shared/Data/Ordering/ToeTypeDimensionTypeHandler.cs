using Dapper;
using Domain.Orders.ValueObjects;
using System.Data;

namespace ApplicationCore.Shared.Data.Ordering;

public class ToeTypeTypeHandler : SqlMapper.TypeHandler<ToeType> {
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