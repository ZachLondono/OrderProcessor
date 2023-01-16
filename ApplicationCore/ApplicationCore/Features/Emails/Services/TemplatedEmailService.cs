using ApplicationCore.Features.Emails.Domain;
using ApplicationCore.Features.Shared;

namespace ApplicationCore.Features.Emails.Services;

internal class TemplatedEmailService : ITemplatedEmailService {

    private readonly IEmailService _emailService;
    private readonly ITemplateService _templateService;

    public TemplatedEmailService(IEmailService emailService, ITemplateService templateService) {
        _emailService = emailService;
        _templateService = templateService;
    }

    public async Task<string> SendEmailAsync(Email email, object model) {

        email.Subject = await _templateService.FillTemplate(email.Subject, model);
        email.Body = await _templateService.FillTemplate(email.Body, model);

        return await _emailService.SendEmailAsync(email);
    }
}
