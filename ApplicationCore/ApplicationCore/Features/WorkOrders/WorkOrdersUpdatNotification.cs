using ApplicationCore.Infrastructure.UI;

namespace ApplicationCore.Features.WorkOrders;

public class WorkOrdersUpdateNotification : IUINotification {

    public Guid WorkOrderId { get; set; }

}
