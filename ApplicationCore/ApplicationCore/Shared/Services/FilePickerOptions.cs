namespace ApplicationCore.Features.Shared.Services;

public class FilePickerOptions {

    public string Title { get; set; } = string.Empty;
    public string InitialDirectory { get; set; } = string.Empty;
    public FilePickerFilter Filter { get; set; } = FilePickerFilter.NoFilter;

}
