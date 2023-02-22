namespace ApplicationCore.Features.Shared.Services;

public interface IFilePicker {

    public Task<bool> PickFileAsync(string title, string directory, FilePickerFilter filter, Action<string> onFilePicked);

}
