using ApplicationCore.Features.Orders.Data;
using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.State.DataModels;
using ApplicationCore.Infrastructure.Bus;
using Dapper;
using Microsoft.Extensions.Logging;
using System.Data;

namespace ApplicationCore.Features.Orders.Shared.State;

public class GetOrderById {

    public record Query(Guid OrderId) : IQuery<Order>;

    public class Handler : QueryHandler<Query, Order> {

        private readonly IOrderingDbConnectionFactory _factory;

        public Handler(IOrderingDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response<Order>> Handle(Query request) {

            using var connection = _factory.CreateConnection();

            var orderData = await connection.QuerySingleAsync<OrderDataModel>(OrderDataModel.GetQueryById(), new { Id = request.OrderId });
            
            var itemData = await connection.QueryAsync<AdditionalItemDataModel>(AdditionalItemDataModel.GetQueryByOrderId(), request);
            var items = itemData.Select(i => i.AsDomainModel());

            var products = GetProducts(request.OrderId, connection);

            var order = orderData.AsDomainModel(request.OrderId, products, items);

            return Response<Order>.Success(order);

        }

        private IEnumerable<IProduct> GetProducts(Guid orderId, IDbConnection connection) {

            return Enumerable.Empty<IProduct>();
            
        }

    }

}
