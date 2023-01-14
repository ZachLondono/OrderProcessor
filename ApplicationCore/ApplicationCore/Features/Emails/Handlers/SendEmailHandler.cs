using ApplicationCore.Features.Emails.Contracts;
using ApplicationCore.Features.Emails.Services;
using ApplicationCore.Infrastructure;

namespace ApplicationCore.Features.Emails.Handlers;

public class SendEmailHandler : CommandHandler<SendEmailRequest, SendEmailResponse> {

    private readonly IEmailService _emailService;
    private readonly IBus _bus;

    public SendEmailHandler(IEmailService emailService, IBus bus) {
        _emailService = emailService;
        _bus = bus;
    }

    public override async Task<Response<SendEmailResponse>> Handle(SendEmailRequest request) {

        var response = await _emailService.SendEmailAsync(request.Email);

        var result = new SendEmailResponse(response);

        await _bus.Publish(new EmailSentNotification(request.Email.Recipients.Aggregate((a, b) => $"{a},{b}")));

        return new(result);

    }

}
