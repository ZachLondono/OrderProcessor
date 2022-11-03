using ApplicationCore.Features.Orders.Domain;

namespace ApplicationCore.Features.Schedule;

public class ScheduledOrder {

    public Guid Id { get; set; }

    public string Number { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public DateTime ScheduledDate { get; set; }

    public Status Status { get; set; }

}