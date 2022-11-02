namespace ApplicationCore.Shared;

public record FilePickerFilter(string Description, string Extension) {

    public string ToFilterString() {
        return $"{Description}|*{Extension}";
    }

}