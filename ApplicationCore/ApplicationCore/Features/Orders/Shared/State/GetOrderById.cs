using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Orders.Shared.State.DataModels;
using ApplicationCore.Shared.Data.Ordering;
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

            using var connection = await _factory.CreateConnection();

            var orderData = await connection.QuerySingleOrDefaultAsync<OrderDataModel>(OrderDataModel.GetQueryById(), new { Id = request.OrderId });

            if (orderData is null) {

                return Response<Order>.Error(new() {
                    Title = "Order not found",
                    Details = $"Could not find order in database with given id {request.OrderId}"
                });

            }

            var itemData = await connection.QueryAsync<AdditionalItemDataModel>(AdditionalItemDataModel.GetQueryByOrderId(), request);
            var items = itemData.Select(i => i.ToDomainModel());

            IEnumerable<IProduct> products;
            try {
                products = await GetProducts(request.OrderId, connection);
            } catch (Exception ex) {
                return Response<Order>.Error(new() {
                    Title = "Failed to load products",
                    Details = ex.Message
                });
            }

            var order = orderData.ToDomainModel(request.OrderId, products, items);

            return Response<Order>.Success(order);

        }

        private async Task<IEnumerable<IProduct>> GetProducts(Guid orderId, IDbConnection connection) {

            List<IProductDataModel> productData = new();

            // add product data to list here
            await AddProductDataToCollection<ClosetPartDataModel>(productData, orderId, connection);
            await AddProductDataToCollection<CustomDrilledVerticalPanelDataModel>(productData, orderId, connection);
            await AddProductDataToCollection<DovetailDrawerBoxDataModel>(productData, orderId, connection);
            await AddMDFDoorProductDataToCollection(productData, orderId, connection);

            await AddProductDataToCollection<BaseCabinetDataModel>(productData, orderId, connection);
            await AddProductDataToCollection<WallCabinetDataModel>(productData, orderId, connection);
            await AddProductDataToCollection<DrawerBaseCabinetDataModel>(productData, orderId, connection);
            await AddProductDataToCollection<TallCabinetDataModel>(productData, orderId, connection);
            await AddProductDataToCollection<SinkCabinetDataModel>(productData, orderId, connection);
            await AddProductDataToCollection<TrashCabinetDataModel>(productData, orderId, connection);
            await AddProductDataToCollection<DiagonalBaseCabinetDataModel>(productData, orderId, connection);
            await AddProductDataToCollection<DiagonalWallCabinetDataModel>(productData, orderId, connection);
            await AddProductDataToCollection<PieCutWallCabinetDataModel>(productData, orderId, connection);
            await AddProductDataToCollection<PieCutBaseCabinetDataModel>(productData, orderId, connection);
            await AddProductDataToCollection<BlindWallCabinetDataModel>(productData, orderId, connection);
            await AddProductDataToCollection<BlindBaseCabinetDataModel>(productData, orderId, connection);

            return productData.Aggregate(new List<IProduct>(), ProductAggregator);

        }

        private async Task AddProductDataToCollection<T>(List<IProductDataModel> productData, Guid orderId, IDbConnection connection) where T : IQueryableProductDataModel {

            try {

                string query = T.GetQueryByOrderId;
                var data = await connection.QueryAsync<T>(query, new { OrderId = orderId });
                productData.AddRange(data.Cast<IProductDataModel>());

            } catch (Exception ex) {
                _logger.LogError(ex, "Exception thrown while trying to read data for product type {Type} in order {OrderId}", typeof(T), orderId);
                throw;
            }

        }

        private async Task AddMDFDoorProductDataToCollection(List<IProductDataModel> productData, Guid orderId, IDbConnection connection) {

            try {

                var query = MDFDoorDataModel.GetQueryByOrderId;
                var doors = await connection.QueryAsync<MDFDoorDataModel>(query, new { OrderId = orderId });

                string openingsQuery = MDFDoorDataModel.GetAdditionalOpeningsQueryByProductId;
                foreach (var door in doors) {
                    var data = await connection.QueryAsync<AdditionalOpening>(openingsQuery, new { ProductId = door.Id });
                    if (data is IEnumerable<AdditionalOpening> openings) {
                        door.AdditionalOpenings = openings.ToArray();
                    } else {
                        door.AdditionalOpenings = Array.Empty<AdditionalOpening>();
                    }
                }

                productData.AddRange(doors);

            } catch (Exception ex) {
                _logger.LogError(ex, "Exception thrown while trying to read data for product type {Type} in order {OrderId}", typeof(MDFDoorDataModel), orderId);
                throw;
            }

        }

        private List<IProduct> ProductAggregator(List<IProduct> accumulator, IProductDataModel data) {
            try {
                accumulator.Add(data.MapToProduct());
            } catch (Exception ex) {
                _logger.LogError(ex, "Exception thrown while trying to map data to product");
                throw;
            }
            return accumulator;
        }

    }

}
