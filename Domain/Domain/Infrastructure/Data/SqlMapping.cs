using Domain.Infrastructure.Data.TypeHandlers;
using Dapper;
using Domain.Orders.Persistance;

namespace Domain.Infrastructure.Data;

public static class SqlMapping {

    public static void AddSqlMaps() {
        SqlMapper.RemoveTypeMap(typeof(decimal));
        SqlMapper.AddTypeHandler(new SqliteDecimalTypeHandler());
        SqlMapper.AddTypeHandler(new SqliteDimensionTypeHandler());
        SqlMapper.AddTypeHandler(new SqliteDictionaryEnumerableTypeHandler());
        SqlMapper.AddTypeHandler(new DimensionArrayTypeHandler());
        SqlMapper.AddTypeHandler(new StringListTypeHandler());
        SqlMapper.RemoveTypeMap(typeof(Guid));
        SqlMapper.RemoveTypeMap(typeof(Guid?));
        SqlMapper.AddTypeHandler(new SqliteGuidTypeHandler());
        AddOrderingSqlMap();
    }

    public static void AddOrderingSqlMap() {
        SqlMapper.AddTypeHandler(new ToeTypeTypeHandler());
        SqlMapper.AddTypeHandler(new SqliteFixedDividersCountsTypeHandler());
        SqlMapper.AddTypeHandler(new SqliteUBoxDimensionTypeHandler());
    }

}