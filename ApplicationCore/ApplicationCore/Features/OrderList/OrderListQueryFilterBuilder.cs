namespace ApplicationCore.Features.OrderList;

public class OrderListQueryFilterBuilder {

    public string? SearchTerm { get; set; } = null;
    public Guid? CustomerId { get; set; } = null;
    public Guid? VendorId { get; set; } = null;
    public int Page { get; set; } = 0;
    public int PageSize { get; set; } = 0;

    public string GetQueryFilter() {

        List<string> filters = new();
        if (VendorId is not null && VendorId != Guid.Empty) {
            filters.Add("vendor_id = @VendorId");
        }
        if (CustomerId is not null && CustomerId != Guid.Empty) {
            filters.Add("customer_id = @CustomerId");
        }
        if (SearchTerm is not null) {
            filters.Add($"(name LIKE @ModifiedSearchTerm OR number LIKE @ModifiedSearchTerm)");
        }

        string filter = "";
        if (filters.Any()) {
            filter += " WHERE " + string.Join(" AND ", filters.ToArray());
        }

        string offset = "";
        if (Page >= 1 && PageSize > 0) {
            offset = $" OFFSET @CurrentPageStart";
        }

        string limit = "";
        if (PageSize > 0) {
            limit = $" LIMIT @PageSize";
        }

        return $"""
                {filter}
                ORDER BY order_date DESC{limit}{offset}
                """;

    }

}
