namespace ApplicationCore.Features.Shared.Services;

public interface IFileReader {

    // TODO: segregate read and write access into seperate interfaces
    Stream OpenReadFileStream(string filepath, FileAccess access = FileAccess.Read);

    Task<string> ReadFileContentsAsync(string filePath);

    bool DoesFileExist(string filePath);

    string GetAvailableFileName(string direcotry, string filename, string fileExtension = "");

}
