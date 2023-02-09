using ApplicationCore.Infrastructure;

namespace ApplicationCore.Features.WorkOrders;

public class WorkOrdersUpdatNotification : IUINotification {

    public Guid WorkOrderId { get; set; }

}
