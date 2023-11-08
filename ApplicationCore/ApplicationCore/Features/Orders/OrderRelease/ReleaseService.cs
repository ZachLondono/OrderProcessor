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
using ApplicationCore.Shared.CNC.ReleasePDF;
using ApplicationCore.Shared.CNC.WorkOrderReleaseEmail;
using ApplicationCore.Shared.CNC.WSXML;
using ApplicationCore.Shared.CNC.WSXML.ReleasedJob;
using ApplicationCore.Shared.CNC.WSXML.Report;
using ApplicationCore.Features.Orders.Shared.Domain.Products.DrawerBoxes;
using ApplicationCore.Features.Orders.OrderRelease.Handlers.DovetailDBPackingList;
using ApplicationCore.Features.Orders.OrderRelease.Handlers.FivePieceDoorCutList;
using ApplicationCore.Features.Orders.Shared.Domain.Products.Doors;
using ApplicationCore.Features.Orders.OrderRelease.Handlers.DoweledDrawerBoxCutList;
using ApplicationCore.Features.Orders.Shared.Domain.Components;
using CADCodeProxy;
using ApplicationCore.Shared.Domain;
using CADCodeProxy.CNC;
using CADCodeProxy.Machining;
using ApplicationCore.Features.Orders.Shared.Domain;

namespace ApplicationCore.Features.Orders.OrderRelease;

public class ReleaseService {

    public System.Action ShowProgressBar;
    public System.Action HideProgressBar;
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

    public ReleaseService(ILogger<ReleaseService> logger, IFileReader fileReader, InvoiceDecoratorFactory invoiceDecoratorFactory, PackingListDecoratorFactory packingListDecoratorFactory, CNCReleaseDecoratorFactory cncReleaseDecoratorFactory, JobSummaryDecoratorFactory jobSummaryDecoratorFactory, CompanyDirectory.GetCustomerByIdAsync getCustomerByIdAsync, CompanyDirectory.GetVendorByIdAsync getVendorByIdAsync, IEmailService emailService, IWSXMLParser wsxmlParser, IDovetailDBPackingListDecoratorFactory dovetailDBPackingListDecoratorFactory, IFivePieceDoorCutListWriter fivePieceDoorCutListWriter, IDoweledDrawerBoxCutListWriter doweledDrawerBoxCutListWriter) {
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

        _fivePieceDoorCutListWriter.OnError += OnError;
        _doweledDrawerBoxCutListWriter.OnError += OnError;
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

                ReleasedJob? jobData = null;
                WSXMLReport? report = await Task.Run(() => WSXMLParser.ParseWSXMLReport(filePath));
                if (report is not null) {
                    jobData = _wsxmlParser.MapDataToReleasedJob(report, orderDate, dueDate, customerName, vendorName);
                }

                if (jobData is null) {
                    continue;
                }

                releases.Add(jobData);
                var decorator = _cncReleaseDecoratorFactory.Create(jobData);
                cncReleaseDecorators.Add(decorator);

                if (configuration.CopyCNCReportToWorkingDirectory) {
                    foreach (var order in orders) {
                        await Task.Run(() => CopyReportToWorkingDirectory(order.WorkingDirectory, filePath));
                    }
                }

            }
        }

        if (configuration.GenerateCNCGCode) {

            foreach (var order in orders) {
                var gCodeRelease = await GenerateGCode(order, customerName, vendorName);
                if (gCodeRelease is not null) {
                    releases.Add(gCodeRelease);
                    var decorator = _cncReleaseDecoratorFactory.Create(gCodeRelease);
                    cncReleaseDecorators.Add(decorator);
                }
            }
            
        }

        List<string> additionalPDFs = new(configuration.AdditionalFilePaths);

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

                if (configuration.IncludeDovetailDBPackingList && order.Products.Any(p => p is DovetailDrawerBoxProduct)) {
                    var dovetailDecorator = await _dovetailDBPackingListDecoratorFactory.CreateDecorator(order);
                    decorators.Add(dovetailDecorator);
                }
            }

            if (configuration.IncludeInvoiceInRelease) {
                var decorator = await _invoiceDecoratorFactory.CreateDecorator(order);
                decorators.Add(decorator);
            }

            if (configuration.Generate5PieceCutList) {

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
                                TotalDoorCount = group.Count(),
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

                cutListResults.ForEach(result => {
                    OnFileGenerated?.Invoke(result.ExcelFilePath);
                    if (result.PDFFilePath is string filePath)
                        additionalPDFs.Add(filePath);
                });

            }

            if (configuration.GenerateDoweledDrawerBoxCutList) {

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

                cutListResults.ForEach(result => {
                    OnFileGenerated?.Invoke(result.ExcelFilePath);
                    if (result.PDFFilePath is string filePath)
                        additionalPDFs.Add(filePath);
                });


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

        OnProgressReport?.Invoke("Generating release PDF");
        IEnumerable<string> filePaths = Enumerable.Empty<string>();
        try {
            filePaths = await GeneratePDFAsync(directories, decorators, filename, customerName, additionalPDFs);
        } catch (Exception ex) {
            OnError?.Invoke($"Could not generate release PDF - '{ex.Message}'");
            _logger.LogError(ex, "Exception thrown while trying to generate release pdf");
        }

        if (filePaths.Any() && (configuration.SendReleaseEmail || configuration.PreviewReleaseEmail) && configuration.ReleaseEmailRecipients is string recipients) {
            OnProgressReport?.Invoke("Sending release email");
            try {

                var body = await Task.Run(() => {
                    bool multipleOrders = orders.Count > 1;
                    string orderNotes = string.Join(
                                            ';',
                                            orders.Where(o => !string.IsNullOrEmpty(o.Note))
                                                    .Select(o => $"{(multipleOrders ? $"{o.Number}:" : "")}{o.Note}")
                                        );
                    return GenerateEmailBodies(configuration.IncludeMaterialSummaryInEmailBody, releases, orderNotes);
                }); 

                List<string> attachments = new() { filePaths.First() };
                if (configuration.AttachAdditionalFiles) {
                    attachments.AddRange(configuration.AdditionalFilePaths);
                }

                if (configuration.PreviewReleaseEmail) {
                    await Task.Run(() => CreateAndDisplayOutlookEmail(recipients, $"RELEASED: {orderNumbers} {customerName}", body.HTMLBody, body.TextBody, attachments));
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

    private async Task<ReleasedJob?> GenerateGCode(Order order, string customerName, string vendorName) {

        // TODO: Move ReleasedJob out of WSXML namespace

        var parts = order.Products
                        .OfType<ICNCPartContainer>()
                        .Where(p => p.ContainsCNCParts())
                        .SelectMany(p => p.GetCNCParts(customerName))
                        .ToArray();
        
        if (!parts.Any()) {
            return null;
        }

        // TODO: get machine info from a config file
        var machines = new List<Machine>() {
            new() {
                Name = "Anderson Stratos",
                TableOrientation = TableOrientation.Standard,
                NestOutputDirectory = @"C:\Users\Zachary Londono\Desktop\CC Output",
                SingleProgramOutputDirectory = @"C:\Users\Zachary Londono\Desktop\CC Output",
                ToolFilePath = @"Y:\CADCode\cfg\Tool Files\Andi Stratos Royal - Tools from Omni.mdb",
                PictureOutputDirectory = @"C:\Users\Zachary Londono\Desktop\CC Output",
                LabelDatabaseOutputDirectory = @"C:\Users\Zachary Londono\Desktop\CC Output",
            },
            new() {
                Name = "Omnitech Selexx",
                TableOrientation = TableOrientation.Rotated,
                NestOutputDirectory = @"C:\Users\Zachary Londono\Desktop\CC Output",
                SingleProgramOutputDirectory = @"C:\Users\Zachary Londono\Desktop\CC Output",
                ToolFilePath = @"Y:\CADCode\cfg\Tool Files\Royal Omnitech Fanuc-Smart names SMALL PARTS.mdb",
                PictureOutputDirectory = @"C:\Users\Zachary Londono\Desktop\CC Output",
                LabelDatabaseOutputDirectory = @"C:\Users\Zachary Londono\Desktop\CC Output",
            }
        };

        Batch batch = new() {
            Name = $"{order.Number} - {order.Name}",
            Parts = parts,
            InfoFields = new()
        };

        var generator = new GCodeGenerator(CADCodeProxy.Enums.LinearUnits.Millimeters);

        parts.Select(p => (p.Material, p.Thickness))
            .Distinct()
            .ForEach(material => generator.Inventory.Add(new() {
                MaterialName = material.Material,
                AvailableQty = 10,
                IsGrained = true,
                PanelLength = 2000,
                PanelWidth = 1000,
                PanelThickness = material.Thickness,
                Priority = 1,
            }));

        ShowProgressBar?.Invoke();

        if (SetProgressBarValue is not null) generator.CADCodeProgressEvent += SetProgressBarValue.Invoke;
        //if (OnError is not null) generator.CADCodeErrorEvent += (msg) => OnError.Invoke(msg);
        if (OnProgressReport is not null) generator.GenerationEvent += OnProgressReport.Invoke;

        var result = await Task.Run(() => generator.GeneratePrograms(machines, batch, ""));
        DateTime timestamp = DateTime.Now;

        HideProgressBar?.Invoke();

        static string GetImageFileName(string patternName) {

            int idx = patternName.IndexOf('.');
            if (idx < 0) {
                return patternName;
            }

            return patternName[..idx];

        }

        var releases = result.MachineResults.Select(machineResult => {

            var currentOrientation = (machines.FirstOrDefault(m => m.Name == machineResult.MachineName)?.TableOrientation ?? TableOrientation.Standard);

            var programs = machineResult.MaterialGCodeGenerationResults
                        .SelectMany(genResult => {
                            return genResult.ProgramNames
                                    .Select((program, idx) => {

                                        var parts = genResult.PlacedParts.Where(p => p.ProgramIndex == idx).ToList();

                                        int inventoryIndex = parts.First().UsedInventoryIndex; // TODO: get inventory index for program name
                                        var inventory = genResult.UsedInventory[inventoryIndex];

                                        var area = inventory.Width * inventory.Length;
                                        var usedArea = parts.Sum(part => part.Width * part.Length);
                                        var yield = usedArea / area;

                                        return new ReleasedProgram() {
                                            Name = program,
                                            ImagePath = @$"C:\Users\Zachary Londono\Desktop\CC Output\{GetImageFileName(program)}.wmf",
                                            HasFace6 = false,
                                            Material = new() {
                                                Name = genResult.MaterialName,
                                                Width = inventory.Width,
                                                Length = inventory.Length,
                                                Thickness = inventory.Thickness,
                                                IsGrained = inventory.IsGrained,
                                                Yield = yield
                                            },
                                            Parts = parts.Select(placedPart => {

                                                // TODO: get label data from result
                                                Dictionary<string, string> label = new();

                                                return new NestedPart() {
                                                    Name = placedPart.Name,
                                                    FileName = label.GetValueOrEmpty("Face5Filename"),
                                                    HasFace6 = false,
                                                    Face6FileName = null,
                                                    ImageData = "",
                                                    Width = Dimension.FromMillimeters(placedPart.Width),
                                                    Length = Dimension.FromMillimeters(placedPart.Length),
                                                    Description = label.GetValueOrEmpty("Description"),
                                                    Center = new(placedPart.InsertionPoint.X, placedPart.InsertionPoint.Y), // TODO: may need to change insertion point based on part rotation
                                                    ProductNumber = label.GetValueOrEmpty("Cabinet Number"),
                                                    ProductId = Guid.Empty, // TODO: find a way to get thr product id
                                                    PartId = "", //placedPart.Id, TODO: get id from result
                                                    IsRotated = placedPart.IsRotated,
                                                    HasBackSideProgram = false,
                                                    Note = "",
                                                };
                                            })
                                            .ToList()
                                        };

                                    });

                        });

            ApplicationCore.Shared.CNC.Domain.TableOrientation orientation = (machines.FirstOrDefault(m => m.Name == machineResult.MachineName)?.TableOrientation ?? TableOrientation.Standard) switch {
                TableOrientation.Rotated => ApplicationCore.Shared.CNC.Domain.TableOrientation.Rotated,
                TableOrientation.Standard or _ => ApplicationCore.Shared.CNC.Domain.TableOrientation.Standard
            };

            return new MachineRelease() {
                MachineName = machineResult.MachineName,
                MachineTableOrientation = orientation,
                ToolTable = new Dictionary<int, string>(),
                Programs = programs,
                SinglePrograms = Enumerable.Empty<SinglePartProgram>()
            };

        });

        return new ReleasedJob() {
            JobName = batch.Name,
            CustomerName = customerName,
            VendorName = vendorName,
            TimeStamp = timestamp,
            OrderDate = order.OrderDate,
            ReleaseDate = DateTime.Today,
            DueDate = order.DueDate,
            Releases = releases
        };

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

        var model = new ReleasedWorkOrderSummary(releasedJobs, note);

        var htmlBody = ReleaseEmailBodyGenerator.GenerateHTMLReleaseEmailBody(model, includeReleaseSummary);
        var textBody = ReleaseEmailBodyGenerator.GenerateTextReleaseEmailBody(model, includeReleaseSummary);

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

        List<string> files = new();

        var documentBytes = await Task.Run(() => {

            var document = Document.Create(doc => {

                foreach (var decorator in decorators) {
                    try {
                        decorator.Decorate(doc);
                    } catch (Exception ex) {
                        OnError?.Invoke($"Error adding pages to document '{name}' - '{ex.Message}'");
                    }
                }

            });
            
            return document.GeneratePdf();

        });

        if (attachedFiles.Any()) {

            List<byte[]> documents = new() {
                documentBytes
            };

            foreach (var file in attachedFiles) {
                documents.Add(await File.ReadAllBytesAsync(file));
            }

            documentBytes = await Task.Run(() => PdfMerger.Merge(documents));

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
