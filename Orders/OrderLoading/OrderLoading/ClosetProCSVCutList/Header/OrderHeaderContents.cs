using Domain.Companies.Entities;

namespace OrderLoading.ClosetProCSVCutList.Header;

public record OrderHeaderContents(
    string OrderNumber,
    string OrderName,
    string WorkingDirectory,
    Customer Customer);
