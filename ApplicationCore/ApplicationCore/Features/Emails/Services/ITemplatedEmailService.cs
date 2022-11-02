using ApplicationCore.Features.Emails.Domain;

namespace ApplicationCore.Features.Emails.Services;

public interface ITemplatedEmailService {

    Task<string> SendEmailAsync(Email email, object model);

}