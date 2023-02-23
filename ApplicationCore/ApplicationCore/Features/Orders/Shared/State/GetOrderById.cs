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
        private readonly ILogger<Handler> _logger;

        public Handler(ILogger<Handler> logger, IOrderingDbConnectionFactory factory) {
            _logger = logger;
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

            List<IProductDataModel> productData = new();

            // add product data to list here

            return productData.Aggregate(new List<IProduct>(), ProductAggregator);

        }

        private async Task AddProductDataToCollection<T>(List<IProductDataModel> productData, Guid orderId, IDbConnection connection) where T : IQueryableProductDataModel {

            try {

                var data = await connection.QueryAsync<T>(T.GetQueryByOrderId(), new { OrderId = orderId });
                productData.AddRange(data.Cast<IProductDataModel>());

            } catch (Exception ex) {
                _logger.LogError("Exception thrown while trying to read data for product type {Type} in order {OrderId} {Ex}", typeof(T), orderId, ex);
            }

        }

        private List<IProduct> ProductAggregator(List<IProduct> accumulator, IProductDataModel data) {
            try {
                accumulator.Add(data.MapToProduct());
            } catch (Exception ex) {
                _logger.LogError("Exception thrown while trying to map data to product {Ex}", ex);
            }
            return accumulator;
        }

    }

}
