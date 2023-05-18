namespace ApplicationCore.Features.Shared.Services;

public interface IFilePicker {

    public void PickFile(string title, string directory, FilePickerFilter filter, Action<string> onFilePicked);

}
