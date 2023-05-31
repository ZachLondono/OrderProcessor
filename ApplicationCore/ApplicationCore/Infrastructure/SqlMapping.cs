using ApplicationCore.Features.Orders.Data;
using ApplicationCore.Infrastructure.Data;
using Dapper;

namespace ApplicationCore.Infrastructure;

public static class SqlMapping {

    public static void AddSqlMaps() {
        SqlMapper.RemoveTypeMap(typeof(decimal));
        SqlMapper.AddTypeHandler(new SqliteDecimalTypeHandler());
        SqlMapper.AddTypeHandler(new SqliteDimensionTypeHandler());
        SqlMapper.AddTypeHandler(new SqliteDictionaryEnumerableTypeHandler());
        SqlMapper.AddTypeHandler(new DimensionArrayTypeHandler());
        SqlMapper.RemoveTypeMap(typeof(Guid));
        SqlMapper.RemoveTypeMap(typeof(Guid?));
        SqlMapper.AddTypeHandler(new SqliteGuidTypeHandler());
        AddOrderingSqlMap();
    }

    public static void AddOrderingSqlMap() {
        SqlMapper.AddTypeHandler(new ToeTypeTypeHandler());
        SqlMapper.AddTypeHandler(new SqliteFixedDivdersCountsTypeHandler());
        SqlMapper.AddTypeHandler(new SqliteUBoxDimensionTypeHandler());
    }

}