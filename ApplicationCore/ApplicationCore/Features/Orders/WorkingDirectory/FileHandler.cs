namespace ApplicationCore.Features.Orders.WorkingDirectory;

internal class FileHandler : IFileHandler {

    public void Copy(string sourceFileName, string destFileName) => File.Copy(sourceFileName, destFileName);

    public void CreateDirectory(string path) => Directory.CreateDirectory(path);

    public void DeleteDirectory(string path) => Directory.Delete(path, false);

    public void DeleteFile(string path) => File.Delete(path);

    public bool DirectoryExists(string path) => Directory.Exists(path);

    public bool FileExists(string path) => File.Exists(path);

    public string[] GetFiles(string path, string searchPattern, SearchOption searchOption) => Directory.GetFiles(path, searchPattern, searchOption);

    public void Move(string sourceFileName, string destFileName) => File.Move(sourceFileName, destFileName);

}
