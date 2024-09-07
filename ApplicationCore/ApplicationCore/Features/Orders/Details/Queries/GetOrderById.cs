using Domain.Orders.Entities;
using Domain.Orders.ValueObjects;
using Domain.Orders.Persistance.DataModels;
using Microsoft.Extensions.Logging;
using System.Data;
using Domain.Orders.Entities.Products;
using Domain.Infrastructure.Bus;
using Domain.Orders.Persistance;
using Domain.Orders.Persistance.Repositories;
using Domain.Infrastructure.Data;

namespace ApplicationCore.Features.Orders.Details.Queries;

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

            var orderData = connection.QuerySingleOrDefault<OrderDataModel>(OrderDataModel.GetQueryById(), new { Id = request.OrderId });

            if (orderData is null) {

                return new Error() {
                    Title = "Order not found",
                    Details = $"Could not find order in database with given id {request.OrderId}"
                };

            }

            var itemData = connection.Query<AdditionalItemDataModel>(AdditionalItemDataModel.GetQueryByOrderId(), request);
            var items = itemData.Select(i => i.ToDomainModel()).ToList();

            IReadOnlyCollection<IProduct> products;
            try {
                products = GetProducts(request.OrderId, connection);
            } catch (Exception ex) {
                return new Error() {
                    Title = "Failed to load products",
                    Details = ex.Message
                };
            }

            var hardware = GetHardware(request.OrderId, connection);

            var order = orderData.ToDomainModel(request.OrderId, products, items, hardware);

            return order;

        }

        private IReadOnlyCollection<IProduct> GetProducts(Guid orderId, ISynchronousDbConnection connection) {

            List<IProductDataModel> productData = [];

            // add product data to list here
            AddProductDataToCollection<ClosetPartDataModel>(productData, orderId, connection);
            AddProductDataToCollection<CustomDrilledVerticalPanelDataModel>(productData, orderId, connection);
            AddProductDataToCollection<ZargenDrawerDataModel>(productData, orderId, connection);
            AddProductDataToCollection<DovetailDrawerBoxDataModel>(productData, orderId, connection);
            AddProductDataToCollection<DoweledDrawerBoxDataModel>(productData, orderId, connection);
            AddProductDataToCollection<FivePieceDoorDataModel>(productData, orderId, connection);
            AddProductDataToCollection<CounterTopDataModel>(productData, orderId, connection);
            AddMDFDoorProductDataToCollection(productData, orderId, connection);

            AddProductDataToCollection<CabinetPartDataModel>(productData, orderId, connection);
            AddProductDataToCollection<BaseCabinetDataModel>(productData, orderId, connection);
            AddProductDataToCollection<WallCabinetDataModel>(productData, orderId, connection);
            AddProductDataToCollection<DrawerBaseCabinetDataModel>(productData, orderId, connection);
            AddProductDataToCollection<TallCabinetDataModel>(productData, orderId, connection);
            AddProductDataToCollection<SinkCabinetDataModel>(productData, orderId, connection);
            AddProductDataToCollection<TrashCabinetDataModel>(productData, orderId, connection);
            AddProductDataToCollection<DiagonalBaseCabinetDataModel>(productData, orderId, connection);
            AddProductDataToCollection<DiagonalWallCabinetDataModel>(productData, orderId, connection);
            AddProductDataToCollection<PieCutWallCabinetDataModel>(productData, orderId, connection);
            AddProductDataToCollection<PieCutBaseCabinetDataModel>(productData, orderId, connection);
            AddProductDataToCollection<BlindWallCabinetDataModel>(productData, orderId, connection);
            AddProductDataToCollection<BlindBaseCabinetDataModel>(productData, orderId, connection);

            return productData.Aggregate(new List<IProduct>(), ProductAggregator).ToList();

        }

        private void AddProductDataToCollection<T>(List<IProductDataModel> productData, Guid orderId, ISynchronousDbConnection connection) where T : IQueryableProductDataModel {

            try {

                string query = T.GetQueryByOrderId;
                var data = connection.Query<T>(query, new { OrderId = orderId });
                productData.AddRange(data.Cast<IProductDataModel>());

            } catch (Exception ex) {
                _logger.LogError(ex, "Exception thrown while trying to read data for product type {Type} in order {OrderId}", typeof(T), orderId);
                throw;
            }

        }

        private void AddMDFDoorProductDataToCollection(List<IProductDataModel> productData, Guid orderId, ISynchronousDbConnection connection) {

            try {

                var query = MDFDoorDataModel.GetQueryByOrderId;
                var doors = connection.Query<MDFDoorDataModel>(query, new { OrderId = orderId });

                string openingsQuery = MDFDoorDataModel.GetAdditionalOpeningsQueryByProductId;
                foreach (var door in doors) {
                    var data = connection.Query<AdditionalOpening>(openingsQuery, new { ProductId = door.Id });
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

        private static Hardware GetHardware(Guid orderId, ISynchronousDbConnection connection) {

            var suppliesRepo = new OrderSuppliesRepository(connection);
            var supplies = suppliesRepo.GetOrderSupplies(orderId);

            var slidesRepo = new OrderDrawerSlidesRepository(connection);
            var slides = slidesRepo.GetOrderDrawerSlides(orderId);

            var railsRepo = new OrderHangingRailRepository(connection);
            var hangingRails = railsRepo.GetOrderHangingRails(orderId);

            return new(supplies.ToArray(), slides.ToArray(), hangingRails.ToArray());

        }

    }

}
