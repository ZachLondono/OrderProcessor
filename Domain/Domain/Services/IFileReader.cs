namespace Domain.Services;

public interface IFileReader {

    // TODO: segregate read and write access into separate interfaces
    Stream OpenReadFileStream(string filepath, FileAccess access = FileAccess.Read);

    Task<string> ReadFileContentsAsync(string filePath);

    bool DoesFileExist(string filePath);

    string GetAvailableFileName(string directory, string filename, string fileExtension = "");

    string RemoveInvalidPathCharacters(string input, char replacement = '_');

}
