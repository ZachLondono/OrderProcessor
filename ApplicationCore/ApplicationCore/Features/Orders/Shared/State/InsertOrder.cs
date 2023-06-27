using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Features.Orders.Shared.Domain.Notifications;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Shared.Data.Ordering;
using ApplicationCore.Infrastructure.Bus;
using Dapper;
using Microsoft.Extensions.Logging;
using System.Data;

namespace ApplicationCore.Features.Orders.Shared.State;

public partial class InsertOrder {

    public record Command(Order NewOrder) : ICommand;

    public partial class Handler : CommandHandler<Command> {

        private readonly ILogger<Handler> _logger;
        private readonly IOrderingDbConnectionFactory _factory;
        private readonly IBus _bus;

        public Handler(ILogger<Handler> logger, IOrderingDbConnectionFactory factory, IBus bus) {
            _logger = logger;
            _factory = factory;
            _bus = bus;
        }

        public override async Task<Response> Handle(Command request) {

            Order order = request.NewOrder;

            using var connection = await _factory.CreateConnection();

            connection.Open();
            var trx = connection.BeginTransaction();

            try {

                Guid shippingAddressId = Guid.NewGuid();
                await InsertAddress(order.Shipping.Address, shippingAddressId, connection, trx);

                Guid billingAddressId = Guid.NewGuid();
                await InsertAddress(order.Billing.Address, billingAddressId, connection, trx);

                await InsertOrder(order, shippingAddressId, billingAddressId, connection, trx);

                await InsertItems(order.AdditionalItems, order.Id, connection, trx);

                await InsertProducts(order.Products, order.Id, connection, trx);

                trx.Commit();

                await _bus.Publish(new OrderCreatedNotification() {
                    Order = order
                });

                return Response.Success();

            } catch (Exception ex) {

                trx.Rollback();
                _logger.LogError(ex, "Exception thrown while creating order");

                return Response.Error(new() {
                    Title = "Could not create order",
                    Details = $"An exception was thrown while trying to create order, check logs for details - {ex.Message}"
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
                    note,
                    working_directory,
                    vendor_id,
                    customer_id,
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
                    @Note,
                    @WorkingDirectory,
                    @VendorId,
                    @CustomerId,
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
                    order.Note,
                    order.WorkingDirectory,
                    order.VendorId,
                    order.CustomerId,
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

                const string itemCommand = @"INSERT INTO additional_items (id, order_id, description, price)
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