namespace ApplicationCore.Shared;

public interface IFilePicker {

    public bool TryPickFile(string title, string directory, FilePickerFilter filter, out string fileName);

}
