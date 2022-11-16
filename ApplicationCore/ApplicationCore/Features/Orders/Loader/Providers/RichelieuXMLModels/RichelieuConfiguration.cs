namespace ApplicationCore.Features.Orders.Loader.Providers.RichelieuXMLModels;

internal class RichelieuConfiguration {

    public required Guid VendorId { get; set; }

    public required string Schema { get; set; }

    public required Dictionary<string, string> OptionMap { get; set; }

    public required Dictionary<string, string> MaterialMap { get; set; }

}
