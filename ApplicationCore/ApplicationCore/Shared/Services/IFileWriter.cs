namespace ApplicationCore.Shared.Services;

public interface IFileWriter {

    Task OverwriteWriteContentInFileAsync(string filePath, string content);

}
