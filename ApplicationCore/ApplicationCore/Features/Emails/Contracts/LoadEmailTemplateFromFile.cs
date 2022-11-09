using ApplicationCore.Infrastructure;

namespace ApplicationCore.Features.Emails.Contracts;

public class LoadEmailTemplateFromFileRequest : IQuery<LoadEmailTemplateFromFileResponse> {

    public string FilePath { get; init; }

    public LoadEmailTemplateFromFileRequest(string filePath) {
        FilePath = filePath;
    }

}