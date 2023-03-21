using ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.ReleasePDF;
using ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.Invoice;
using ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.JobSummary;
using ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.PackingList;
using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Features.Shared.Services;
using ApplicationCore.Infrastructure.Data;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MoreLinq;
using QuestPDF.Fluent;

namespace ApplicationCore.Features.Orders.Details.OrderRelease;

internal class ReleaseService {

    public Action<string>? OnProgressReport;
    public Action<string>? OnError;
    public Action<string>? OnActionComplete;

    private readonly IFileReader _fileReader;
    private readonly InvoiceDecorator _invoiceDecorator;
    private readonly PackingListDecorator _packingListDecorator;
    private readonly CNCReleaseDecorator _cncReleaseDecorator;
    private readonly JobSummaryDecorator _jobSummaryDecorator;

    public ReleaseService(IFileReader fileReader, InvoiceDecorator invoiceDecorator, PackingListDecorator packingListDecorator, CNCReleaseDecorator cncReleaseDecorator, JobSummaryDecorator jobSummaryDecorator) {
        _fileReader = fileReader;
        _invoiceDecorator = invoiceDecorator;
        _packingListDecorator = packingListDecorator;
        _cncReleaseDecorator = cncReleaseDecorator;
        _jobSummaryDecorator = jobSummaryDecorator;
    }

    public async Task Release(Order order, ReleaseConfiguration configuration) {

        if (configuration.ReleaseOutputDirectory is null) {
            OnError?.Invoke("No output directory set");
            return;
        }

        await CreateReleasePDF(order, configuration);

        await Invoicing(order, configuration);

        OnActionComplete?.Invoke("Release Complete");

    }

    private async Task CreateReleasePDF(Order order, ReleaseConfiguration configuration) {

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

        var directories = (configuration.ReleaseOutputDirectory ?? "").Split(';');

        var filePaths = GeneratePDF(directories, order, decorators, "Release");

        if (configuration.SendReleaseEmail && configuration.ReleaseEmailRecipients is string recipients) {
            OnProgressReport?.Invoke("Sending release email");
            await SendEmailAsync(recipients, $"RELEASED: {order.Number} {order.Name}", "Please see attached release", new string[] { filePaths.First() }, configuration);
        }

    }

    private async Task Invoicing(Order order, ReleaseConfiguration configuration) {

        if (!configuration.GenerateInvoice && !configuration.SendInvoiceEmail) {
            OnProgressReport?.Invoke("Not generating invoice pdf, because option was not enabled");
            return;
        }

        string[] invoiceDirectories = configuration.GenerateInvoice ? (configuration.InvoiceOutputDirectory ?? "").Split(';') : new string[] { Path.GetTempPath() };

        var filePaths = GeneratePDF(invoiceDirectories, order, new IDocumentDecorator[] { _invoiceDecorator }, "Invoice");

        if (configuration.SendInvoiceEmail && configuration.InvoiceEmailRecipients is string recipients) {
            OnProgressReport?.Invoke("Sending invoice email");
            await SendEmailAsync(recipients, $"INVOICE: {order.Number} {order.Name}", "Please see attached invoice", new string[] { filePaths.First() }, configuration);
        }

        if (!configuration.GenerateInvoice) {
            File.Delete(filePaths.First());
        }

    }

    private async Task SendEmailAsync(string recipients, string subject, string body, IEnumerable<string> attachments, ReleaseConfiguration configuration) {

        var message = new MimeMessage();

        recipients.Split(';').Where(s => !string.IsNullOrWhiteSpace(s)).ForEach(r => message.To.Add(new MailboxAddress(r, r)));

        if (message.To.Count == 0) {
            OnError?.Invoke("No email recipients specified");
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
        OnProgressReport?.Invoke($"Email sent with response '{response}'");
        await client.DisconnectAsync(true);

    }

    private IEnumerable<string> GeneratePDF(IEnumerable<string> outputDirs, Order order, IEnumerable<IDocumentDecorator> decorators, string name) {
        
        Document document = Document.Create(doc => {

            foreach (var decorator in decorators) {
                decorator.Decorate(order, doc);
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
            OnProgressReport?.Invoke($"File generated {filePath}");
        }

        return files;

    }

}
