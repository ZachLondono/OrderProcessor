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
using ApplicationCore.Shared.CNC.WorkOrderReleaseEmail;
using ApplicationCore.Shared.CNC.WSXML;
using ApplicationCore.Shared.CNC.WSXML.Report;
using ApplicationCore.Features.Orders.Shared.Domain.Products.DrawerBoxes;
using ApplicationCore.Features.Orders.OrderRelease.Handlers.DovetailDBPackingList;
using ApplicationCore.Features.Orders.OrderRelease.Handlers.FivePieceDoorCutList;
using ApplicationCore.Features.Orders.Shared.Domain.Products.Doors;
using ApplicationCore.Features.Orders.OrderRelease.Handlers.DoweledDrawerBoxCutList;
using ApplicationCore.Features.Orders.Shared.Domain.Components;
using ApplicationCore.Shared.CNC.Job;
using ApplicationCore.Features.Orders.OrderRelease.Handlers.GCode;
using CADCodeProxy.Exceptions;
using OutlookApp = Microsoft.Office.Interop.Outlook.Application;
using Exception = System.Exception;

namespace ApplicationCore.Features.Orders.OrderRelease;

public class ReleaseService {

    public System.Action? ShowProgressBar;
    public System.Action? HideProgressBar;
    public Action<int>? SetProgressBarValue;
    public Action<string>? OnProgressReport;
    public Action<string>? OnFileGenerated;
    public Action<string>? OnError;
    public Action<string>? OnActionComplete;

    private readonly ILogger<ReleaseService> _logger;
    private readonly IFileReader _fileReader;
    private readonly InvoiceDecoratorFactory _invoiceDecoratorFactory;
    private readonly PackingListDecoratorFactory _packingListDecoratorFactory;
    private readonly IDovetailDBPackingListDecoratorFactory _dovetailDBPackingListDecoratorFactory;
    private readonly CNCReleaseDecoratorFactory _cncReleaseDecoratorFactory;
    private readonly JobSummaryDecoratorFactory _jobSummaryDecoratorFactory;
    private readonly CompanyDirectory.GetCustomerByIdAsync _getCustomerByIdAsync;
    private readonly CompanyDirectory.GetVendorByIdAsync _getVendorByIdAsync;
    private readonly IEmailService _emailService;
    private readonly IWSXMLParser _wsxmlParser;
    private readonly IFivePieceDoorCutListWriter _fivePieceDoorCutListWriter;
    private readonly IDoweledDrawerBoxCutListWriter _doweledDrawerBoxCutListWriter;
    private readonly CNCPartGCodeGenerator _gcodeGenerator;

    public ReleaseService(ILogger<ReleaseService> logger, IFileReader fileReader, InvoiceDecoratorFactory invoiceDecoratorFactory, PackingListDecoratorFactory packingListDecoratorFactory, CNCReleaseDecoratorFactory cncReleaseDecoratorFactory, JobSummaryDecoratorFactory jobSummaryDecoratorFactory, CompanyDirectory.GetCustomerByIdAsync getCustomerByIdAsync, CompanyDirectory.GetVendorByIdAsync getVendorByIdAsync, IEmailService emailService, IWSXMLParser wsxmlParser, IDovetailDBPackingListDecoratorFactory dovetailDBPackingListDecoratorFactory, IFivePieceDoorCutListWriter fivePieceDoorCutListWriter, IDoweledDrawerBoxCutListWriter doweledDrawerBoxCutListWriter, CNCPartGCodeGenerator gcodeGenerator) {
        _fileReader = fileReader;
        _invoiceDecoratorFactory = invoiceDecoratorFactory;
        _packingListDecoratorFactory = packingListDecoratorFactory;
        _cncReleaseDecoratorFactory = cncReleaseDecoratorFactory;
        _jobSummaryDecoratorFactory = jobSummaryDecoratorFactory;
        _logger = logger;
        _getCustomerByIdAsync = getCustomerByIdAsync;
        _getVendorByIdAsync = getVendorByIdAsync;
        _emailService = emailService;
        _wsxmlParser = wsxmlParser;
        _dovetailDBPackingListDecoratorFactory = dovetailDBPackingListDecoratorFactory;
        _fivePieceDoorCutListWriter = fivePieceDoorCutListWriter;
        _doweledDrawerBoxCutListWriter = doweledDrawerBoxCutListWriter;
        _gcodeGenerator = gcodeGenerator;

        _fivePieceDoorCutListWriter.OnError += (msg) => OnError?.Invoke(msg);
        _doweledDrawerBoxCutListWriter.OnError += (msg) => OnError?.Invoke(msg);

        _gcodeGenerator.OnError += (msg) => OnError?.Invoke(msg);
        _gcodeGenerator.OnProgressReport += (msg) => OnProgressReport?.Invoke(msg);
        _gcodeGenerator.ShowProgressBar += () => ShowProgressBar?.Invoke();
        _gcodeGenerator.HideProgressBar += () => HideProgressBar?.Invoke();
        _gcodeGenerator.SetProgressBarValue += (val) => SetProgressBarValue?.Invoke(val);
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

        await OrchestrateRelease(orders, configuration, orderDate, dueDate, customerName, vendorName);

        foreach (var order in orders) {
            await Invoicing(order, configuration, customerName);
        }

        OnActionComplete?.Invoke("Release Complete");

    }

    private async Task OrchestrateRelease(List<Order> orders, ReleaseConfiguration configuration, DateTime orderDate, DateTime? dueDate, string customerName, string vendorName) {

        if (configuration.ReleaseOutputDirectory is null) {
            OnError?.Invoke("No output directory set");
            return;
        }

        if (!configuration.GeneratePackingList && !configuration.GenerateJobSummary && !configuration.GenerateCNCRelease && !configuration.IncludeInvoiceInRelease) {
            OnProgressReport?.Invoke("Not generating release pdf, because options where not enabled");
            return;
        }

        var directories = (configuration.ReleaseOutputDirectory ?? "").Split(';').Where(s => !string.IsNullOrWhiteSpace(s));

        if (!directories.Any()) {
            OnError?.Invoke("No output directory was specified for release pdf");
            return;
        }

        string orderNumbers = string.Join(", ", orders.Select(o => o.Number));

        var filename = configuration.ReleaseFileName ?? $"{orderNumbers} RELEASE";

        var releases = await GetCNCReleases(orders, configuration, orderDate, dueDate, customerName, vendorName);

        var decorators = await CreateDocumentDecorators(orders, configuration, releases);

        var additionalPDFs = new List<string>(configuration.AdditionalFilePaths);
        var cutLists = await CreateCutLists(orders, configuration, customerName, vendorName);
        additionalPDFs.AddRange(cutLists);
        
        OnProgressReport?.Invoke("Generating release PDF");
        try {

            var documentData = await BuildPDFAsync(decorators, additionalPDFs);
            var filePaths = await SaveFileDataToDirectoriesAsync(documentData, directories, customerName, filename, isTemp:false);

            if (filePaths.Any() && (configuration.SendReleaseEmail || configuration.PreviewReleaseEmail) && configuration.ReleaseEmailRecipients is string recipients) {
                await SendReleaseEmail(orders, configuration, customerName, releases, orderNumbers, filePaths, recipients);
            } else {
                OnProgressReport?.Invoke("Not sending release email");
            }

        } catch (Exception ex) {
            OnError?.Invoke($"Could not generate release PDF - '{ex.Message}'");
            _logger.LogError(ex, "Exception thrown while trying to generate release pdf");
        }

    }

    private async Task<List<string>> CreateCutLists(List<Order> orders, ReleaseConfiguration configuration, string customerName, string vendorName) {
        List<string> cutLists = new();
        if (configuration.Generate5PieceCutList) {
            var fivePieceCutLists = await Generate5PieceCutLists(customerName, vendorName, orders);
            cutLists.AddRange(fivePieceCutLists);
        }

        if (configuration.GenerateDoweledDrawerBoxCutList) {
            var doweledDBCutLists = await GenerateDoweledDrawerBoxCutLists(customerName, vendorName, orders);
            cutLists.AddRange(doweledDBCutLists);
        }

        return cutLists;
    }

    private async Task<List<IDocumentDecorator>> CreateDocumentDecorators(List<Order> orders, ReleaseConfiguration configuration, List<ReleasedJob> releases) {
        List<IDocumentDecorator> decorators = new();
        if (configuration.GenerateJobSummary) {
            var jobSummaryDecorators = await CreateJobSummaryDecorators(orders, configuration, releases);
            decorators.AddRange(jobSummaryDecorators);
        }

        if (configuration.GeneratePackingList) {
            var packingListDecorators = await CreatePackingListDecorators(orders);
            decorators.AddRange(packingListDecorators);
        }

        if (configuration.IncludeDovetailDBPackingList) {
            var dovetailDBPackingListDecorators = await CreateDovetailDBPackingListDecorators(orders);
            decorators.AddRange(dovetailDBPackingListDecorators);
        }

        if (configuration.IncludeInvoiceInRelease) {
            var invoiceDecorators = await CreateInvoiceDecorators(orders);
            decorators.AddRange(invoiceDecorators);
        }

        decorators.AddRange(releases.Select(_cncReleaseDecoratorFactory.Create).ToList());
        return decorators;
    }

    private async Task<List<IDocumentDecorator>> CreateDovetailDBPackingListDecorators(List<Order> orders) {
        List<IDocumentDecorator> dovetailDBPackingListDecorators = new();
        foreach (var order in orders) {
            if (!order.Products.Any(p => p is DovetailDrawerBoxProduct)) continue;
            var dovetailDecorator = await _dovetailDBPackingListDecoratorFactory.CreateDecorator(order);
            dovetailDBPackingListDecorators.Add(dovetailDecorator);
        }

        return dovetailDBPackingListDecorators;
    }

    private async Task<List<IDocumentDecorator>> CreatePackingListDecorators(List<Order> orders) {
        List<IDocumentDecorator> packingListDecorators = new();
        foreach (var order in orders) {
            var decorator = await _packingListDecoratorFactory.CreateDecorator(order);
            packingListDecorators.Add(decorator);
        }

        return packingListDecorators;
    }

    private async Task<List<IDocumentDecorator>> CreateJobSummaryDecorators(List<Order> orders, ReleaseConfiguration configuration, List<ReleasedJob> releases) {
        List<IDocumentDecorator> jobSummaryDecorators = new();
        foreach (var order in orders) {
            string[] materials = releases.SelectMany(r => r.Releases)
                                        .SelectMany(r => r.Programs)
                                        .Select(p => p.Material.Name)
                                        .Distinct()
                                        .ToArray();

            var decorator = await _jobSummaryDecoratorFactory.CreateDecorator(order, configuration.IncludeProductTablesInSummary, configuration.SupplyOptions, materials, true);
            jobSummaryDecorators.Add(decorator);
        }

        return jobSummaryDecorators;
    }

    private async Task SendReleaseEmail(List<Order> orders, ReleaseConfiguration configuration, string customerName, List<ReleasedJob> releases, string orderNumbers, IEnumerable<string> filePaths, string recipients) {
        OnProgressReport?.Invoke("Sending release email");
        try {

            var (HTMLBody, TextBody) = await Task.Run(() => {
                bool multipleOrders = orders.Count > 1;
                var orderNotes = orders.Where(o => !string.IsNullOrEmpty(o.Note))
                                        .Select(o => $"{(multipleOrders ? $"{o.Number}:" : "")}{o.Note}");
                string note = string.Join(';', orderNotes);
                return GenerateEmailBodies(configuration.IncludeMaterialSummaryInEmailBody, releases, note);
            });

            List<string> attachments = new() { filePaths.First() };
            if (configuration.AttachAdditionalFiles) {
                attachments.AddRange(configuration.AdditionalFilePaths);
            }

            if (configuration.PreviewReleaseEmail) {
                await Task.Run(() => CreateAndDisplayOutlookEmail(recipients, $"RELEASED: {orderNumbers} {customerName}", HTMLBody, TextBody, attachments));
            } else {
                await SendEmailAsync(recipients, $"RELEASED: {orderNumbers} {customerName}", HTMLBody, TextBody, attachments);
            }

        } catch (Exception ex) {
            OnError?.Invoke($"Could not send email - '{ex.Message}'");
            _logger.LogError(ex, "Exception thrown while trying to send release email");
        }
    }

    private async Task<List<IDocumentDecorator>> CreateInvoiceDecorators(List<Order> orders) {
        List<IDocumentDecorator> invoiceDecorators = new();
        foreach (var order in orders) {
            var decorator = await _invoiceDecoratorFactory.CreateDecorator(order);
            invoiceDecorators.Add(decorator);
        }

        return invoiceDecorators;
    }

    private async Task<List<ReleasedJob>> GetCNCReleases(List<Order> orders, ReleaseConfiguration configuration, DateTime orderDate, DateTime? dueDate, string customerName, string vendorName) {
        List<ReleasedJob> releases = new();

        if (configuration.GenerateCNCRelease) {
            var wsxmlReleasedJobs = await GetWSXMLReleasedJobs(orders, configuration.CNCDataFilePaths, configuration.CopyCNCReportToWorkingDirectory, orderDate, dueDate, customerName, vendorName);
            releases.AddRange(wsxmlReleasedJobs);
        }

        if (configuration.GenerateCNCGCode) {
            var gcodeJob = await GenerateGCode(orders, customerName, vendorName);
            if (gcodeJob is not null) {
                releases.Add(gcodeJob);
            }
        }

        return releases;
    }

    private async Task<List<ReleasedJob>> GetWSXMLReleasedJobs(List<Order> orders, List<string> wsxmlFiles, bool copyToWorkingDirectory, DateTime orderDate, DateTime? dueDate, string customerName, string vendorName) {

        List<ReleasedJob> wsxmlReleasedJobs = new();
        foreach (var filePath in wsxmlFiles) {

            if (Path.GetExtension(filePath) != ".xml") {
                OnError?.Invoke("CADCode report file is an invalid file type");
                continue;
            }

            ReleasedJob? jobData = null;
            WSXMLReport? report = await Task.Run(() => WSXMLParser.ParseWSXMLReport(filePath));
            if (report is not null) {
                jobData = _wsxmlParser.MapDataToReleasedJob(report, orderDate, dueDate, customerName, vendorName);
            }

            if (jobData is null) {
                continue;
            }

            wsxmlReleasedJobs.Add(jobData);

            if (!copyToWorkingDirectory) {
                continue;
            }

            foreach (var order in orders) {
                if (!Directory.Exists(order.WorkingDirectory)) {
                    OnError?.Invoke($"Can not copy WSXML file to order working directory because it can ot be accessed or does not exist - '{order.WorkingDirectory}'");
                    continue;
                }
                await Task.Run(() => CopyReportToWorkingDirectory(order.WorkingDirectory, filePath));
            }

        }

        OnActionComplete?.Invoke($"'{wsxmlReleasedJobs.Count}' WSXML jobs loaded");

        return wsxmlReleasedJobs;
    }

    private async Task<ReleasedJob?> GenerateGCode(List<Order> orders, string customerName, string vendorName) {

        try {

            var gCodeRelease = await _gcodeGenerator.GenerateGCode(orders, customerName, vendorName);

            if (gCodeRelease is not null) {
                return gCodeRelease;
            }

        } catch (CADCodeAuthorizationException ex) {

            OnError?.Invoke($"Failed to authorize CADCode. Make sure there is an accessible and available license key. - {ex.Message}");
            _logger.LogError(ex, "CADCode could not authorize");

        } catch (CADCodeInitializationException ex) {

            OnError?.Invoke($"CADCode failed to initialize - {ex.ErrorNumber} : {ex.Message}");
            _logger.LogError(ex, "Exception thrown while initializing CADCode");

        } catch (Exception ex) {

            OnError?.Invoke($"Error while generating G-code - {ex.Message}");
            _logger.LogError(ex, "Exception thrown while generating g-code");

        }

        return null;

    }

    private async Task<List<string>> Generate5PieceCutLists(string customerName, string vendorName, IEnumerable<Order> orders) {

        List<string> generatedFiles = new();

        foreach (var order in orders) { 
            var outputDirectory = Path.Combine(order.WorkingDirectory, "CUTLIST");
            var cutListResults = await Task.Run(() =>
                order.Products
                    .OfType<FivePieceDoorProduct>()
                    .GroupBy(d => d.Material)
                    .Select(group => new FivePieceCutList() {
                        CustomerName = customerName,
                        VendorName = vendorName,
                        Note = order.Note,
                        Material = group.First().Material,
                        OrderDate = order.OrderDate,
                        OrderName = order.Name,
                        OrderNumber = order.Number,
                        TotalDoorCount = group.Sum(door => door.Qty),
                        Items = group.Select(door => (door, door.GetParts()))
                                        .SelectMany(doorParts =>
                                            doorParts.Item2.Select(part => new FivePieceDoorLineItem() {
                                                CabNumber = doorParts.door.ProductNumber,
                                                Note = "",
                                                Qty = part.Qty,
                                                PartName = part.Name,
                                                Length = part.Length.AsMillimeters(),
                                                Width = part.Length.AsMillimeters()
                                            })
                                        )
                                        .ToList()
                    })
                    .Select(cutList => _fivePieceDoorCutListWriter.WriteCutList(cutList, outputDirectory, true))
                    .OfType<FivePieceDoorCutListResult>()
                    .ToList()
            );

            cutListResults.Select(result => result.ExcelFilePath)
                            .ForEach(file => OnFileGenerated?.Invoke(file));

            generatedFiles.AddRange(cutListResults.Select(result => result.PDFFilePath).OfType<string>());

        }

        return generatedFiles;

    }

    private async Task<IEnumerable<string>> GenerateDoweledDrawerBoxCutLists(string customerName, string vendorName, IEnumerable<Order> orders) {

        List<string> generatedFiles = new();

        foreach (var order in orders) {

            var outputDirectory = Path.Combine(order.WorkingDirectory, "CUTLIST");
            var cutListResults = await Task.Run(() =>
                order.Products
                    .OfType<DoweledDrawerBoxProduct>()
                    .GroupBy(d => d.BottomMaterial)
                    .Select(group => new DoweledDrawerBoxCutList() {
                        CustomerName = customerName,
                        VendorName = vendorName,
                        Note = order.Note,
                        Material = $"{group.First().BottomMaterial.Thickness.RoundToInchMultiple(0.0625).AsInchFraction()} {group.First().BottomMaterial.Name}",
                        OrderDate = order.OrderDate,
                        OrderName = order.Name,
                        OrderNumber = order.Number,
                        TotalBoxCount = group.Sum(box => box.Qty),
                        Items = group.Select(box => box.GetBottom(DoweledDrawerBox.Construction, box.ProductNumber))
                                        .GroupBy(b => (b.Width, b.Length))
                                        .OrderByDescending(g => g.Key.Length)
                                        .OrderByDescending(g => g.Key.Width)
                                        .Select(bottomGroup => new DoweledDBCutListLineItem() {
                                            CabNumbers = string.Join(", ", bottomGroup.Select(p => p.ProductNumber)),
                                            Note = "",
                                            Qty = bottomGroup.Sum(p => p.Qty),
                                            PartName = "Bottom",
                                            Length = bottomGroup.Key.Length.AsMillimeters(),
                                            Width = bottomGroup.Key.Width.AsMillimeters()
                                        })
                                        .ToList()
                    })
                    .Select(cutList => _doweledDrawerBoxCutListWriter.WriteCutList(cutList, outputDirectory, true))
                    .OfType<DoweledDBCutListResult>()
                    .ToList()
            );

            cutListResults.Select(result => result.ExcelFilePath)
                            .ForEach(file => OnFileGenerated?.Invoke(file));

            generatedFiles.AddRange(cutListResults.Select(result => result.PDFFilePath).OfType<string>());

        }

        return generatedFiles;

    }

    private async Task Invoicing(Order order, ReleaseConfiguration configuration, string customerName) {

        if (!configuration.GenerateInvoice && !configuration.SendInvoiceEmail) {
            OnProgressReport?.Invoke("Not generating invoice pdf, because option was not enabled");
            return;
        }

        bool isTemp = !configuration.GenerateInvoice;

        IEnumerable<string> invoiceDirectories = GetInvoiceDirectories(configuration);
        if (!invoiceDirectories.Any()) {
            OnError?.Invoke("No output directory was specified for invoice pdf");
            return;
        }

        var filename = configuration.InvoiceFileName ?? $"{order.Number} INVOICE";

        IEnumerable<string> filePaths = Enumerable.Empty<string>();
        try {
            var decorator = await _invoiceDecoratorFactory.CreateDecorator(order);
            var documentBytes = await Task.Run(() => Document.Create(doc => decorator.Decorate(doc)).GeneratePdf());
            filePaths = await SaveFileDataToDirectoriesAsync(documentBytes, invoiceDirectories, customerName, filename, isTemp);
        } catch (Exception ex) {
            OnError?.Invoke($"Could not generate invoice PDF - '{ex.Message}'");
            _logger.LogError(ex, "Exception thrown while trying to generate invoice pdf");
        }

        if (filePaths.Any() && (configuration.SendInvoiceEmail) && configuration.InvoiceEmailRecipients is string recipients) {
            OnProgressReport?.Invoke("Sending invoice email");
            try {
                if (configuration.PreviewInvoiceEmail) {
                    await Task.Run(() => CreateAndDisplayOutlookEmail(recipients, $"INVOICE: {order.Number} {customerName}", "Please see attached invoice", "Please see attached invoice", new string[] { filePaths.First() }));
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

        if (isTemp) {
            filePaths.ForEach(file => {
                _logger.LogInformation("Deleting temporary invoice pdf '@File'", file);
                File.Delete(file);
            });
        }

    }

    private static IEnumerable<string> GetInvoiceDirectories(ReleaseConfiguration configuration) {
        IEnumerable<string> invoiceDirectories;
        if (configuration.GenerateInvoice) {
            invoiceDirectories = (configuration.InvoiceOutputDirectory ?? "").Split(';').Where(s => !string.IsNullOrWhiteSpace(s)).Where(Directory.Exists);
        } else {
            invoiceDirectories = new string[] { Path.GetTempPath() };
        }

        return invoiceDirectories;
    }

    private void CopyReportToWorkingDirectory(string workingDirectory, string filePath) {
        string fileName = Path.GetFileNameWithoutExtension(filePath);
        string destFileName = _fileReader.GetAvailableFileName(workingDirectory, fileName + " (WSML)", "xml");
        File.Copy(filePath, destFileName);
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

        var response = await Task.Run(() => _emailService.SendMessageAsync(message));
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

    private static Account? GetSenderOutlookAccount(OutlookApp app, string preferredEmail) {

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

    private async Task<byte[]> BuildPDFAsync(IEnumerable<IDocumentDecorator> decorators, IEnumerable<string> attachedFiles) {

        if (!decorators.Any()) {
            return Array.Empty<byte>();
        }

        var documentBytes = await Task.Run(() => {

            var document = Document.Create(doc => {

                foreach (var decorator in decorators) {
                    try {
                        decorator.Decorate(doc);
                    } catch (Exception ex) {
                        OnError?.Invoke($"Error adding pages to document - '{ex.Message}'");
                    }
                }

            });

            return document.GeneratePdf();

        });

        if (attachedFiles.Any()) {

            List<byte[]> documents = new() {
                documentBytes
            };

            foreach (var file in attachedFiles.Where(File.Exists)) {
                documents.Add(await File.ReadAllBytesAsync(file));
            }

            documentBytes = await Task.Run(() => PdfMerger.Merge(documents));

        }

        return documentBytes;

    }

    private async Task<IEnumerable<string>> SaveFileDataToDirectoriesAsync(byte[] documentBytes, IEnumerable<string> outputDirs, string customerName, string fileName, bool isTemp) {

        List<string> files = new();

        foreach (var outputDir in outputDirs) {

            string directory = ReplaceTokensInDirectory(_fileReader, customerName, outputDir);

            try {

                if (!Directory.Exists(directory)) {
                    Directory.CreateDirectory(directory);
                }

                var filePath = _fileReader.GetAvailableFileName(directory, fileName, ".pdf");
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

    private static (string HTMLBody, string TextBody) GenerateEmailBodies(bool includeReleaseSummary, List<ReleasedJob> jobs, string note) {

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

        var model = new ReleasedWorkOrderSummary(releasedJobs, note);

        var htmlBody = ReleaseEmailBodyGenerator.GenerateHTMLReleaseEmailBody(model, includeReleaseSummary);
        var textBody = ReleaseEmailBodyGenerator.GenerateTextReleaseEmailBody(model, includeReleaseSummary);

        return (htmlBody, textBody);

    }

    public static string ReplaceTokensInDirectory(IFileReader fileReader, string customerName, string outputDir) {
        var sanitizedName = fileReader.RemoveInvalidPathCharacters(customerName);
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
