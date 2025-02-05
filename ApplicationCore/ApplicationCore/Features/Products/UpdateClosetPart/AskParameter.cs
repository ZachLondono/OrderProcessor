namespace ApplicationCore.Features.Products.UpdateClosetPart;

public class AskParameter {

    public required string Name { get; set; }
    public required string Value { get; set; }

    public static AskParameter FromKeyValuePair(KeyValuePair<string, string> pair) {

        return new() {
            Name = pair.Key,
            Value = pair.Value,
        };

    }

}