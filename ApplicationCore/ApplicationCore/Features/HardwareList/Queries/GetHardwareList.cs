using Domain.Infrastructure.Bus;
using Domain.Orders.Persistance;
using Domain.Orders.Persistance.Repositories;
using Domain.Orders.ValueObjects;

namespace ApplicationCore.Features.HardwareList.Queries;

public class GetHardwareList {

    public record Query(Guid OrderId) : IQuery<Hardware>;

    public class Handler(IOrderingDbConnectionFactory factory) : QueryHandler<Query, Hardware> {

        private readonly IOrderingDbConnectionFactory _factory = factory;

        public override async Task<Response<Hardware>> Handle(Query query) {

            using var connection = await _factory.CreateConnection();

            return await Task.Run(() => {

                var suppliesRepo = new OrderSuppliesRepository(connection);
                var supplies = suppliesRepo.GetOrderSupplies(query.OrderId);
    
                var slidesRepo = new OrderDrawerSlidesRepository(connection);
                var slides = slidesRepo.GetOrderDrawerSlides(query.OrderId);
    
                var railsRepo = new OrderHangingRailRepository(connection);
                var hangingRails = railsRepo.GetOrderHangingRails(query.OrderId);
    
                return new Hardware(supplies.ToArray(), slides.ToArray(), hangingRails.ToArray());

            });

        }

    }

}
