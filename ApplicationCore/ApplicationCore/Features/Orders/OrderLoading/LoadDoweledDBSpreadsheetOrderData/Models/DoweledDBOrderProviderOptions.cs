namespace ApplicationCore.Features.Orders.OrderLoading.LoadDoweledDBSpreadsheetOrderData.Models;

public class DoweledDBOrderProviderOptions {

    public string DefaultWorkingDirectory = string.Empty;

    public Dictionary<string, string> VendorIds { get; set; } = new();

}
