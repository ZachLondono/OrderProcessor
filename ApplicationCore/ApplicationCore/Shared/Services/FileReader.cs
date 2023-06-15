namespace ApplicationCore.Shared.Services;

public class FileReader : IFileReader {

    public bool DoesFileExist(string filePath) => File.Exists(filePath);

    public string GetAvailableFileName(string directory, string filename, string fileExtension = "") {

        int index = 1;

        filename = RemoveInvalidPathCharacters(filename);

        if (fileExtension.StartsWith(".")) {
            fileExtension = fileExtension[1..];
        }

        string filepath = Path.Combine(directory, $"{filename}{(fileExtension == string.Empty ? "" : $".{fileExtension}")}");

        while (DoesFileExist(filepath)) {

            filepath = Path.Combine(directory, $"{filename} ({index++}).{fileExtension}");

        }

        return filepath;

    }

    public Stream OpenReadFileStream(string filepath, FileAccess access = FileAccess.Read) => new FileStream(filepath, FileMode.Open, access, FileShare.ReadWrite);

    public async Task<string> ReadFileContentsAsync(string filePath) => await File.ReadAllTextAsync(filePath);

    public string RemoveInvalidPathCharacters(string input, char replacement = '_') {

        foreach (char c in Path.GetInvalidFileNameChars()) {
            input = input.Replace(c, replacement);
        }

        return input;

    }

}
