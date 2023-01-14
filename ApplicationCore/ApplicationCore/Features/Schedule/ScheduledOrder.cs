using ApplicationCore.Features.Orders.Shared.Domain;

namespace ApplicationCore.Features.Schedule;

public class ScheduledOrder {

    public Guid Id { get; set; }

    public string Number { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public DateTime ProductionDate { get; set; }

    public Status Status { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public string VendorName { get; set; } = string.Empty;


}