using ApplicationCore.Features.Companies.Customers;
using ApplicationCore.Features.Companies.Customers.Create;
using ApplicationCore.Features.Companies.Customers.Edit;
using ApplicationCore.Features.Companies.Customers.List;
using ApplicationCore.Features.Companies.Data;
using ApplicationCore.Features.Companies.Vendors;
using ApplicationCore.Features.Companies.Vendors.Edit;
using ApplicationCore.Features.Companies.Vendors.List;
using Microsoft.Extensions.DependencyInjection;

namespace ApplicationCore.Features.Companies;

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

}