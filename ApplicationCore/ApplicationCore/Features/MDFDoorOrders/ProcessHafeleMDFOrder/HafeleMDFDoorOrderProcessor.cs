using ApplicationCore.Shared.Services;
using Domain.Extensions;
using Domain.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Office.Interop.Excel;
using Microsoft.Office.Interop.Outlook;
using MimeKit;
using OrderExporting.DoorOrderExport;
using OrderExporting.Invoice;
using OrderLoading;
using OrderLoading.LoadHafeleMDFDoorSpreadsheetOrderData.ReadOrderFile;
using QuestPDF.Fluent;
using System.Runtime.InteropServices;
using ExcelApp = Microsoft.Office.Interop.Excel.Application;
using Exception = System.Exception;
using OutlookApp = Microsoft.Office.Interop.Outlook.Application;

namespace ApplicationCore.Features.MDFDoorOrders.ProcessHafeleMDFOrder;

public class HafeleMDFDoorOrderProcessor {

    private readonly ILogger<HafeleMDFDoorOrderProcessor> _logger;
    private readonly IFileReader _fileReader;
    private readonly IEmailService _emailService;

    public HafeleMDFDoorOrderProcessor(ILogger<HafeleMDFDoorOrderProcessor> logger, IFileReader fileReader, IEmailService emailService) {
        _logger = logger;
        _fileReader = fileReader;
        _emailService = emailService;
    }

    public async Task ProcessOrderAsync(ProcessOptions options) {

        var orderData = await Task.Run(() => LoadOrderData(options.DataFile));

        if (orderData is null) {

            _logger.LogError("Failed to load order data from file: {FilePath}", options.DataFile);
            return;

        }

        if (options.GenerateInvoice) {
            await HandleInvoiceGenerationAsync(options, orderData);
        }

        if (options.FillOrderSheet) {
            _ = await Task.Run(() => FillOrderSheet(orderData, options.HafelePO, options.OrderSheetTemplatePath, options.OrderSheetOutputDirectory));
        }

        if (options.PostToGoogleSheets) {
            await PostOrderToGoogleSheet(orderData, options.HafelePO);
        }

    }

    private async Task HandleInvoiceGenerationAsync(ProcessOptions options, HafeleMDFDoorOrder orderData) {
        
        if (!Directory.Exists(options.InvoicePDFOutputDirectory)) {
            _logger.LogError("Not generating invoice because invoice output directory does not exist: {Directory}", options.InvoicePDFOutputDirectory);
            return;
        }

        var invoice = GenerateInvoice(orderData, options.HafelePO, options.InvoicePDFOutputDirectory);

        if (options.SendInvoiceEmail) {
            await HandleInvoiceEmailAsync(options, invoice);
        }

    }

    private async Task HandleInvoiceEmailAsync(ProcessOptions options, string invoiceFilePath) {
        
        if (options.InvoiceEmailRecipients.Count == 0) {
            _logger.LogWarning("Not sending invoice email because no recipients were specified");
            return;
        }

        string subject = $"{options.HafelePO} INVOICE";
        string body = $"Please see attached invoice.";

        if (options.PreviewInvoiceEmail) {
            CreateAndDisplayOutlookEmail(options.InvoiceEmailRecipients.Select(e => e.Address), options.InvoiceEmailCopyRecipients.Select(e => e.Address), subject, body, [invoiceFilePath]);
        } else {
            await SendInvoiceEmail(options.InvoiceEmailRecipients.Select(e => e.Address), options.InvoiceEmailCopyRecipients.Select(e => e.Address), subject, body, [invoiceFilePath]);
        }

    }

    private HafeleMDFDoorOrder? LoadOrderData(string orderData) {
        var result = HafeleMDFDoorOrder.Load(orderData);
        foreach (var warning in result.Warnings) {
            _logger.LogWarning("Hafele Order Parsing Warning: " + warning);
        }
        foreach (var error in result.Errors) {
            _logger.LogError("Hafele Order Parsing Error: " + error);
        }
        return result.Data;
    }

    private string GenerateInvoice(HafeleMDFDoorOrder orderData, string hafelePO, string outputDirectory) {

        var doors = orderData.GetProducts();
        var invoiceAmount = orderData.GetInvoiceAmount();

        var model = new Invoice() {
            OrderNumber = hafelePO,
            OrderName = orderData.Options.JobName,
            Date = DateTime.Today,
            SubTotal = invoiceAmount,
            SalesTax = 0,
            Shipping = 0,
            Total = invoiceAmount,
            Terms = "",
            Discount = 0M,
            Vendor = new() {
                Name = "Royal Cabinet Co.",
                Line1 = "15E Easy St",
                Line2 = "Bound Brook, NJ 08805",
                Line3 = "",
                Line4 = "",
            },
            Customer = new() {
                Name = "Hafele America Co.",
                Line1 = "3901 Cheyenne Drive",
                Line2 = "P.O. Box 4000",
                Line3 = "Archdale, NC 27263",
                Line4 = "",
            },
            MDFDoors = doors.Select(cab => new MDFDoorItem() {
                                Line = cab.ProductNumber,
                                Qty = cab.Qty,
                                Height = cab.Height,
                                Width = cab.Width,
                                Description = cab.GetDescription(),
                                UnitPrice = cab.UnitPrice,
                            }).ToList(),
            Cabinets = [],
            CabinetParts = [],
            FivePieceDoors = [],
            ClosetParts = [],
            ZargenDrawers = [],
            DovetailDrawerBoxes = [],
            DoweledDrawerBoxes = [],
            CounterTops = [],
            AdditionalItems = []
        };

        var document = Document.Create(d => {
            var decorator = new InvoiceDecorator(model);
            decorator.Decorate(d);
        });

        var filePath = Path.Combine(outputDirectory, $"{hafelePO} Invoice.pdf");

        document.GeneratePdf(filePath);
        return filePath;

    }

    private async Task SendInvoiceEmail(IEnumerable<string> recipients, IEnumerable<string> copyRecipients, string subject, string body, IEnumerable<string> attachments) {

        var message = new MimeMessage();

        recipients.Where(s => !string.IsNullOrWhiteSpace(s))
                  .ForEach(r => message.To.Add(new MailboxAddress(r, r)));

        copyRecipients.Where(s => !string.IsNullOrWhiteSpace(s))
                  .ForEach(r => message.Cc.Add(new MailboxAddress(r, r)));

        if (message.To.Count == 0) {
            return;
        }

        var sender = _emailService.GetSender();
        message.From.Add(sender);
        message.Subject = subject;

        var builder = new BodyBuilder {
            TextBody = body
        };
        attachments.Where(a => File.Exists(a)).ForEach(att => builder.Attachments.Add(att));

        message.Body = builder.ToMessageBody();

        _ = await Task.Run(() => _emailService.SendMessageAsync(message));

    }

    private void CreateAndDisplayOutlookEmail(IEnumerable<string> recipients, IEnumerable<string> copyRecipients, string subject, string body, IEnumerable<string> attachments) {

        OutlookApp app = new OutlookApp();
        MailItem mailItem = (MailItem) app.CreateItem(OlItemType.olMailItem);
        mailItem.To = string.Join(';', recipients);
        mailItem.CC = string.Join(';', copyRecipients);
        mailItem.Subject = subject;
        mailItem.Body = body;

        attachments.Where(_fileReader.DoesFileExist)
                   .ForEach(att => mailItem.Attachments.Add(att));

        var senderMailBox = _emailService.GetSender();
        var sender = GetSenderOutlookAccount(app, senderMailBox.Address);

        if (sender is not null) {
            mailItem.SendUsingAccount = sender;
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
    
    private IEnumerable<string> FillOrderSheet(HafeleMDFDoorOrder order, string hafelePO, string template, string outputDirectory) {

        var orderFiles = CreateDoorOrders(order, hafelePO);

        ExcelApp app = new() {
            DisplayAlerts = false,
            Visible = false,
            ScreenUpdating = false
        };

        List<string> filesGenerated = new();

        var workbooks = app.Workbooks;
        //bool wasExceptionThrown = false;
        foreach (var orderFile in orderFiles) {

            Workbook? workbook = null;

            try {

                workbook = workbooks.Open(template, ReadOnly: true);

                app.Calculation = XlCalculation.xlCalculationManual;

                var worksheets = workbook.Worksheets;
                Worksheet worksheet = (Worksheet)worksheets["MDF"];

                orderFile.WriteToWorksheet(worksheet);

                _ = Marshal.ReleaseComObject(worksheet);
                _ = Marshal.ReleaseComObject(worksheets);

                string fileName = _fileReader.GetAvailableFileName(outputDirectory, $"{hafelePO} - {order.Options.JobName} MDF DOORS", ".xlsm");
                string finalPath = Path.GetFullPath(fileName);

                workbook.SaveAs(finalPath);

                filesGenerated.Add(finalPath);

            } catch (Exception ex) {

                _logger.LogError(ex, "Exception thrown while filling door order group");
                //wasExceptionThrown = true;

            } finally {

                if (workbook is not null) {
                    workbook.Close(SaveChanges: false);
                    _ = Marshal.ReleaseComObject(workbook);
                }

            }

        }

        workbooks.Close();
        app?.Quit();

        _ = Marshal.ReleaseComObject(workbooks);
        if (app is not null) _ = Marshal.ReleaseComObject(app);

        // Clean up COM objects, calling these twice ensures it is fully cleaned up.
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        GC.WaitForPendingFinalizers();

        //string? error = wasExceptionThrown ? "An error occurred while trying to write one or more door orders" : null;
        //return new(filesGenerated, error);

        return filesGenerated;

    }

    private static IEnumerable<DoorOrder> CreateDoorOrders(HafeleMDFDoorOrder order, string hafelePO) {

        var doors = order.GetProducts();

        var groups = GeneralSpecs.SeparatedDoorsBySpecs(doors);

        for (int i = 0; i < groups.Length; i++) {

            // TODO: Optimization - find the most common frame width and set that to the default for the group. Then do not overwrite those values in the line items.

            var group = groups[i];

            yield return new() {
                OrderDate = order.Options.Date,
                DueDate = order.Options.GetDueDate(),
                Company = order.Options.Company,
                TrackingNumber = $"{hafelePO}{(groups.Length == 1 ? string.Empty : $"-{i + 1}")}",
                JobName = order.Options.JobName,
                ProcessorOrderId = Guid.Empty,
                Units = DoorOrder.METRIC_UNITS,
                VendorName = "Hafele America Co.",
                Specs = group.Key,
                LineItems = group.Select(d => LineItem.FromDoor(d, group.Key.StilesRails))
            };

        }
    }

    private static async Task PostOrderToGoogleSheet(HafeleMDFDoorOrder order, string hafelePO) {

        var row = new GoogleSheetRow() {
            FileDate = order.Options.Date.ToShortDateString(),
            HafelePO = hafelePO,
            ProjectNumber = order.Options.HafeleOrderNumber,
            ConfigNumber = "",
            CustomerName = order.Options.Company,
            CustomerPO = order.Options.JobName,
            TotalItemCount = order.Sizes.Sum(s => s.Qty).ToString(),
            ShipDate = order.Options.GetDueDate().ToShortDateString(),
            InvoiceAmount = order.GetInvoiceAmount().ToString(),
            TrackingNumber = ""
        };

        await row.PostData();

    }

}
