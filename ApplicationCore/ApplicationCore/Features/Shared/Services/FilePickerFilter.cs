namespace ApplicationCore.Features.Shared.Services;

public record FilePickerFilter(string Description, string Extension) {

    private bool _noFilter = false;

    public static FilePickerFilter NoFilter
        => new("", "") {
            _noFilter = true
        };

    public string ToFilterString() {
        return _noFilter ? "" : $"{Description}|*{Extension}";
    }

}