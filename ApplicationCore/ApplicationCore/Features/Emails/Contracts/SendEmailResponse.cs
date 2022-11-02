namespace ApplicationCore.Features.Emails.Contracts;

public class SendEmailResponse {
    public string ServerResponse { get; init; }
    public SendEmailResponse(string serverResponse) {
        ServerResponse = serverResponse;
    }
}