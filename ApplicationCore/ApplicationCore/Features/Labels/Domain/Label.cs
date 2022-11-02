namespace ApplicationCore.Features.Labels.Domain;

public class Label {

    public IReadOnlyDictionary<string, string> Fields { get; init; }

    public Label(IReadOnlyDictionary<string, string> fields) {
        Fields = fields;
    }

}
