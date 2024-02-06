using Microsoft.Extensions.DependencyInjection;
using Domain.Infrastructure.Bus;
using Companies.Infrastructure;
using Domain.Companies;
using Companies.Vendors.Queries;
using Companies.Customers.List;
using Companies.Customers.Create;
using Companies.Vendors.List;
using Companies.Customers.Edit;
using Companies.Vendors.Edit;
using Companies.Vendors.Commands;
using Companies.Customers.Queries;
using Companies.Customers.Commands;

namespace Companies;

public static class DependencyInjection {

    public static IServiceCollection AddCompanies(this IServiceCollection services)
        => services.AddCustomers()
                    .AddVendors()
                    .AddTransient<ICompaniesDbConnectionFactory, SqliteCompaniesDbConnectionFactory>()
                    .AddViewModels()
                    .AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

    private static IServiceCollection AddViewModels(this IServiceCollection services)
        => services.AddTransient<CreateCustomerViewModel>()
                    .AddTransient<VendorListViewModel>()
                    .AddTransient<CustomerListViewModel>()
                    .AddTransient<EditCustomerViewModel>()
                    .AddTransient<EditVendorViewModel>();

    public static IServiceCollection AddVendors(this IServiceCollection services) {

        services.AddTransient<CompanyDirectory.GetVendorByIdAsync>(sp => {

            var bus = sp.GetRequiredService<IBus>();
            return async (id) => {
                var result = await bus.Send(new GetVendorById.Query(id));
                return result.Match(
                    vendor => vendor,
                    error => null
                );
            };

        });

        services.AddTransient<CompanyDirectory.GetVendorNameByIdAsync>(sp => {

            var bus = sp.GetRequiredService<IBus>();
            return async (id) => {
                var result = await bus.Send(new GetVendorNameById.Query(id));
                return result.Match<string?>(
                    vendorName => vendorName,
                    error => null
                );
            };

        });

        services.AddTransient<CompanyDirectory.UpdateVendorAsync>(sp => {

            var bus = sp.GetRequiredService<IBus>();
            return async (vendor) => {
                _ = await bus.Send(new UpdateVendor.Command(vendor));
            };

        });

        return services;

    }

    public static IServiceCollection AddCustomers(this IServiceCollection services) {

        services.AddTransient<CompanyDirectory.GetCustomerByIdAsync>(sp => {

            var bus = sp.GetRequiredService<IBus>();
            return async (id) => {
                var result = await bus.Send(new GetCustomerById.Query(id));
                return result.Match(
                    customer => customer,
                    error => null
                );
            };

        });

        services.AddTransient<CompanyDirectory.GetCustomerNameByIdAsync>(sp => {

            var bus = sp.GetRequiredService<IBus>();
            return async (id) => {
                var result = await bus.Send(new GetCustomerNameById.Query(id));
                return result.Match<string?>(
                    customerName => customerName,
                    error => null
                );
            };

        });

        services.AddTransient<CompanyDirectory.GetCustomerOrderPrefixByIdAsync>(sp => {

            var bus = sp.GetRequiredService<IBus>();
            return async (customerId) => {
                var result = await bus.Send(new GetCustomerOrderPrefixById.Query(customerId));
                return result.Match(
                    orderNumberPrefix => orderNumberPrefix,
                    error => null
                );
            };

        });

        services.AddTransient<CompanyDirectory.GetCustomerWorkingDirectoryRootByIdAsync>(sp => {

            var bus = sp.GetRequiredService<IBus>();
            return async (customerId) => {
                var result = await bus.Send(new GetCustomerWorkingDirectoryRootById.Query(customerId));
                return result.Match(
                    workingDirectoryRoot => workingDirectoryRoot,
                    error => null
                );
            };

        });

        services.AddTransient<CompanyDirectory.GetCustomerIdByAllmoxyIdAsync>(sp => {

            var bus = sp.GetRequiredService<IBus>();
            return async (allmoxyId) => {
                var result = await bus.Send(new GetCustomerIdByAllmoxyId.Query(allmoxyId));
                return result.Match(
                    customerId => customerId,
                    error => null
                );
            };

        });

        services.AddTransient<CompanyDirectory.GetCustomerIdByNameAsync>(sp => {

            var bus = sp.GetRequiredService<IBus>();
            return async (name) => {
                var result = await bus.Send(new GetCustomerIdByName.Query(name));
                return result.Match(
                    customerId => customerId,
                    error => null
                );
            };

        });

        services.AddTransient<CompanyDirectory.InsertCustomerAsync>(sp => {

            var bus = sp.GetRequiredService<IBus>();
            return async (customer, allmoxyId) => {
                _ = await bus.Send(new InsertCustomer.Command(customer, allmoxyId));
            };

        });

        services.AddTransient<CompanyDirectory.UpdateCustomerAsync>(sp => {

            var bus = sp.GetRequiredService<IBus>();
            return async (customer) => {
                _ = await bus.Send(new UpdateCustomer.Command(customer));
            };

        });

        return services;

    }

}