using ApplicationCore.Features.Companies.Contracts.Entities;
using ApplicationCore.Features.Companies.Vendors.Commands;
using ApplicationCore.Features.Companies.Vendors.Queries;
using ApplicationCore.Infrastructure.Bus;
using Microsoft.Extensions.DependencyInjection;

namespace ApplicationCore.Features.Companies.Vendors;

internal static class DependencyInjection {

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

}