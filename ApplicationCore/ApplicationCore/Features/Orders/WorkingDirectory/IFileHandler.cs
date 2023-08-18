namespace ApplicationCore.Features.Orders.WorkingDirectory;

public interface IFileHandler {

    bool FileExists(string path);
    bool DirectoryExists(string path);
    void CreateDirectory(string path);
    string[] GetFiles(string path, string searchPattern, SearchOption searchOption);
    void Copy(string sourceFileName, string destFileName); 
    void Move(string sourceFileName, string destFileName); 
    void DeleteFile(string path); 
    void DeleteDirectory(string path); 

}
