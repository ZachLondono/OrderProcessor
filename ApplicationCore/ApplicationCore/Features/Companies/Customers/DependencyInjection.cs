using ApplicationCore.Features.Companies.Contracts.Entities;
using ApplicationCore.Features.Companies.Customers.Commands;
using ApplicationCore.Features.Companies.Customers.Queries;
using ApplicationCore.Infrastructure.Bus;
using Microsoft.Extensions.DependencyInjection;

namespace ApplicationCore.Features.Companies.Customers;

internal static class DependencyInjection {

    public static IServiceCollection AddCustomers(this IServiceCollection services) {

        services.AddTransient<Contracts.CompanyDirectory.GetCustomerByIdAsync>(sp => {

            var bus = sp.GetRequiredService<IBus>();
            return async (id) => {
                var result = await bus.Send(new GetCustomerById.Query(id));
                return result.Match<Customer?>(
                    customer => customer,
                    error => null
                );
            };

        });

        services.AddTransient<Contracts.CompanyDirectory.GetCustomerIdByAllmoxyIdAsync>(sp => {

            var bus = sp.GetRequiredService<IBus>();
            return async (allmoxyId) => {
                var result = await bus.Send(new GetCustomerIdByAllmoxyId.Query(allmoxyId));
                return result.Match<Guid?>(
                    customerId => customerId,
                    error => null
                );
            };

        });

        services.AddTransient<Contracts.CompanyDirectory.InsertCustomerAsync>(sp => {

            var bus = sp.GetRequiredService<IBus>();
            return async (customer, allmoxyId) => {
                _ = await bus.Send(new InsertCustomer.Command(customer, allmoxyId));
            };

        });

        services.AddTransient<Contracts.CompanyDirectory.UpdateCustomerAsync>(sp => {

            var bus = sp.GetRequiredService<IBus>();
            return async (customer) => {
                _ = await bus.Send(new UpdateCustomer.Command(customer));
            };

        });

        return services;

    }

}
