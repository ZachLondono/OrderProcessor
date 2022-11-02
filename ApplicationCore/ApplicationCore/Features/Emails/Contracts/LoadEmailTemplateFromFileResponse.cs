using ApplicationCore.Features.Emails.Domain;

namespace ApplicationCore.Features.Emails.Contracts;

public class LoadEmailTemplateFromFileResponse {

    public Email Email {get; init; }

    public LoadEmailTemplateFromFileResponse(Email email) {
        Email = email;
    }

}
