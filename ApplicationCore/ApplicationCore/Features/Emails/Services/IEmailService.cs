using ApplicationCore.Features.Emails.Domain;

namespace ApplicationCore.Features.Emails.Services;

public interface IEmailService {

    Task<string> SendEmailAsync(Email email);

}
