namespace OrderLoading.LoadDoweledDBSpreadsheetOrderData.Models;

public class DoweledDBOrderProviderOptions {

    public string DefaultWorkingDirectory { get; set; } = string.Empty;

    public Dictionary<string, string> VendorIds { get; set; } = new();

}
