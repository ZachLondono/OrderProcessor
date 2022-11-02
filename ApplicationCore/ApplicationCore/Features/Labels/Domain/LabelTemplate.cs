namespace ApplicationCore.Features.Labels.Domain;

public class LabelTemplate {

    public string Name { get; init; }

    public IReadOnlyList<string> Fields { get; init; }

    public LabelTemplate(string name, IReadOnlyList<string> fields) {
        Name = name;
        Fields = fields;
    }

}
