namespace ApplicationCore.Features.Orders.Loader.Providers;

internal class OTConfiguration {

    public Dictionary<string, string> VendorIds { get; set; } = new();

    public string DefaultDirectory { get; init; } = string.Empty;

    public Dictionary<string, string> MaterialMap { get; init; } = new();

}
