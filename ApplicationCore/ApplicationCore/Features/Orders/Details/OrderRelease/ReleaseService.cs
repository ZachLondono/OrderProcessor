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

internal class ReleaseService {

    public Action<string>? OnProgressReport;
    public Action<string>? OnFileGenerated;
    public Action<string>? OnError;
    public Action<string>? OnActionComplete;

    private readonly ILogger<ReleaseService> _logger;
    private readonly IFileReader _fileReader;
    private readonly InvoiceDecorator _invoiceDecorator;
    private readonly PackingListDecorator _packingListDecorator;
    private readonly CNCReleaseDecorator _cncReleaseDecorator;
    private readonly JobSummaryDecorator _jobSummaryDecorator;

    public ReleaseService(ILogger<ReleaseService> logger, IFileReader fileReader, InvoiceDecorator invoiceDecorator, PackingListDecorator packingListDecorator, CNCReleaseDecorator cncReleaseDecorator, JobSummaryDecorator jobSummaryDecorator) {
        _fileReader = fileReader;
        _invoiceDecorator = invoiceDecorator;
        _packingListDecorator = packingListDecorator;
        _cncReleaseDecorator = cncReleaseDecorator;
        _jobSummaryDecorator = jobSummaryDecorator;
        _logger = logger;
    }

    public async Task Release(Order order, ReleaseConfiguration configuration) {

        await CreateReleasePDF(order, configuration);

        await Invoicing(order, configuration);

        OnActionComplete?.Invoke("Release Complete");

    }

    private async Task CreateReleasePDF(Order order, ReleaseConfiguration configuration) {

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

        if (configuration.GenerateCNCRelease && configuration.CNCDataFilePath is string filePath) {
            _cncReleaseDecorator.ReportFilePath = filePath;
            decorators.Add(_cncReleaseDecorator);
        }

        var directories = (configuration.ReleaseOutputDirectory ?? "").Split(';').Where(s => !string.IsNullOrWhiteSpace(s));

        if (!directories.Any()) {
            OnError?.Invoke("No output directory was specified for release pdf");
            return;
        }

        IEnumerable<string> filePaths = Enumerable.Empty<string>();
        try { 
            filePaths = GeneratePDF(directories, order, decorators, "Release");
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

    private async Task Invoicing(Order order, ReleaseConfiguration configuration) {

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
            filePaths = GeneratePDF(invoiceDirectories, order, new IDocumentDecorator[] { _invoiceDecorator }, "Invoice", isTemp);
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

    private IEnumerable<string> GeneratePDF(IEnumerable<string> outputDirs, Order order, IEnumerable<IDocumentDecorator> decorators, string name, bool isTemp = false) {
        
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
            
            string directory = Path.Combine(outputDir, _fileReader.RemoveInvalidPathCharacters($"{order.Number} {order.Name}"));

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

}
