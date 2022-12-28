namespace ApplicationCore.Features.CNC.GCode.Contracts;

public class Batch {

    public required string Name { get; init; }
    public required BatchMaterial Material { get; init; }
    public required IReadOnlyList<Part> Parts { get; init; }
    public required IEnumerable<LabelField> LabelFields { get; init; }

    public IEnumerable<Label> GetLabels() {

        foreach (var part in Parts) {

            List<LabelField> fields = new(LabelFields);
            fields.AddRange(part.LabelFields);

            yield return new Label(fields);

        }

    }

}
