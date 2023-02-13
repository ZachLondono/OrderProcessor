namespace ApplicationCore.Features.Shared.Services;

public class FileReader : IFileReader {

    public bool DoesFileExist(string filePath) => File.Exists(filePath);

    public string GetAvailableFileName(string direcotry, string filename, string fileExtension = "") {

        int index = 1;

        foreach (char c in Path.GetInvalidFileNameChars()) {
            filename = filename.Replace(c, '_');
        }

        if (fileExtension.StartsWith(".")) {
            fileExtension = fileExtension[1..];
        }

        string filepath = Path.Combine(direcotry, $"{filename}{(fileExtension == string.Empty ? "" : $".{fileExtension}")}");

        while (DoesFileExist(filepath)) {

            filepath = Path.Combine(direcotry, $"{filename} ({index++}).{fileExtension}");

        }

        return filepath;

    }

    public Stream OpenReadFileStream(string filepath, FileAccess access = FileAccess.Read) => new FileStream(filepath, FileMode.Open, access, FileShare.ReadWrite);

    public async Task<string> ReadFileContentsAsync(string filePath) => await File.ReadAllTextAsync(filePath);

}
