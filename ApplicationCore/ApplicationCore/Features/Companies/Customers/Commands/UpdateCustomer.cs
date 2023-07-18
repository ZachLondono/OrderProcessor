using ApplicationCore.Features.Companies.Contracts.Entities;
using ApplicationCore.Features.Companies.Contracts.ValueObjects;
using ApplicationCore.Shared.Data.Companies;
using ApplicationCore.Infrastructure.Bus;
using Dapper;
using System.Data;

namespace ApplicationCore.Features.Companies.Customers.Commands;

internal class UpdateCustomer {

    public record Command(Customer Customer) : ICommand;

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

            await UpdateCustomer(customer, connection, trx);

            await UpdateContact(customer.BillingContact, customer.Id, "billing_contact_id", connection, trx);
            await UpdateAddress(customer.BillingAddress, customer.Id, "billing_address_id", connection, trx);

            await UpdateContact(customer.ShippingContact, customer.Id, "shipping_contact_id", connection, trx);
            await UpdateAddress(customer.ShippingAddress, customer.Id, "shipping_address_id", connection, trx);

            trx.Commit();
            connection.Close();

            return Response.Success();

        }

        private static async Task UpdateCustomer(Customer customer, IDbConnection connection, IDbTransaction trx) {
            await connection.ExecuteAsync(
                """
                UPDATE customers
                SET
                    name = @Name,
                    shipping_method = @ShippingMethod,
                    order_number_prefix = @OrderNumberPrefix
                WHERE id = @Id;
                """, customer, trx);
        }

        public static async Task UpdateContact(Contact contact, Guid customerId, string idColName, IDbConnection connection, IDbTransaction trx) {

            await connection.ExecuteAsync(
                $"""
                UPDATE contacts
                SET
                    name = @Name,
                    phone_number = @Phone,
                    email = @Email
                WHERE id = (SELECT {idColName} FROM customers WHERE id = @CustomerId);
                """, new {
                    contact.Name,
                    contact.Phone,
                    contact.Email,
                    CustomerId = customerId
                }, trx);

        }

        public static async Task UpdateAddress(Address address, Guid customerId, string idColName, IDbConnection connection, IDbTransaction trx) {

            await connection.ExecuteAsync(
                $"""
                UPDATE addresses
                SET
                    line1 = @Line1,
                    line2 = @Line2,
                    line3 = @Line3,
                    city = @City,
                    state = @State,
                    zip = @Zip,
                    country = @Country
                WHERE id = (SELECT {idColName} FROM customers WHERE id = @CustomerId);
                """, new {
                    address.Line1,
                    address.Line2,
                    address.Line3,
                    address.City,
                    address.State,
                    address.Zip,
                    address.Country,
                    CustomerId = customerId
                }, trx);

        }

    }

}

