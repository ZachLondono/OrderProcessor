namespace ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

public record Address {

    public string Line1 { get; init; } = string.Empty;
    public string Line2 { get; init; } = string.Empty;
    public string Line3 { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string Zip { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;

    public string GetLine4() {
        return $"{City}{(string.IsNullOrWhiteSpace(State) ? "" : $", {State}")}{(string.IsNullOrWhiteSpace(Zip) ? "" : $" {Zip}")}";
    }

}