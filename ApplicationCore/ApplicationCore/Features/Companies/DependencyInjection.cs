using Domain.Companies.Entities;
using Domain.Companies.Customers;
using Domain.Companies.Customers.Commands;
using Domain.Companies.Customers.Create;
using Domain.Companies.Customers.Edit;
using Domain.Companies.Customers.List;
using Domain.Companies.Customers.Queries;
using Domain.Companies.Vendors;
using Domain.Companies.Vendors.Commands;
using Domain.Companies.Vendors.Edit;
using Domain.Companies.Vendors.List;
using Domain.Companies.Vendors.Queries;
using ApplicationCore.Infrastructure.Bus;
using ApplicationCore.Shared.Data.Companies;
using Microsoft.Extensions.DependencyInjection;

namespace Domain.Companies;

internal static class DependencyInjection {

    public static IServiceCollection AddCompanies(this IServiceCollection services)
        => services.AddCustomers()
                    .AddVendors()
                    .AddTransient<ICompaniesDbConnectionFactory, SqliteCompaniesDbConnectionFactory>()
                    .AddViewModels();

    private static IServiceCollection AddViewModels(this IServiceCollection services)
        => services.AddTransient<CreateCustomerViewModel>()
                    .AddTransient<VendorListViewModel>()
                    .AddTransient<CustomerListViewModel>()
                    .AddTransient<EditCustomerViewModel>()
                    .AddTransient<EditVendorViewModel>();

    public static IServiceCollection AddVendors(this IServiceCollection services) {

        services.AddTransient<Contracts.CompanyDirectory.GetVendorByIdAsync>(sp => {

            var bus = sp.GetRequiredService<IBus>();
            return async (id) => {
                var result = await bus.Send(new GetVendorById.Query(id));
                return result.Match<Vendor?>(
                    vendor => vendor,
                    error => null
                );
            };

        });

        services.AddTransient<Contracts.CompanyDirectory.GetVendorNameByIdAsync>(sp => {

            var bus = sp.GetRequiredService<IBus>();
            return async (id) => {
                var result = await bus.Send(new GetVendorNameById.Query(id));
                return result.Match<string?>(
                    vendorName => vendorName,
                    error => null
                );
            };

        });

        services.AddTransient<Contracts.CompanyDirectory.UpdateVendorAsync>(sp => {

            var bus = sp.GetRequiredService<IBus>();
            return async (vendor) => {
                _ = await bus.Send(new UpdateVendor.Command(vendor));
            };

        });

        return services;

    }

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

        services.AddTransient<Contracts.CompanyDirectory.GetCustomerNameByIdAsync>(sp => {

            var bus = sp.GetRequiredService<IBus>();
            return async (id) => {
                var result = await bus.Send(new GetCustomerNameById.Query(id));
                return result.Match<string?>(
                    customerName => customerName,
                    error => null
                );
            };

        });

        services.AddTransient<Contracts.CompanyDirectory.GetCustomerOrderPrefixByIdAsync>(sp => {

            var bus = sp.GetRequiredService<IBus>();
            return async (customerId) => {
                var result = await bus.Send(new GetCustomerOrderPrefixById.Query(customerId));
                return result.Match<string?>(
                    orderNumberPrefix => orderNumberPrefix,
                    error => null
                );
            };

        });

        services.AddTransient<Contracts.CompanyDirectory.GetCustomerWorkingDirectoryRootByIdAsync>(sp => {

            var bus = sp.GetRequiredService<IBus>();
            return async (customerId) => {
                var result = await bus.Send(new GetCustomerWorkingDirectoryRootById.Query(customerId));
                return result.Match<string?>(
                    workingDirectoryRoot => workingDirectoryRoot,
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

        services.AddTransient<Contracts.CompanyDirectory.GetCustomerIdByNameAsync>(sp => {

            var bus = sp.GetRequiredService<IBus>();
            return async (name) => {
                var result = await bus.Send(new GetCustomerIdByName.Query(name));
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