using ApplicationCore.Features.CNC.ReleaseEmail;
using ApplicationCore.Features.Companies.Contracts;
using ApplicationCore.Features.Orders.OrderRelease;
using ApplicationCore.Features.Orders.OrderRelease.Handlers.CNC;
using ApplicationCore.Features.Orders.OrderRelease.Handlers.Invoice;
using ApplicationCore.Features.Orders.OrderRelease.Handlers.JobSummary;
using ApplicationCore.Features.Orders.OrderRelease.Handlers.PackingList;
using ApplicationCore.Shared.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace ApplicationCore.Tests.Unit.Orders;

public class ReleaseServiceTests {

    private readonly ReleaseService _sut;

    private readonly ILogger<ReleaseService> _logger;
    private readonly IFileReader _fileReader;
    private readonly InvoiceDecoratorFactory _invoiceDecorator;
    private readonly PackingListDecoratorFactory _packingListDecorator;
    private readonly CNCReleaseDecoratorFactory _cncReleaseDecoratorFactory;
    private readonly JobSummaryDecoratorFactory _jobSummaryDecorator;
    private readonly CompanyDirectory.GetCustomerByIdAsync _getCustomerByIdAsync;
    private readonly CompanyDirectory.GetVendorByIdAsync _getVendorByIdAsync;
    private readonly IEmailService _emailService;

    public ReleaseServiceTests() {
        _fileReader = new FileReader();
        _logger = Substitute.For<ILogger<ReleaseService>>();
        _getCustomerByIdAsync = Substitute.For<CompanyDirectory.GetCustomerByIdAsync>();
        _getVendorByIdAsync = Substitute.For<CompanyDirectory.GetVendorByIdAsync>();
        var sp = Substitute.For<IServiceProvider>();
        _cncReleaseDecoratorFactory = new CNCReleaseDecoratorFactory(sp);
        _invoiceDecorator = new(sp);
        _packingListDecorator = new(sp);
        _jobSummaryDecorator = new(sp);
        _emailService = Substitute.For<IEmailService>();

        _sut = new ReleaseService(_logger, _fileReader, _invoiceDecorator, _packingListDecorator, _cncReleaseDecoratorFactory, _jobSummaryDecorator, _getCustomerByIdAsync, _getVendorByIdAsync, _emailService);
    }

    [Fact]
    public void ReplaceTokensInDirectory_ReplacesCustomerTokenWithCustomerName() {
        // Arrange
        var customerName = "Customer Name";
        var outputDir = "C:/Output/{customer}/Sub Directory";

        // Act
        var result = _sut.ReplaceTokensInDirectory(customerName, outputDir);

        // Assert
        result.Should().Be("C:/Output/Customer Name/Sub Directory");
    }

    [Fact]
    public void ReplaceTokensInDirectory_ReplacesCustomerTokenWithSanitizedCustomerName() {
        // Arrange
        var customerName = "Customer/Name\\WithInvalid/Characters";
        var outputDir = "C:/Output/{customer}";

        // Act
        var result = _sut.ReplaceTokensInDirectory(customerName, outputDir);

        // Assert
        result.Should().Be("C:/Output/Customer_Name_WithInvalid_Characters");
    }

    [Fact]
    public void ReplaceTokensInDirectory_ReturnsOriginalOutputDir_IfCustomerTokenNotPresent() {
        // Arrange
        var customerName = "Customer Name";
        var outputDir = "C:/Output/";

        // Act
        var result = _sut.ReplaceTokensInDirectory(customerName, outputDir);

        // Assert
        result.Should().Be(outputDir);
    }
}