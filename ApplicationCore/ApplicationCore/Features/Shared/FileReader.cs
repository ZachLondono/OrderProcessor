namespace ApplicationCore.Features.Shared;

public class FileReader : IFileReader {

    public bool DoesFileExist(string filePath) => File.Exists(filePath);

    public Stream OpenReadFileStream(string filepath, FileAccess access = FileAccess.Read) => new FileStream(filepath, FileMode.Open, access, FileShare.ReadWrite);

    public async Task<string> ReadFileContentsAsync(string filePath) => await File.ReadAllTextAsync(filePath);

}
