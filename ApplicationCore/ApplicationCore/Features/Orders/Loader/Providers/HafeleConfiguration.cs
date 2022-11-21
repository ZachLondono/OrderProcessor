namespace ApplicationCore.Features.Orders.Loader.Providers;

internal class HafeleConfiguration {

	public string VendorId { get; set; }

	public string DefaultDirectory { get; init; } = string.Empty;

	public Dictionary<string, string> MaterialMap { get; init; } = new();

}
