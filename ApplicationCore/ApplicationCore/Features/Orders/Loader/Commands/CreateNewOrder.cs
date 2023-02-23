using ApplicationCore.Features.Orders.Data;
using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Features.Orders.Shared.Domain.Notifications;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Infrastructure.Bus;
using Dapper;
using Microsoft.Extensions.Logging;
using System.Data;

namespace ApplicationCore.Features.Orders.Loader.Commands;

public partial class CreateNewOrder {

    public record Command(string Source, string Number, string Name, Customer Customer, Guid VendorId, string Comment, DateTime OrderDate, ShippingInfo Shipping, BillingInfo Billing, decimal Tax, decimal PriceAdjustment, bool Rush, IReadOnlyDictionary<string, string> Info, IEnumerable<IProduct> Products, IEnumerable<AdditionalItem> AdditionalItems, Guid? OrderId = null) : ICommand<Order>;

    public partial class Handler : CommandHandler<Command, Order> {

        private readonly ILogger<Handler> _logger;
        private readonly IOrderingDbConnectionFactory _factory;
        private readonly IBus _bus;

        public Handler(ILogger<Handler> logger, IOrderingDbConnectionFactory factory, IBus bus) {
            _logger = logger;
            _factory = factory;
            _bus = bus;
        }

        public override async Task<Response<Order>> Handle(Command request) {

            Order order = Order.Create(request.Source, request.Number, request.Name, request.Customer, request.VendorId, request.Comment, request.OrderDate, request.Shipping, request.Billing, request.Tax, request.PriceAdjustment, request.Rush, request.Info, request.Products, request.AdditionalItems, request.OrderId);

            using var connection = _factory.CreateConnection();

            connection.Open();
            var trx = connection.BeginTransaction();

            try {

                Guid shippingAddressId = Guid.NewGuid();
                await InsertAddress(order.Shipping.Address, shippingAddressId, connection, trx);

                Guid billingAddressId = Guid.NewGuid();
                await InsertAddress(order.Billing.Address, billingAddressId, connection, trx);

                await InsertOrder(order, shippingAddressId, billingAddressId, connection, trx);

                await InsertItems(request.AdditionalItems, order.Id, connection, trx);

                await InsertProducts(order.Products, order.Id, connection, trx);

                trx.Commit();

                await _bus.Publish(new OrderCreatedNotification() {
                    Order = order
                });

                return new(order);

            } catch (Exception ex) {
                
                trx.Rollback();
                _logger.LogError(ex, "Exception thrown while creating order");

                return Response<Order>.Error(new() {
                    Title = "Could not create order",
                    Details = $"An exception was thrown while trying to create order, check logs for detauls - {ex.Message}"
                });

            } finally {
                connection.Close();
            }        

        }

        private static async Task InsertOrder(Order order, Guid shippingAddressId, Guid billingAddressId, IDbConnection connection, IDbTransaction trx) {
            await connection.ExecuteAsync(
                """
                INSERT INTO orders
                    (id,
                    source,
                    number,
                    name,
                    vendor_id,
                    customer_name,
                    customer_comment,
                    order_date,
                    info,
                    tax,
                    price_adjustment,
                    rush,
                    shipping_method,
                    shipping_price,
                    shipping_contact,
                    shipping_phone_number,
                    shipping_address_id,
                    invoice_email,
                    billing_phone_number,
                    billing_address_id)
                VALUES
                    (@Id,
                    @Source,
                    @Number,
                    @Name,
                    @VendorId,
                    @CustomerName,
                    @CustomerComment,
                    @OrderDate,
                    @Info,
                    @Tax,
                    @PriceAdjustment,
                    @Rush,
                    @ShippingMethod,
                    @ShippingPrice,
                    @ShippingContact,
                    @ShippingPhoneNumber,
                    @ShippingAddressId,
                    @InvoiceEmail,
                    @BillingPhoneNumber,
                    @BillingAddressId);
                """,
                new {
                    order.Id,
                    order.Source,
                    order.Name,
                    order.Number,
                    order.VendorId,
                    CustomerName = order.Customer.Name,
                    order.CustomerComment,
                    order.OrderDate,
                    Info = (IDictionary<string, string>)order.Info,
                    order.Tax,
                    order.PriceAdjustment,
                    order.Rush,
                    ShippingMethod = order.Shipping.Method,
                    ShippingPrice = order.Shipping.Price,
                    ShippingContact = order.Shipping.Contact,
                    ShippingPhoneNumber = order.Shipping.PhoneNumber,
                    ShippingAddressId = shippingAddressId,
                    order.Billing.InvoiceEmail,
                    BillingPhoneNumber = order.Billing.PhoneNumber,
                    BillingAddressId = billingAddressId
                }, trx);
        }

        private static async Task InsertAddress(Address address, Guid id, IDbConnection connection, IDbTransaction trx) {

            string addressInsertQuery = @"INSERT INTO addresses (id, line1, line2, line3, city, state, zip, country)
                                            VALUES (@Id, @Line1, @Line2, @Line3, @City, @State, @Zip, @Country);";

            await connection.ExecuteAsync(addressInsertQuery, new {
                Id = id,
                address.Line1,
                address.Line2,
                address.Line3,
                address.City,
                address.State,
                address.Zip,
                address.Country,
            }, trx);

        }

        private static async Task InsertItems(IEnumerable<AdditionalItem> items, Guid orderId, IDbConnection connection, IDbTransaction trx) {

            foreach (var item in items) {

                const string itemCommand = @"INSERT INTO additional_items (id, orderid, description, price)
                                        VALUES (@Id, @OrderId, @Description, @Price);";

                await connection.ExecuteAsync(itemCommand, new {
                    item.Id,
                    OrderId = orderId,
                    item.Description,
                    item.Price
                }, trx);

            }

        }

        private async Task InsertProducts(IEnumerable<IProduct> products, Guid orderId, IDbConnection connection, IDbTransaction trx) {

            foreach (var product in products) {

                await InsertProduct((dynamic)product, orderId, connection, trx);

            }

        }

        private Task InsertProduct(object unknown, Guid orderId, IDbConnection connection, IDbTransaction trx) {
            _logger.LogCritical("No insert method for product type {Type}", unknown.GetType());
            return Task.CompletedTask;
        }

    }

}

