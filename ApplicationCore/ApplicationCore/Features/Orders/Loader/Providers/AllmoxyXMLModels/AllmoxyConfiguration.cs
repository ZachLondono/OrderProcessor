namespace ApplicationCore.Features.Orders.Loader.Providers.AllmoxyXMLModels;

public class AllmoxyConfiguration {

    public string VendorId { get; init; } = string.Empty;

    public string DefaultDirectory { get; init; } = string.Empty;

    public Dictionary<string, string> OptionMap { get; init; } = new();

    public Dictionary<string, string> MaterialMap { get; init; } = new();

}
