using ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.ReleasePDF.Contracts;
using ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.ReleasePDF.Services;
using QuestPDF.Infrastructure;
using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Features.Companies.Contracts;
using ApplicationCore.Features.Tools.Contracts;
using ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.ReleasePDF.WSXML;

namespace ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.ReleasePDF;

internal class CNCReleaseDecorator : ICNCReleaseDecorator {

    private readonly IReleasePDFWriter _pdfService;
    private readonly CompanyDirectory.GetCustomerByIdAsync _getCustomerByIdAsync;
    private readonly CompanyDirectory.GetVendorByIdAsync _getVendorByIdAsync;
    private readonly CNCToolBox.GetToolCarousels _getToolCarousels;

    public string ReportFilePath { get; set; } = string.Empty;

    public CNCReleaseDecorator(IReleasePDFWriter pdfService, CompanyDirectory.GetCustomerByIdAsync getCustomerByIdAsync, CompanyDirectory.GetVendorByIdAsync getVendorByIdAsync, CNCToolBox.GetToolCarousels getToolCarousels) {
        _pdfService = pdfService;
        _getCustomerByIdAsync = getCustomerByIdAsync;
        _getVendorByIdAsync = getVendorByIdAsync;
        _getToolCarousels = getToolCarousels;
    }

    public async Task Decorate(Order order, IDocumentContainer container) {

        var vendor = await _getVendorByIdAsync(order.VendorId);
        var customer = await _getCustomerByIdAsync(order.CustomerId);
        var toolCarousels = await _getToolCarousels();

        ReleasedJob? releasedJob = WSXMLParser.ParseWSXMLReport(ReportFilePath, order.OrderDate, customer?.Name ?? "", vendor?.Name ?? "", toolCarousels);

        if (releasedJob is null) {

            return;

        }

        var decorators = _pdfService.GenerateDecorators(releasedJob);

        foreach (var decorator in decorators) {

            await decorator.Decorate(order, container);

        }

        return;

    }
}