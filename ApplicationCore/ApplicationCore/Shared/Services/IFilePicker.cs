namespace ApplicationCore.Shared.Services;

public interface IFilePicker {

    public void PickFile(FilePickerOptions options, Action<string> onFilePicked);

    public void PickFiles(FilePickerOptions options, Action<string[]> onFilesPicked);

}
