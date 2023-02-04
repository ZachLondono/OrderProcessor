using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Features.Orders.Shared.Domain.Notifications;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Infrastructure;
using ApplicationCore.Infrastructure.Data;
using Dapper;
using System.Data;
using System.Diagnostics;

namespace ApplicationCore.Features.Orders.Loader.Commands;
public class CreateNewOrder {

    public record Command(string Source, string Number, string Name, Customer Customer, Guid VendorId, string Comment, DateTime OrderDate, ShippingInfo Shipping, BillingInfo Billing, decimal Tax, decimal PriceAdjustment, bool Rush, IReadOnlyDictionary<string, string> Info, IEnumerable<IProduct> Products, IEnumerable<AdditionalItem> AdditionalItems, Guid? OrderId = null) : ICommand<Order>;

    public class Handler : CommandHandler<Command, Order> {

        private readonly IDbConnectionFactory _factory;
        private readonly IBus _bus;

        public Handler(IDbConnectionFactory factory, IBus bus) {
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
                const string shippingAddressCommand = @"INSERT INTO shippingaddresses (id, line1, line2, line3, city, state, zip, country)
                                                        VALUES (@Id, @Line1, @Line2, @Line3, @City, @State, @Zip, @Country);";
                await connection.ExecuteAsync(shippingAddressCommand, new {
                    Id = shippingAddressId,
                    order.Shipping.Address.Line1,
                    order.Shipping.Address.Line2,
                    order.Shipping.Address.Line3,
                    order.Shipping.Address.City,
                    order.Shipping.Address.State,
                    order.Shipping.Address.Zip,
                    order.Shipping.Address.Country,
                });

                Guid billingAddressId = Guid.NewGuid();
                const string billingAddressCommand = @"INSERT INTO billingaddresses (id, line1, line2, line3, city, state, zip, country)
                                                        VALUES (@Id, @Line1, @Line2, @Line3, @City, @State, @Zip, @Country);";
                await connection.ExecuteAsync(billingAddressCommand, new {
                    Id = billingAddressId,
                    order.Billing.Address.Line1,
                    order.Billing.Address.Line2,
                    order.Billing.Address.Line3,
                    order.Billing.Address.City,
                    order.Billing.Address.State,
                    order.Billing.Address.Zip,
                    order.Billing.Address.Country,
                });

                const string command = @"INSERT INTO orders (id, source, number, name, vendorid, customername, customercomment, orderdate, info, tax, priceadjustment, rush, shippingmethod, shippingprice, shippingcontact, shippingphonenumber, shippingaddressid, invoiceemail, billingphonenumber, billingaddressid)
                                        VALUES (@Id, @Source, @Number, @Name, @VendorId, @CustomerName, @CustomerComment, @OrderDate, @Info, @Tax, @PriceAdjustment, @Rush, @ShippingMethod, @ShippingPrice, @ShippingContact, @ShippingPhoneNumber, @ShippingAddressId, @InvoiceEmail, @BillingPhoneNumber, @BillingAddressId);";

                await connection.ExecuteAsync(command, new {
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

                foreach (var item in request.AdditionalItems) {

                    await CreateItem(item, order.Id, connection, trx);

                }

                trx.Commit();

            } catch (Exception ex) {
                trx.Rollback();
                Debug.WriteLine(ex);
            } finally {
                connection.Close();
            }

            await _bus.Publish(new OrderCreatedNotification() {
                Order = order
            });

            return new(order);

        }

        private static async Task CreateItem(AdditionalItem item, Guid orderId, IDbConnection connection, IDbTransaction transaction) {

            const string itemCommand = @"INSERT INTO additionalitems (id, orderid, description, price)
                                        VALUES (@Id, @OrderId, @Description, @Price)";

            await connection.ExecuteAsync(itemCommand, new {
                item.Id,
                OrderId = orderId,
                item.Description,
                item.Price
            }, transaction);

        }

    }

}
