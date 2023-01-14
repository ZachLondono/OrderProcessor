using ApplicationCore.Infrastructure.Data;
using Dapper;
using MediatR;
using MediatR.Pipeline;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Reflection;

namespace ApplicationCore.Infrastructure;

public static class DependencyInjection {

    public static IServiceCollection AddApplicationInfrastructure(this IServiceCollection services, IConfiguration configuration) {

        var cacheConfig = configuration.GetRequiredSection("Cache").Get<CacheConfiguration>();

        if (cacheConfig is null || !cacheConfig.UseLocalCache) {
            services.AddSingleton<IBus, MediatRBus>();
        } else {
            services.AddSingleton(cacheConfig);
            services.AddSingleton<MediatRBus>();
            services.AddSingleton<IBus, CachedBusDecorator>();
        }

        services.AddSingleton<IUIBus, UIBus>();

        SqlMapper.RemoveTypeMap(typeof(decimal));
        SqlMapper.AddTypeHandler(new SqliteDecimalTypeHandler());
        SqlMapper.AddTypeHandler(new SqliteFixedDivdersCountsTypeHandler());
        SqlMapper.AddTypeHandler(new SqliteUBoxDimensionTypeHandler());
        SqlMapper.AddTypeHandler(new SqliteDimensionTypeHandler());
        SqlMapper.AddTypeHandler(new SqliteDictionaryEnumerableTypeHandler());
        SqlMapper.AddTypeHandler(new SqliteGuidTypeHandler());
        SqlMapper.RemoveTypeMap(typeof(Guid));
        SqlMapper.RemoveTypeMap(typeof(Guid?));
        services.AddSingleton<IDbConnectionFactory, SqliteDbConnectionFactory>();

        RegisterExceptionHandlers(services);

        return services;

    }

    private static void RegisterExceptionHandlers(IServiceCollection services) {

        // TODO: create source generator so this doesn't run on startup

        var commands = typeof(DependencyInjection).GetTypeInfo()
                                                        .Assembly
                                                        .GetTypes()
                                                        .Where(t => typeof(ICommand).IsAssignableFrom(t) && !t.IsAbstract && t.IsClass)
                                                        .ToList();
        foreach (var type in commands) {

            var implementationType = typeof(ExceptionBehaviorB<>).MakeGenericType(type);
            var serviceType = typeof(AsyncRequestExceptionHandler<,>).MakeGenericType(type, typeof(Response));

            Debug.WriteLine($"{serviceType} => {implementationType}");

            services.AddTransient(serviceType, implementationType);

        }

        var queries = GetAllTypesImplementingOpenGenericType(typeof(IQuery<>), typeof(ICommand<>), typeof(DependencyInjection).GetTypeInfo().Assembly).ToList();

        foreach (var type in queries) {

            var successType = type.GetInterfaces()[0].GetGenericArguments()[0];

            var implementationType = typeof(ExceptionBehaviorA<,>).MakeGenericType(type, successType);
            var responseType = typeof(Response<>).MakeGenericType(successType);
            var serviceType = typeof(AsyncRequestExceptionHandler<,>).MakeGenericType(type, responseType);

            Debug.WriteLine($"{serviceType} => {implementationType}");

            services.AddTransient(serviceType, implementationType);

        }
    }

    public static IEnumerable<Type> GetAllTypesImplementingOpenGenericType(Type openGenericType1, Type openGenericType2, Assembly assembly) {
        return from x in assembly.GetTypes()
               from z in x.GetInterfaces()
               let y = x.BaseType
               where
               ((y != null && y.IsGenericType &&
               (openGenericType1.IsAssignableFrom(y.GetGenericTypeDefinition()) || openGenericType2.IsAssignableFrom(y.GetGenericTypeDefinition()))) ||
               (z.IsGenericType &&
               (openGenericType1.IsAssignableFrom(z.GetGenericTypeDefinition()) || openGenericType2.IsAssignableFrom(z.GetGenericTypeDefinition())))) && y.IsClass && !y.IsAbstract
               select x;
    }

}