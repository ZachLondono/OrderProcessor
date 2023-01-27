using ApplicationCore.Features.Companies.Domain;
using ApplicationCore.Features.Companies.Queries;
using ApplicationCore.Features.Emails.Contracts;
using ApplicationCore.Features.Emails.Domain;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Infrastructure;
using ApplicationCore.Features.Shared;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using Unit = QuestPDF.Infrastructure.Unit;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Entities;

namespace ApplicationCore.Features.Orders.Complete;

public class SendInvoiceEmail : DomainListener<TriggerOrderCompleteNotification> {

    private readonly ILogger<SendInvoiceEmail> _logger;
    private readonly IBus _bus;
    private readonly EmailConfiguration _configuration;
    private readonly IFileReader _fileReader;

    public SendInvoiceEmail(ILogger<SendInvoiceEmail> logger, IBus bus, EmailConfiguration configuration, IFileReader fileReader) {
        _logger = logger;
        _bus = bus;
        _configuration = configuration;
        _fileReader = fileReader;
    }

    public override async Task Handle(TriggerOrderCompleteNotification notification) {

        if (!notification.CompleteProfile.EmailInvoice) return;

        var order = notification.Order;
        var profile = notification.CompleteProfile;

        Company? vendor = await GetCompany(order.VendorId);
        if (order.Billing.InvoiceEmail is null || vendor is null) {
            // TODO: send error ui notification
            return;
        }

        var doc = GenerateInvoicePDF(order, vendor);
        string pdfPath = GetAvailableFileName(profile.InvoicePDFDirectory, $"{order.Number} - INVOICE");
        doc.GeneratePdf(pdfPath);


        var sender = new EmailSender(profile.EmailSenderName, profile.EmailSenderEmail, profile.EmailSenderPassword, _configuration.Host, _configuration.Port);
        var recipients = new string[] { order.Billing.InvoiceEmail, vendor.InvoiceEmail }.Where(r => !string.IsNullOrWhiteSpace(r));
        if (!recipients.Any()) {
            // TODO: show no recipients warning
            return;
        }

        // TODO: use templates for subject and body
        string subject = $"{order.Number} - INVOICE";
        string body = $"Please see attached invoice";

        var email = new Email(sender, recipients, subject, body, new string[] { pdfPath });

        var response = await _bus.Send(new SendEmailRequest(email));

        response.Match(
            serverResponse => {
                _logger.LogInformation("Email server response {Response}", serverResponse.ServerResponse);
            },
            error => {
                // TODO: notify of error
                _logger.LogError("Error sending email {Error}", error);
            }
        );

    }

    private async Task<Company?> GetCompany(Guid companyId) {
        Company? company = null;
        var custQuery = await _bus.Send(new GetCompanyById.Query(companyId));
        custQuery.Match(
            (comp) => {
                company = comp;
            },
            (error) => {
                _logger.LogError("Error loading company {Error}", error);
            }
        );
        return company;
    }

    private string GetAvailableFileName(string direcotry, string filename) {

        int index = 1;

        string filepath = Path.Combine(direcotry, $"{filename}.pdf");

        while (_fileReader.DoesFileExist(filepath)) {

            filepath = Path.Combine(direcotry, $"{filename} ({index++}).pdf");

        }

        return filepath;

    }

    private static Document GenerateInvoicePDF(Order order, Company vendor) {

        return Document.Create(container => {

            container.Page(page => {

                var pageSize = PageSizes.A4;
                page.Size(pageSize);
                page.Margin(0, Unit.Centimetre);
                page.PageColor(Colors.White);

                page.Header()
                    .AlignCenter()
                    .Text("Invoice")
                    .FontSize(28);

                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Column(col => {

                        col.Item().Table(table => {

                            uint companyRow = 1;
                            TextSpanDescriptor CompanyInfoCell(TableDescriptor table, string text, float fontSize) {
                                return table.Cell().Row(companyRow++).Column(2).AlignMiddle().PaddingLeft(5).AlignLeft().Text(text).FontSize(fontSize);
                            }

                            table.Cell().Row(companyRow).Column(1).AlignMiddle().AlignRight().Text("From").Italic().FontSize(12);
                            CompanyInfoCell(table, vendor.Name, 16).Bold();
                            CompanyInfoCell(table, vendor.Address.Line1, 12);
                            CompanyInfoCell(table, $"{vendor.Address.City}, {vendor.Address.State} {vendor.Address.Zip}", 12);
                            CompanyInfoCell(table, vendor.PhoneNumber, 12);

                            table.Cell().Row(companyRow++).Text("");
                            table.Cell().Row(companyRow).Column(1).AlignMiddle().AlignRight().Text("To").Italic().FontSize(12);
                            CompanyInfoCell(table, order.Customer.Name, 16).Bold();
                            CompanyInfoCell(table, order.Billing.Address.Line1, 12);
                            CompanyInfoCell(table, $"{order.Billing.Address.City}, {order.Billing.Address.State} {order.Billing.Address.Zip}", 12);
                            CompanyInfoCell(table, order.Billing.PhoneNumber ?? "", 12);

                            int maxCharCount = 0;
                            float fontSize = 12;
                            uint infoRow = 1;
                            void AddInfo(TableDescriptor table, string key, string value, bool bold = false) {
                                var header = table.Cell().Row(infoRow).Column(4).AlignMiddle().AlignRight().Text(key).FontSize(fontSize);
                                if (bold) header.Bold();
                                table.Cell().Row(infoRow).Column(5).AlignMiddle().AlignLeft().PaddingLeft(5).Text(value).FontSize(fontSize);

                                if (value.Length > maxCharCount) maxCharCount = value.Length;
                                infoRow++;
                            }

                            AddInfo(table, "Date:", DateTime.Today.ToShortDateString(), true);
                            AddInfo(table, "Invoice #:", order.Number, true);
                            AddInfo(table, "Job:", order.Name, true);
                            AddInfo(table, "Terms:", "COD", true);

                            AddInfo(table, "", ""); // blank row

                            AddInfo(table, "Subtotal:", $"${order.SubTotal}");
                            if (order.PriceAdjustment != 0) {
                                AddInfo(table, "Discount:", $"${order.PriceAdjustment}");
                                AddInfo(table, "Net Amt.:", $"${order.AdjustedSubTotal}");
                            }
                            AddInfo(table, "Sales Tax:", $"${order.Tax}");
                            AddInfo(table, "Total:", $"${order.Total}", true);

                            float col1Width = 60f;
                            float companyNameWidth = (vendor.Name.Length > order.Customer.Name.Length ? vendor.Name.Length : order.Customer.Name.Length) * 16 * 0.6f;
                            float tableHeaderWidth = 60f;
                            float tableValueWidth = maxCharCount * fontSize * 0.6f;

                            var spacing = pageSize.Width - (col1Width + companyNameWidth + tableHeaderWidth + tableValueWidth);
                            if (spacing < 0) {
                                tableValueWidth += spacing;
                                spacing = pageSize.Width - (col1Width + companyNameWidth + tableHeaderWidth + tableValueWidth);
                            }

                            table.ColumnsDefinition(columns => {
                                columns.ConstantColumn(col1Width);
                                columns.ConstantColumn(companyNameWidth);
                                columns.ConstantColumn(spacing);
                                columns.ConstantColumn(tableHeaderWidth);
                                columns.ConstantColumn(tableValueWidth);
                            });

                        });

                        col.Item().Height(35);

                        col.Item().AlignCenter().Table(table => {

                            table.ColumnsDefinition(columns => {
                                columns.ConstantColumn(30, Unit.Point);
                                columns.ConstantColumn(30, Unit.Point);
                                columns.ConstantColumn(150, Unit.Point);
                                columns.ConstantColumn(30, Unit.Point);
                                columns.ConstantColumn(40, Unit.Point);
                                columns.ConstantColumn(40, Unit.Point);
                                columns.ConstantColumn(40, Unit.Point);
                                columns.ConstantColumn(50, Unit.Point);
                                columns.ConstantColumn(50, Unit.Point);
                            });

                            table.Cell().Row(1).Column(2).BorderTop(1).BorderLeft(1).AlignMiddle().AlignCenter().Text("2").FontSize(12);
                            table.Cell().Row(1).Column(3).BorderTop(1).BorderRight(1).AlignMiddle().AlignLeft().Text("Boxes in Order").FontSize(12);

                            table.Cell().Row(2).Column(1).BorderLeft(1).BorderTop(1).Background("#CED5EA").PaddingVertical(3).AlignMiddle().AlignCenter().Text("Line").Bold().FontSize(11);
                            table.Cell().Row(2).Column(2).BorderLeft(1).BorderTop(1).Background("#CED5EA").PaddingVertical(3).AlignMiddle().AlignCenter().Text("Qty").Bold().FontSize(11);
                            table.Cell().Row(2).Column(3).BorderLeft(1).BorderTop(1).Background("#CED5EA").PaddingVertical(3).AlignMiddle().AlignCenter().Text("Description").Bold().FontSize(11);
                            table.Cell().Row(2).Column(4).BorderLeft(1).BorderTop(1).Background("#CED5EA").PaddingVertical(3).AlignMiddle().AlignCenter().Text("Logo").Bold().FontSize(11);
                            table.Cell().Row(2).Column(5).BorderLeft(1).BorderTop(1).Background("#CED5EA").PaddingVertical(3).AlignMiddle().AlignCenter().Text("Height").Bold().FontSize(11);
                            table.Cell().Row(2).Column(6).BorderLeft(1).BorderTop(1).Background("#CED5EA").PaddingVertical(3).AlignMiddle().AlignCenter().Text("Width").Bold().FontSize(11);
                            table.Cell().Row(2).Column(7).BorderLeft(1).BorderTop(1).Background("#CED5EA").PaddingVertical(3).AlignMiddle().AlignCenter().Text("Depth").Bold().FontSize(11);
                            table.Cell().Row(2).Column(8).BorderLeft(1).BorderTop(1).Background("#CED5EA").PaddingVertical(3).AlignMiddle().AlignCenter().Text("Price").Bold().FontSize(11);
                            table.Cell().Row(2).Column(9).BorderLeft(1).BorderTop(1).BorderRight(1).Background("#CED5EA").PaddingVertical(3).AlignMiddle().AlignCenter().Text("Ext. Price").Bold().FontSize(11);

                            uint row = 3;
                            foreach (var box in order.Products.Where(p => p is DovetailDrawerBoxProduct).Cast<DovetailDrawerBoxProduct>()) {
                                table.Cell().Row(row).Column(1).BorderLeft(1).BorderTop(1).AlignMiddle().AlignCenter().Text(box.ProductNumber).FontSize(10);
                                table.Cell().Row(row).Column(2).BorderLeft(1).BorderTop(1).AlignMiddle().AlignCenter().Text(box.Qty).FontSize(10);
                                table.Cell().Row(row).Column(3).BorderLeft(1).BorderTop(1).AlignMiddle().AlignLeft().PaddingLeft(2).Text("Dovetail Drawer Box").FontSize(10);
                                table.Cell().Row(row).Column(4).BorderLeft(1).BorderTop(1).AlignMiddle().AlignCenter().Text(box.Options.Logo == LogoPosition.None ? "N" : "Y").FontSize(10);
                                table.Cell().Row(row).Column(5).BorderLeft(1).BorderTop(1).AlignMiddle().AlignCenter().Text(box.Height.AsInchFraction()).FontSize(10);
                                table.Cell().Row(row).Column(6).BorderLeft(1).BorderTop(1).AlignMiddle().AlignCenter().Text(box.Width.AsInchFraction()).FontSize(10);
                                table.Cell().Row(row).Column(7).BorderLeft(1).BorderTop(1).AlignMiddle().AlignCenter().Text(box.Depth.AsInchFraction()).FontSize(10);
                                table.Cell().Row(row).Column(8).BorderLeft(1).BorderTop(1).AlignMiddle().AlignCenter().Text($"${box.UnitPrice}").FontSize(10);
                                table.Cell().Row(row).Column(9).BorderLeft(1).BorderTop(1).BorderRight(1).AlignMiddle().AlignCenter().Text($"${box.UnitPrice * box.Qty}").FontSize(10);
                                row++;
                            }
                            table.Cell().Row(row).ColumnSpan(9).BorderTop(1);


                        });

                    });

                page.Footer()
                    .AlignCenter()
                    .Text(x => {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                        x.Span(" of ");
                        x.TotalPages();
                    });

            });

        });

    }

}
