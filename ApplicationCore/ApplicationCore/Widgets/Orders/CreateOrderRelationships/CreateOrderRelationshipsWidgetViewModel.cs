using ApplicationCore.Features.Orders.OrderRelationships;
using ApplicationCore.Infrastructure.Bus;

namespace ApplicationCore.Widgets.Orders.CreateOrderRelationships;

internal class CreateOrderRelationshipsWidgetViewModel {

    private readonly IBus _bus;

    public CreateOrderRelationshipsWidgetViewModel(IBus bus) {
        _bus = bus;
    }

    public async Task CreateRelationships(IEnumerable<Guid> orderIds) {

        var combinations = GetKCombs(orderIds, 2);
        
        foreach (var combination in combinations) {

            await _bus.Send(new InsertOrderRelationship.Command(combination.First(), combination.Skip(1).First()));

        }

    } 

    static IEnumerable<IEnumerable<T>> GetKCombs<T>(IEnumerable<T> list, int length) where T : IComparable {
        if (length == 1) return list.Select(t => new T[] { t });
        return GetKCombs(list, length - 1)
            .SelectMany(t => list.Where(o => o.CompareTo(t.Last()) > 0), 
                (t1, t2) => t1.Concat(new T[] { t2 }));
    }

}
