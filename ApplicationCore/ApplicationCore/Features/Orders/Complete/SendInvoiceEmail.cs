using ApplicationCore.Features.Companies.Domain;
using ApplicationCore.Features.Companies.Queries;
using ApplicationCore.Features.Emails.Contracts;
using ApplicationCore.Features.Emails.Domain;
using ApplicationCore.Features.Emails.Services;
using ApplicationCore.Infrastructure;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.Orders.Complete;

public class SendInvoiceEmail : INotificationHandler<TriggerOrderCompleteNotification> {

    private readonly ILogger<SendInvoiceEmail> _logger;
    private readonly IBus _bus;
    private readonly InvoiceEmailConfiguration _configuration;

    public SendInvoiceEmail(ILogger<SendInvoiceEmail> logger, IBus bus, InvoiceEmailConfiguration configuration) {
        _logger = logger;
        _bus = bus;
        _configuration = configuration;
    }

    public async Task Handle(TriggerOrderCompleteNotification notification, CancellationToken cancellationToken) {

        if (!notification.CompleteProfile.EmailInvoice) return;

        var order = notification.Order;

        Company? customer = await GetCompany(order.CustomerId);
        Company? vendor = await GetCompany(order.VendorId);
        if (customer is null || vendor is null) {
            // TODO: send error ui notification
            return;
        }

        var sender = new EmailSender(_configuration.SenderName, _configuration.SenderEmail, _configuration.SenderPassword, _configuration.Host, _configuration.Port);
        var recipients = new string[] { customer.InvoiceEmail, vendor.InvoiceEmail };
        string subject = $"{order.Number} - INVOICE";
        string body = $"Please see attached invoice";

        var email = new Email(sender, recipients, subject, body);

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


}
