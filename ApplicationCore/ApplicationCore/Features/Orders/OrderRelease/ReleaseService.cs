using ApplicationCore.Features.CNC.Contracts;
using ApplicationCore.Features.CNC.ReleaseEmail;
using ApplicationCore.Features.Companies.Contracts;
using ApplicationCore.Features.Orders.OrderRelease.Handlers.CNC;
using ApplicationCore.Features.Orders.OrderRelease.Handlers.Invoice;
using ApplicationCore.Features.Orders.OrderRelease.Handlers.JobSummary;
using ApplicationCore.Features.Orders.OrderRelease.Handlers.PackingList;
using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Shared;
using ApplicationCore.Shared.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Office.Interop.Outlook;
using MimeKit;
using QuestPDF.Fluent;
using System.Runtime.InteropServices;
using UglyToad.PdfPig.Writer;
using OutlookApp = Microsoft.Office.Interop.Outlook.Application;
using Exception = System.Exception;

namespace ApplicationCore.Features.Orders.OrderRelease;

public class ReleaseService {

    public Action<string>? OnProgressReport;
    public Action<string>? OnFileGenerated;
    public Action<string>? OnError;
    public Action<string>? OnActionComplete;

    private readonly ILogger<ReleaseService> _logger;
    private readonly IFileReader _fileReader;
    private readonly InvoiceDecoratorFactory _invoiceDecoratorFactory;
    private readonly PackingListDecoratorFactory _packingListDecoratorFactory;
    private readonly CNCReleaseDecoratorFactory _cncReleaseDecoratorFactory;
    private readonly JobSummaryDecoratorFactory _jobSummaryDecoratorFactory;
    private readonly CompanyDirectory.GetCustomerByIdAsync _getCustomerByIdAsync;
    private readonly CompanyDirectory.GetVendorByIdAsync _getVendorByIdAsync;
    private readonly IEmailService _emailService;
    private readonly ReleaseEmailBodyGenerator _emailBodyGenerator;

    public ReleaseService(ILogger<ReleaseService> logger, IFileReader fileReader, InvoiceDecoratorFactory invoiceDecoratorFactory, PackingListDecoratorFactory packingListDecoratorFactory, CNCReleaseDecoratorFactory cncReleaseDecoratorFactory, JobSummaryDecoratorFactory jobSummaryDecoratorFactory, CompanyDirectory.GetCustomerByIdAsync getCustomerByIdAsync, CompanyDirectory.GetVendorByIdAsync getVendorByIdAsync, IEmailService emailService, ReleaseEmailBodyGenerator emailBodyGenerator) {
        _fileReader = fileReader;
        _invoiceDecoratorFactory = invoiceDecoratorFactory;
        _packingListDecoratorFactory = packingListDecoratorFactory;
        _cncReleaseDecoratorFactory = cncReleaseDecoratorFactory;
        _jobSummaryDecoratorFactory = jobSummaryDecoratorFactory;
        _logger = logger;
        _getCustomerByIdAsync = getCustomerByIdAsync;
        _getVendorByIdAsync = getVendorByIdAsync;
        _emailService = emailService;
        _emailBodyGenerator = emailBodyGenerator;
    }

    public async Task Release(List<Order> orders, ReleaseConfiguration configuration) {

        if (orders.Count == 0) {
            throw new InvalidOperationException("No orders selected to include in release");
        }

        // TODO: check that all orders have the same customer & vendor, if not list all of them separated by a comma
        var customerName = await GetCustomerName(orders.First().CustomerId);
        var vendorName = await GetVendorName(orders.First().VendorId);
        var orderDate = orders.First().OrderDate;
        var dueDate = orders.First().DueDate;

        await CreateReleasePDF(orders, configuration, orderDate, dueDate, customerName, vendorName);

        foreach (var order in orders) {
            await Invoicing(order, configuration, customerName);
        }

        OnActionComplete?.Invoke("Release Complete");

    }

    private async Task CreateReleasePDF(List<Order> orders, ReleaseConfiguration configuration, DateTime orderDate, DateTime? dueDate, string customerName, string vendorName) {

        if (configuration.ReleaseOutputDirectory is null) {
            OnError?.Invoke("No output directory set");
            return;
        }

        if (!configuration.GeneratePackingList && !configuration.GenerateJobSummary && !configuration.GenerateCNCRelease && !configuration.IncludeInvoiceInRelease) {
            OnProgressReport?.Invoke("Not generating release pdf, because options where not enabled");
            return;
        }

        List<IDocumentDecorator> decorators = new();

        List<ReleasedJob> releases = new();
        List<ICNCReleaseDecorator> cncReleaseDecorators = new();
        if (configuration.GenerateCNCRelease) {
            foreach (var filePath in configuration.CNCDataFilePaths) {

                if (Path.GetExtension(filePath) != ".xml") {
                    OnError?.Invoke("CADCode report file is an invalid file type");
                    continue;
                }

                var (decorator, jobData) = await _cncReleaseDecoratorFactory.Create(filePath, orderDate, dueDate, customerName, vendorName);
                if (jobData is not null) {
                    releases.Add(jobData);
                }
                cncReleaseDecorators.Add(decorator);

                if (configuration.CopyCNCReportToWorkingDirectory) {
                    foreach (var order in orders) {
                        CopyReportToWorkingDirectory(order.WorkingDirectory, filePath);
                    }
                }

            }
        }

        foreach (var order in orders) {
            if (configuration.GenerateJobSummary) {
                string[] materials = releases.SelectMany(r => r.Releases)
                                            .SelectMany(r => r.Programs)
                                            .Select(p => p.Material.Name)
                                            .Distinct()
                                            .ToArray();

                var decorator = await _jobSummaryDecoratorFactory.CreateDecorator(order, configuration.IncludeProductTablesInSummary, configuration.SupplyOptions, configuration.IncludeInvoiceSummary, materials, true);
                decorators.Add(decorator);
            }

            if (configuration.GeneratePackingList) {
                var decorator = await _packingListDecoratorFactory.CreateDecorator(order);
                decorators.Add(decorator);
            }

            if (configuration.IncludeInvoiceInRelease) {
                var decorator = await _invoiceDecoratorFactory.CreateDecorator(order);
                decorators.Add(decorator);
            }
        }

        decorators.AddRange(cncReleaseDecorators);

        var directories = (configuration.ReleaseOutputDirectory ?? "").Split(';').Where(s => !string.IsNullOrWhiteSpace(s));

        if (!directories.Any()) {
            OnError?.Invoke("No output directory was specified for release pdf");
            return;
        }

        string orderNumbers = string.Join(", ", orders.Select(o => o.Number));

        var filename = configuration.ReleaseFileName ?? $"{orderNumbers} RELEASE";

        IEnumerable<string> filePaths = Enumerable.Empty<string>();
        try {
            filePaths = await GeneratePDFAsync(directories, decorators, filename, customerName, configuration.AttachAdditionalFiles ? configuration.AdditionalFilePaths : Enumerable.Empty<string>());
        } catch (Exception ex) {
            OnError?.Invoke($"Could not generate release PDF - '{ex.Message}'");
            _logger.LogError(ex, "Exception thrown while trying to generate release pdf");
        }

        if (filePaths.Any() && (configuration.SendReleaseEmail || configuration.PreviewReleaseEmail) && configuration.ReleaseEmailRecipients is string recipients) {
            OnProgressReport?.Invoke("Sending release email");
            try {

                bool multipleOrders = orders.Count > 1;
                string orderNotes = string.Join(
                                        ';',
                                        orders.Where(o => !string.IsNullOrEmpty(o.Note))
                                                .Select(o => $"{(multipleOrders ? $"{o.Number}:" : "")}{o.Note}")
                                    );
                var body = GenerateEmailBodies(configuration.IncludeMaterialSummaryInEmailBody, releases, orderNotes);

                List<string> attachments = new() { filePaths.First() };
                if (configuration.AttachAdditionalFiles) {
                    attachments.AddRange(configuration.AdditionalFilePaths);
                }

                if (configuration.PreviewReleaseEmail) {
                    CreateAndDisplayOutlookEmail(recipients, $"RELEASED: {orderNumbers} {customerName}", body.HTMLBody, body.TextBody, attachments);
                } else {
                    await SendEmailAsync(recipients, $"RELEASED: {orderNumbers} {customerName}", body.HTMLBody, body.TextBody, attachments);
                }

            } catch (Exception ex) {
                OnError?.Invoke($"Could not send email - '{ex.Message}'");
                _logger.LogError(ex, "Exception thrown while trying to send release email");
            }
        } else {
            OnProgressReport?.Invoke("Not sending release email");
        }

    }

    private (string HTMLBody, string TextBody) GenerateEmailBodies(bool includeReleaseSummary, List<ReleasedJob> jobs, string note) {

        var releasedJobs = jobs.Where(j => j.Releases.Any())
                                .Select(job => {

                                    var usedMaterials = job.Releases
                                                            .First()
                                                            .Programs
                                                            .Select(p => p.Material)
                                                            .GroupBy(m => (m.Name, m.Width, m.Length, m.Thickness, m.IsGrained))
                                                            .Select(g => new UsedMaterial(g.Count(), g.Key.Name, g.Key.Width, g.Key.Length, g.Key.Thickness));

                                    return new Job(job.JobName, usedMaterials);

                                });

        var model = new Model(releasedJobs, note);

        var htmlBody = _emailBodyGenerator.GenerateHTMLReleaseEmailBody(model, includeReleaseSummary);
        var textBody = _emailBodyGenerator.GenerateHTMLReleaseEmailBody(model, includeReleaseSummary);

        return (htmlBody, textBody);

    }

    private void CopyReportToWorkingDirectory(string workingDirectory, string filePath) {
        string fileName = Path.GetFileNameWithoutExtension(filePath);
        string destFileName = _fileReader.GetAvailableFileName(workingDirectory, fileName + " (WSML)", "xml");
        File.Copy(filePath, destFileName);
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

        if (!invoiceDirectories.Any()) {
            OnError?.Invoke("No output directory was specified for invoice pdf");
            return;
        }

        var filename = configuration.InvoiceFileName ?? $"{order.Number} INVOICE";

        IEnumerable<string> filePaths = Enumerable.Empty<string>();
        try {
            var decorator = await _invoiceDecoratorFactory.CreateDecorator(order);
            filePaths = await GeneratePDFAsync(invoiceDirectories, new IDocumentDecorator[] { decorator }, filename, customerName, Enumerable.Empty<string>(), isTemp);
        } catch (Exception ex) {
            OnError?.Invoke($"Could not generate invoice PDF - '{ex.Message}'");
            _logger.LogError(ex, "Exception thrown while trying to generate invoice pdf");
        }

        if (filePaths.Any() && (configuration.SendInvoiceEmail) && configuration.InvoiceEmailRecipients is string recipients) {
            OnProgressReport?.Invoke("Sending invoice email");
            try {
                if (configuration.PreviewInvoiceEmail) {
                    CreateAndDisplayOutlookEmail(recipients, $"INVOICE: {order.Number} {customerName}", "Please see attached invoice", "Please see attached invoice", new string[] { filePaths.First() });
                } else {
                    await SendEmailAsync(recipients, $"INVOICE: {order.Number} {customerName}", "Please see attached invoice", "Please see attached invoice", new string[] { filePaths.First() });
                }
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

    private async Task SendEmailAsync(string recipients, string subject, string htmlBody, string textBody, IEnumerable<string> attachments) {

        var message = new MimeMessage();

        recipients.Split(';')
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ForEach(r => message.To.Add(new MailboxAddress(r, r)));

        if (message.To.Count == 0) {
            OnError?.Invoke("No email recipients specified");
            return;
        }

        var sender = _emailService.GetSender();
        message.From.Add(sender);
        message.Subject = subject;

        var builder = new BodyBuilder {
            TextBody = textBody,
            HtmlBody = htmlBody
        };
        attachments.Where(_fileReader.DoesFileExist).ForEach(att => builder.Attachments.Add(att));

        message.Body = builder.ToMessageBody();

        var response = await _emailService.SendMessageAsync(message);
        _logger.LogInformation("Response from email client - '{Response}'", response);
        OnActionComplete?.Invoke("Email sent");

    }

    private void CreateAndDisplayOutlookEmail(string recipients, string subject, string htmlBody, string textBody, IEnumerable<string> attachments) {

        var app = new OutlookApp();
        MailItem mailItem = (MailItem)app.CreateItem(OlItemType.olMailItem);
        mailItem.To = recipients;
        mailItem.Subject = subject;
        mailItem.Body = textBody;
        mailItem.HTMLBody = htmlBody;

        attachments.Where(_fileReader.DoesFileExist).ForEach(att => mailItem.Attachments.Add(att));

        var senderMailBox = _emailService.GetSender();
        var sender = GetSenderOutlookAccount(app, senderMailBox.Address);

        if (sender is not null) {
            mailItem.SendUsingAccount = sender;
        } else {
            _logger.LogWarning("No outlook sender was found");
        }

        mailItem.Display();

        Marshal.ReleaseComObject(app);
        Marshal.ReleaseComObject(mailItem);
        if (sender is not null) Marshal.ReleaseComObject(sender);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        GC.WaitForPendingFinalizers();

    }

    static Account? GetSenderOutlookAccount(OutlookApp app, string preferredEmail) {

        var accounts = app.Session.Accounts;
        if (accounts is null || accounts.Count == 0) {
            return null;
        }

        Account? sender = null;
        foreach (Account account in accounts) {
            sender ??= account;
            if (account.SmtpAddress == preferredEmail) {
                sender = account;
                break;
            }
        }

        return sender;

    }

    private async Task<IEnumerable<string>> GeneratePDFAsync(IEnumerable<string> outputDirs, IEnumerable<IDocumentDecorator> decorators, string name, string customerName, IEnumerable<string> attachedFiles, bool isTemp = false) {

        if (!decorators.Any()) {
            OnError?.Invoke($"There are no pages to add to the '{name}' document");
            return Enumerable.Empty<string>();
        }

        Document document = Document.Create(doc => {

            foreach (var decorator in decorators) {
                try {
                    decorator.Decorate(doc);
                } catch (Exception ex) {
                    OnError?.Invoke($"Error adding pages to document '{name}' - '{ex.Message}'");
                }
            }

        });

        List<string> files = new();

        var documentBytes = document.GeneratePdf();
        if (attachedFiles.Any()) {

            List<byte[]> documents = new() {
                documentBytes
            };

            foreach (var file in attachedFiles) {
                documents.Add(await File.ReadAllBytesAsync(file));
            }

            documentBytes = PdfMerger.Merge(documents);

        }

        foreach (var outputDir in outputDirs) {

            string directory = ReplaceTokensInDirectory(customerName, outputDir);

            try {

                if (!Directory.Exists(directory)) {
                    Directory.CreateDirectory(directory);
                }

                var filePath = _fileReader.GetAvailableFileName(directory, name, ".pdf");
                await File.WriteAllBytesAsync(filePath, documentBytes);
                files.Add(filePath);

                if (!isTemp) {
                    OnFileGenerated?.Invoke(Path.GetFullPath(filePath));
                }

            } catch (Exception ex) {

                OnError?.Invoke($"Exception throw while trying to write file to directory '{directory}' - {ex.Message}");

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

            _logger.LogError(ex, "Exception thrown while getting customer name");
            return string.Empty;

        }

    }

    private async Task<string> GetVendorName(Guid vendorId) {

        try {

            var vendor = await _getVendorByIdAsync(vendorId);

            if (vendor is null) {
                return string.Empty;
            }

            return vendor.Name;

        } catch (Exception ex) {

            _logger.LogError(ex, "Exception thrown while getting vendor name");
            return string.Empty;

        }

    }

}
