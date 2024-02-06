using Domain.Companies.Entities;
using Domain.Companies.ValueObjects;
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

            await UpdateClosetProSettings(customer.ClosetProSettings, customer.Id, connection, trx);

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
                    order_number_prefix = @OrderNumberPrefix,
                    working_directory_root = @WorkingDirectoryRoot
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

        public static async Task UpdateClosetProSettings(ClosetProSettings settings, Guid customerId, IDbConnection connection, IDbTransaction trx) {

            await connection.ExecuteAsync(
                $"""
                UPDATE closet_pro_settings
                SET
                    toe_kick_sku = @ToeKickSKU,
                    adjustable_shelf_sku = @AdjustableShelfSKU,
                    fixed_shelf_sku = @FixedShelfSKU,
                    l_fixed_shelf_sku = @LFixedShelfSKU,
                    l_adjustable_shelf_sku = @LAdjustableShelfSKU,
                    l_shelf_radius = @LShelfRadius,
                    diagonal_fixed_shelf_sku = @DiagonalFixedShelfSKU,
                    diagonal_adjustable_shelf_sku = @DiagonalAdjustableShelfSKU,
                    doweled_drawer_box_material_finish = @DoweledDrawerBoxMaterialFinish,
                    vertical_panel_bottom_radius = @VerticalPanelBottomRadius
                WHERE id = (SELECT closet_pro_settings_id FROM customers WHERE id = @CustomerId);
                """, new {
                    CustomerId = customerId,
                    settings.ToeKickSKU,
                    settings.AdjustableShelfSKU,
                    settings.FixedShelfSKU,
                    settings.LFixedShelfSKU,
                    settings.LAdjustableShelfSKU,
                    settings.LShelfRadius,
                    settings.DiagonalFixedShelfSKU,
                    settings.DiagonalAdjustableShelfSKU,
                    settings.DoweledDrawerBoxMaterialFinish,
                    settings.VerticalPanelBottomRadius
                }, trx);

        }

    }

}

