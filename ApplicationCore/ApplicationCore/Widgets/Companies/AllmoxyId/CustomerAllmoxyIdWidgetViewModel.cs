using ApplicationCore.Features.Companies.AllmoxyId;
using ApplicationCore.Infrastructure.Bus;

namespace ApplicationCore.Widgets.Companies.AllmoxyId;

public class CustomerAllmoxyIdWidgetViewModel {

    //public Error Error { get; set; }

    private readonly IBus _bus;

    public CustomerAllmoxyIdWidgetViewModel(IBus bus) {
        _bus = bus;
    }

    public async Task<int?> GetCustomerAllmoxyId(Guid customerId) {

        var response = await _bus.Send(new GetAllmoxyId.Query(customerId));

        int? id = null;
        response.Match(
                allmoxyId => id = allmoxyId,
                error => id = null
            );

        return id;

    }

    public async Task SetCustomerAllmoxyId(Guid customerId, int allmoxyId) {

        _ = await _bus.Send(new UpdateAllmoxyId.Command(customerId, allmoxyId));

    }

    public async Task AddCustomerAllmoxyId(Guid customerId, int allmoxyId) {

        _ = await _bus.Send(new InsertAllmoxyId.Command(customerId, allmoxyId));

    }

}
