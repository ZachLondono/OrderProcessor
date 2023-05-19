using ApplicationCore.Features.CNC.Contracts;
using ApplicationCore.Features.Companies.Contracts;
using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace ApplicationCore.Features.Orders.OrderRelease.Handlers.CNC;

public class CNCReleaseDecoratorFactory {

    private readonly IServiceProvider _serviceProvider;
    private readonly CompanyDirectory.GetCustomerByIdAsync _getCustomerByIdAsync;
    private readonly CompanyDirectory.GetVendorByIdAsync _getVendorByIdAsync;

    public CNCReleaseDecoratorFactory(IServiceProvider serviceProvider, CompanyDirectory.GetCustomerByIdAsync getCustomerByIdAsync, CompanyDirectory.GetVendorByIdAsync getVendorByIdAsync) {
        _serviceProvider = serviceProvider;
        _getCustomerByIdAsync = getCustomerByIdAsync;
        _getVendorByIdAsync = getVendorByIdAsync;
    }

    public async Task<ICNCReleaseDecorator> Create(string reportFilePath, Order order) {

        var customer = await _getCustomerByIdAsync(order.CustomerId);
        var vendor = await _getVendorByIdAsync(order.VendorId);

        string customerName = customer?.Name ?? "";
        string vendorName = vendor?.Name ?? "";

        var decorator = _serviceProvider.GetRequiredService<ICNCReleaseDecorator>();
        await decorator.LoadDataFromFile(reportFilePath, order.OrderDate, customerName, vendorName);
        return decorator;

    }

}