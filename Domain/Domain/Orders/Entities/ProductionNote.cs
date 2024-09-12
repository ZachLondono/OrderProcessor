namespace Domain.Orders.Entities;

public class ProductionNote {

    public Guid Id { get; init; }
    public string Value { get; init; }

    public ProductionNote(Guid id, string value) {
        Id = id;
        Value = value;
    }

    public static ProductionNote Create(string value)
        => new(Guid.NewGuid(), value);

}
