namespace Domain.Companies.ValueObjects;

public record Address {

    public string Line1 { get; set; } = string.Empty;
    public string Line2 { get; set; } = string.Empty;
    public string Line3 { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Zip { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;

    public string GetLine4() {
        return $"{City}{(string.IsNullOrWhiteSpace(State) ? "" : $", {State}")}{(string.IsNullOrWhiteSpace(Zip) ? "" : $" {Zip}")}";
    }

}
