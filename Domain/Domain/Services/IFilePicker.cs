namespace Domain.Services;

public interface IFilePicker {

    public void PickFile(FilePickerOptions options, Action<string> onFilePicked);

    public void PickFile(FilePickerOptions options, Action<string> onFilePicked, Action onCancel);

    public void PickFiles(FilePickerOptions options, Action<string[]> onFilesPicked);

}
