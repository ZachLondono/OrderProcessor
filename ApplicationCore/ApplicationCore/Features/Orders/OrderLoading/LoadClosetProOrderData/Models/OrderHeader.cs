namespace ApplicationCore.Features.Orders.OrderLoading.LoadClosetProOrderData.Models;

public record OrderHeader {

    /// <summary>
    /// Includes designer's name as well as the designer's company (eg "Designer Name (Designer Company)")
    /// </summary>
    public string Designer { get; init; } = string.Empty; // Contains designer name and designer company

    /// <summary>
    /// Full customer name preceded by some other ClosetProSoftware information (eg "CP-USER 5 FirstName LastName")
    /// </summary>
    public string Customer { get; init; } = string.Empty;

    public string DesignerCompany { get; init; } = string.Empty;

    /// <summary>
    /// Customer name where the first name is abbreviated to just the initial (eg FirstName LastName => "F. LastName")
    /// </summary>
    public string CustomerName { get; init; } = string.Empty;

    public string OrderName { get; init; } = string.Empty;

    public string GetDesignerName() {
        try {
            return Designer[0..Designer.IndexOf('(')];
        } catch {
            return Designer;
        }
    }

}
