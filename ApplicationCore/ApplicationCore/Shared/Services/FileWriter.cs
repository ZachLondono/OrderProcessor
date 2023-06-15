namespace ApplicationCore.Shared.Services;

public class FileWriter : IFileWriter {

    public Task OverwriteWriteContentInFileAsync(string filePath, string content) => File.WriteAllTextAsync(filePath, content);

}
