using ApplicationCore.Features.Emails.Contracts;
using ApplicationCore.Features.Emails.Services;
using ApplicationCore.Infrastructure;

namespace ApplicationCore.Features.Emails.Handlers;

public class SendTemplatedEmailHandler : CommandHandler<SendTemplatedEmailRequest, SendEmailResponse> {

    private readonly ITemplatedEmailService _emailService;
    private readonly IBus _bus;

    public SendTemplatedEmailHandler(ITemplatedEmailService emailService, IBus bus) {
        _emailService = emailService;
        _bus = bus;
    }

    public override async Task<Response<SendEmailResponse>> Handle(SendTemplatedEmailRequest request) {

        var response = await _emailService.SendEmailAsync(request.Email, request.Model);

        await _bus.Publish(new EmailSentNotification(request.Email.Recipients.Aggregate((a, b) => $"{a},{b}")));

        return new(new SendEmailResponse(response));

    }

}