using ApplicationCore.Features.Emails.Contracts;
using ApplicationCore.Features.Emails.Domain;
using ApplicationCore.Infrastructure;
using ApplicationCore.Shared;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApplicationCore.Features.Emails.Handlers;

public class LoadEmailTemplateFromFileHandler : QueryHandler<LoadEmailTemplateFromFileRequest, LoadEmailTemplateFromFileResponse> {

    private readonly IFileReader _fileReader;

    public LoadEmailTemplateFromFileHandler(IFileReader fileReader) {
        _fileReader = fileReader;
    }

    public override async Task<Response<LoadEmailTemplateFromFileResponse>> Handle(LoadEmailTemplateFromFileRequest request) {

        var templateContent = await _fileReader.ReadFileContentsAsync(request.FilePath);

        var email = TryParseInvoiceEmailTemplate(templateContent);

        if (email is null) {
            return new(new Error() {
                Title = "Invalid Template",
                Details = "Could not load email template from file"
            });
        }

        return new(new LoadEmailTemplateFromFileResponse(email!));

    }

    private static Email? TryParseInvoiceEmailTemplate(string template) {
        try {
            var data = JsonSerializer.Deserialize<EmailData>(template);

            if (data is null) throw new InvalidDataException();

            var senderData = data.Sender;
            var sender = new EmailSender(senderData?.Name ?? "", senderData?.Email ?? "", senderData?.Password ?? "", senderData?.Host ?? "", senderData?.Port ?? 0);
            var email = new Email(sender, data.Recipients, data.Subject, data.Body);
            return email;
        }
        catch {
            return null;
        }
    }

    public class EmailData {

        [JsonPropertyName("sender")]
        public SenderData? Sender { get; set; } = default;

        [JsonPropertyName("recipients")]
        public string[] Recipients { get; set; } = Array.Empty<string>();

        [JsonPropertyName("subject")]
        public string Subject { get; set; } = string.Empty;

        [JsonPropertyName("body")]
        public string Body { get; set; } = string.Empty;

    }

    public class SenderData {

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;

        [JsonPropertyName("host")]
        public string Host { get; set; } = string.Empty;

        [JsonPropertyName("port")]
        public int Port { get; set; }

    }

}
