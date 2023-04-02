using ApplicationCore.Features.Companies.Contracts;
using ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.ReleasePDF;
using ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.Invoice;
using ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.JobSummary;
using ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.PackingList;
using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Features.Shared.Services;
using ApplicationCore.Infrastructure.Data;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;
using MoreLinq;
using QuestPDF.Fluent;

namespace ApplicationCore.Features.Orders.Details.OrderRelease;

public class ReleaseService {

    public Action<string>? OnProgressReport;
    public Action<string>? OnFileGenerated;
    public Action<string>? OnError;
    public Action<string>? OnActionComplete;

    private readonly ILogger<ReleaseService> _logger;
    private readonly IFileReader _fileReader;
    private readonly IInvoiceDecorator _invoiceDecorator;
    private readonly IPackingListDecorator _packingListDecorator;
    private readonly CNCReleaseDecoratorFactory _cncReleaseDecoratorFactory;
    private readonly IJobSummaryDecorator _jobSummaryDecorator;
    private readonly CompanyDirectory.GetCustomerByIdAsync _getCustomerByIdAsync;

    public ReleaseService(ILogger<ReleaseService> logger, IFileReader fileReader, IInvoiceDecorator invoiceDecorator, IPackingListDecorator packingListDecorator, CNCReleaseDecoratorFactory cncReleaseDecoratorFactory, IJobSummaryDecorator jobSummaryDecorator, CompanyDirectory.GetCustomerByIdAsync getCustomerByIdAsync) {
        _fileReader = fileReader;
        _invoiceDecorator = invoiceDecorator;
        _packingListDecorator = packingListDecorator;
        _cncReleaseDecoratorFactory = cncReleaseDecoratorFactory;
        _jobSummaryDecorator = jobSummaryDecorator;
        _logger = logger;
        _getCustomerByIdAsync = getCustomerByIdAsync;
    }

    public async Task Release(Order order, ReleaseConfiguration configuration) {

        var customerName = await GetCustomerName(order.CustomerId);

        await CreateReleasePDF(order, configuration, customerName);

        await Invoicing(order, configuration, customerName);

        OnActionComplete?.Invoke("Release Complete");

    }

    private async Task CreateReleasePDF(Order order, ReleaseConfiguration configuration, string customerName) {

        if (configuration.ReleaseOutputDirectory is null) {
            OnError?.Invoke("No output directory set");
            return;
        }

        if (!configuration.GeneratePackingList && !configuration.GenerateJobSummary && !configuration.GenerateCNCRelease && !configuration.IncludeInvoiceInRelease) {
            OnProgressReport?.Invoke("Not generating release pdf, because options where not enabled");
            return;
        }

        List<IDocumentDecorator> decorators = new();

        if (configuration.GenerateJobSummary) {
            decorators.Add(_jobSummaryDecorator);
        }

        if (configuration.GeneratePackingList) {
            decorators.Add(_packingListDecorator);
        }

        if (configuration.IncludeInvoiceInRelease) {
            decorators.Add(_invoiceDecorator);
        }

        if (configuration.GenerateCNCRelease) {
            configuration.CNCDataFilePaths
                        .ForEach(filePath => {
                            var decorator = _cncReleaseDecoratorFactory.Create(filePath);
                            decorators.Add(decorator);
                        });
        }

        var directories = (configuration.ReleaseOutputDirectory ?? "").Split(';').Where(s => !string.IsNullOrWhiteSpace(s));

        if (!directories.Any()) {
            OnError?.Invoke("No output directory was specified for release pdf");
            return;
        }

        IEnumerable<string> filePaths = Enumerable.Empty<string>();
        try { 
            filePaths = GeneratePDF(directories, order, decorators, "Release", customerName);
        } catch (Exception ex) {
            OnError?.Invoke($"Could not generate release PDF - '{ex.Message}'");
            _logger.LogError(ex, "Exception thrown while trying to generate release pdf");
        }

        if (filePaths.Any() && configuration.SendReleaseEmail && configuration.ReleaseEmailRecipients is string recipients) {
            OnProgressReport?.Invoke("Sending release email");
            try { 
                await SendEmailAsync(recipients, $"RELEASED: {order.Number} {order.Name}", "Please see attached release", new string[] { filePaths.First() }, configuration);
            } catch (Exception ex) {
                OnError?.Invoke($"Could not send email - '{ex.Message}'");
                _logger.LogError(ex, "Exception thrown while trying to send release email");
            }
        } else {
            OnProgressReport?.Invoke("Not sending release email");
        }

    }

    private async Task Invoicing(Order order, ReleaseConfiguration configuration, string customerName) {

        if (!configuration.GenerateInvoice && !configuration.SendInvoiceEmail) {
            OnProgressReport?.Invoke("Not generating invoice pdf, because option was not enabled");
            return;
        }

        IEnumerable<string> invoiceDirectories;
        bool isTemp = false;
        if (configuration.GenerateInvoice) {
            invoiceDirectories = (configuration.InvoiceOutputDirectory ?? "").Split(';').Where(s => !string.IsNullOrWhiteSpace(s));
        } else {
            invoiceDirectories = new string[] { Path.GetTempPath() };
            isTemp = true;
        }

        if (!invoiceDirectories.Any() ) {
            OnError?.Invoke("No output directory was specified for invoice pdf");
            return;
        }

        IEnumerable<string> filePaths = Enumerable.Empty<string>();
        try { 
            filePaths = GeneratePDF(invoiceDirectories, order, new IDocumentDecorator[] { _invoiceDecorator }, "Invoice", customerName, isTemp);
        } catch (Exception ex) {
            OnError?.Invoke($"Could not generate invoice PDF - '{ex.Message}'");
            _logger.LogError(ex, "Exception thrown while trying to generate invoice pdf");
        }

        if (filePaths.Any() && configuration.SendInvoiceEmail && configuration.InvoiceEmailRecipients is string recipients) {
            OnProgressReport?.Invoke("Sending invoice email");
            try { 
                await SendEmailAsync(recipients, $"INVOICE: {order.Number} {order.Name}", "Please see attached invoice", new string[] { filePaths.First() }, configuration);
            } catch (Exception ex) {
                OnError?.Invoke($"Could not send invoice email - '{ex.Message}'");
                _logger.LogError(ex, "Exception thrown while trying to send invoice email");
            }
        } else {
            OnProgressReport?.Invoke("Not sending invoice email");
        }

        if (!configuration.GenerateInvoice) {
            filePaths.ForEach(file => {
                _logger.LogInformation("Deleting temporary invoice pdf '@File'", file);
                File.Delete(file);
            });
        }

    }

    private async Task SendEmailAsync(string recipients, string subject, string body, IEnumerable<string> attachments, ReleaseConfiguration configuration) {

        var message = new MimeMessage();

        recipients.Split(';').Where(s => !string.IsNullOrWhiteSpace(s)).ForEach(r => message.To.Add(new MailboxAddress(r, r)));

        if (message.To.Count == 0) {
            OnError?.Invoke("No email recipients specified");
            return;
        }

        if (string.IsNullOrWhiteSpace(configuration.EmailSenderEmail)) {
            OnError?.Invoke("No email sender is configured");
            return;
        }


        message.From.Add(new MailboxAddress(configuration.EmailSenderName, configuration.EmailSenderEmail));
        message.Subject = subject;

        var builder = new BodyBuilder {
            TextBody = body
        };
        attachments.Where(_fileReader.DoesFileExist).ForEach(att => builder.Attachments.Add(att));

        message.Body = builder.ToMessageBody();

        using var client = new SmtpClient();
        client.Connect(configuration.EmailServerHost, configuration.EmailServerPort, SecureSocketOptions.Auto);
        client.Authenticate(configuration.EmailSenderEmail, UserDataProtection.Unprotect(configuration.EmailSenderPassword));

        var response = await client.SendAsync(message);
        _logger.LogInformation("Response from email client - '@Response'", response);
        await client.DisconnectAsync(true);

    }

    private IEnumerable<string> GeneratePDF(IEnumerable<string> outputDirs, Order order, IEnumerable<IDocumentDecorator> decorators, string name, string customerName, bool isTemp = false) {
        
        if (!decorators.Any()) {
            OnError?.Invoke($"There are no pages to add to the '{name}' document");
            return Enumerable.Empty<string>();
        }

        Document document = Document.Create(doc => {

            foreach (var decorator in decorators) {
                try {
                    decorator.Decorate(order, doc);
                } catch (Exception ex) {
                    OnError?.Invoke($"Error adding pages to document '{name}' - '{ex.Message}'");
                }
            }

        });

        List<string> files = new();
    
        foreach (var outputDir in outputDirs) {

            string dir = ReplaceTokensInDirectory(customerName, outputDir);

            string directory = Path.Combine(dir, _fileReader.RemoveInvalidPathCharacters($"{order.Number} {order.Name}"));

            if (!Directory.Exists(directory)) {
                Directory.CreateDirectory(directory);
            }

            var filePath = _fileReader.GetAvailableFileName(directory, $"{order.Number} {order.Name} - {name}", ".pdf");
            document.GeneratePdf(filePath);
            files.Add(filePath);

            if (!isTemp) { 
                OnFileGenerated?.Invoke(Path.GetFullPath(filePath));
            }

        }

        return files;
    }

    public string ReplaceTokensInDirectory(string customerName, string outputDir) {
        var sanitizedName = _fileReader.RemoveInvalidPathCharacters(customerName);
        var result = outputDir.Replace("{customer}", sanitizedName);
        return result;
    }

    private async Task<string> GetCustomerName(Guid customerId) {

        try { 
            
            var customer = await _getCustomerByIdAsync(customerId);
            
            if (customer is null) {
                return string.Empty;
            }

            return customer.Name;

        } catch (Exception ex) {

            _logger.LogError(ex, "Exception thrown while getting cutomer name");
            return string.Empty;

        }

    }

}
