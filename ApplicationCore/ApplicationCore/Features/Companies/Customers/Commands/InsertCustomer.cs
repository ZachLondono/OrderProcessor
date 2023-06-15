using ApplicationCore.Features.Companies.Contracts.Entities;
using ApplicationCore.Features.Companies.Contracts.ValueObjects;
using ApplicationCore.Features.Shared.Data.Companies;
using ApplicationCore.Infrastructure.Bus;
using Dapper;
using System.Data;

namespace ApplicationCore.Features.Companies.Customers.Commands;

internal class InsertCustomer {

    public record Command(Customer Customer, int? AllmoxyId) : ICommand;

    public class Handler : CommandHandler<Command> {

        private readonly ICompaniesDbConnectionFactory _factory;

        public Handler(ICompaniesDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response> Handle(Command command) {

            var customer = command.Customer;

            using var connection = await _factory.CreateConnection();

            connection.Open();
            var trx = connection.BeginTransaction();

            var billingContactId = await InsertContact(customer.BillingContact, connection, trx);
            var billingAddressId = await InsertAddress(customer.BillingAddress, connection, trx);
            var shippingContactId = await InsertContact(customer.ShippingContact, connection, trx);
            var shippingAddressId = await InsertAddress(customer.ShippingAddress, connection, trx);

            await InsertCustomer(customer, shippingContactId, shippingAddressId, billingContactId, billingAddressId, connection, trx);

            if (command.AllmoxyId is int allmoxyId) {
                await InsertAllmoxyId(customer.Id, allmoxyId, connection, trx);
            }

            trx.Commit();
            connection.Close();

            return Response.Success();

        }

        private static async Task InsertCustomer(Customer customer, Guid shippingContactId, Guid shippingAddressId, Guid billingContactId, Guid billingAddressId, IDbConnection connection, IDbTransaction trx) {
            await connection.ExecuteAsync(
                    """
                    INSERT INTO customers (
                        id,
                        name,
                        shipping_method,
                        shipping_contact_id,
                        shipping_address_id,
                        billing_contact_id,
                        billing_address_id)
                    VALUES (
                        @Id,
                        @Name,
                        @ShippingMethod,
                        @ShippingContactId,
                        @ShippingAddressId,
                        @BillingContactId,
                        @BillingAddressId
                    );
                    """, new {
                        customer.Id,
                        customer.Name,
                        customer.ShippingMethod,
                        ShippingContactId = shippingContactId,
                        ShippingAddressId = shippingAddressId,
                        BillingContactId = billingContactId,
                        BillingAddressId = billingAddressId
                    }, trx);
        }

        public static async Task<Guid> InsertContact(Contact contact, IDbConnection connection, IDbTransaction trx) {

            Guid id = Guid.NewGuid();

            await connection.ExecuteAsync(
                    $"""
                    INSERT INTO contacts (
                        id,
                        name,
                        phone_number,
                        email)
                    VALUES (
                        @Id,
                        @Name,
                        @Phone,
                        @Email
                    );
                    """, new {
                        Id = id,
                        contact.Name,
                        contact.Phone,
                        contact.Email
                    }, trx);

            return id;

        }

        public static async Task<Guid> InsertAddress(Address address, IDbConnection connection, IDbTransaction trx) {

            Guid id = Guid.NewGuid();

            await connection.ExecuteAsync(
                $"""
                INSERT INTO addresses (
                    id,
                    line1,
                    line2,
                    line3,
                    city,
                    state,
                    zip,
                    country)
                VALUES (
                    @Id,
                    @Line1,
                    @Line2,
                    @Line3,
                    @City,
                    @State,
                    @Zip,
                    @Country
                );
                """, new {
                    Id = id,
                    address.Line1,
                    address.Line2,
                    address.Line3,
                    address.City,
                    address.State,
                    address.Zip,
                    address.Country
                }, trx);

            return id;

        }

        public static async Task InsertAllmoxyId(Guid customerId, int allmoxyId, IDbConnection connection, IDbTransaction trx) {

            await connection.ExecuteAsync(
                "INSERT INTO allmoxy_ids (id, customer_id) VALUES (@AllmoxyId, @CustomerId)",
                new {
                    CustomerId = customerId,
                    AllmoxyId = allmoxyId
                }, trx);


        }

    }

}

