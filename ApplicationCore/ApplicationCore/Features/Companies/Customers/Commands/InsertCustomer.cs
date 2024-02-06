using Domain.Companies.Entities;
using Domain.Companies.ValueObjects;
using ApplicationCore.Shared.Data.Companies;
using ApplicationCore.Infrastructure.Bus;
using Dapper;
using System.Data;

namespace Domain.Companies.Customers.Commands;

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
            var cpSettingsId = await InsertClosetProSettings(customer.ClosetProSettings, connection, trx);

            await InsertCustomer(customer, shippingContactId, shippingAddressId, billingContactId, billingAddressId, cpSettingsId, connection, trx);

            if (command.AllmoxyId is int allmoxyId) {
                await InsertAllmoxyId(customer.Id, allmoxyId, connection, trx);
            }

            trx.Commit();
            connection.Close();

            return Response.Success();

        }

        private static async Task InsertCustomer(Customer customer, Guid shippingContactId, Guid shippingAddressId, Guid billingContactId, Guid billingAddressId, Guid closetProSettingsId, IDbConnection connection, IDbTransaction trx) {
            int rows = await connection.ExecuteAsync(
                    """
                    INSERT INTO customers (
                        id,
                        name,
                        order_number_prefix,
                        shipping_method,
                        shipping_contact_id,
                        shipping_address_id,
                        billing_contact_id,
                        billing_address_id,
                        closet_pro_settings_id,
                        working_directory_root)
                    VALUES (
                        @Id,
                        @Name,
                        @OrderNumberPrefix,
                        @ShippingMethod,
                        @ShippingContactId,
                        @ShippingAddressId,
                        @BillingContactId,
                        @BillingAddressId,
                        @ClosetProSettingsId,
                        @WorkingDirectoryRoot
                    );
                    """, new {
                        customer.Id,
                        customer.Name,
                        customer.OrderNumberPrefix,
                        customer.ShippingMethod,
                        ShippingContactId = shippingContactId,
                        ShippingAddressId = shippingAddressId,
                        BillingContactId = billingContactId,
                        BillingAddressId = billingAddressId,
                        ClosetProSettingsId = closetProSettingsId,
                        customer.WorkingDirectoryRoot
                    }, trx);

            if (rows != 1) {
                throw new InvalidOperationException("Customer record was not saved");
            }

        }

        public static async Task<Guid> InsertContact(Contact contact, IDbConnection connection, IDbTransaction trx) {

            Guid id = Guid.NewGuid();

            int rows = await connection.ExecuteAsync(
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

            if (rows != 1) {
                throw new InvalidOperationException("Address record could not be created for customer");
            }

            return id;

        }

        public static async Task<Guid> InsertAddress(Address address, IDbConnection connection, IDbTransaction trx) {

            Guid id = Guid.NewGuid();

            int rows = await connection.ExecuteAsync(
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

            if (rows != 1) {
                throw new InvalidOperationException("Address record could not be created for customer");
            }

            return id;

        }

        public static async Task InsertAllmoxyId(Guid customerId, int allmoxyId, IDbConnection connection, IDbTransaction trx) {

            int rows = await connection.ExecuteAsync(
                "INSERT INTO allmoxy_ids (id, customer_id) VALUES (@AllmoxyId, @CustomerId)",
                new {
                    CustomerId = customerId,
                    AllmoxyId = allmoxyId
                }, trx);

            if (rows != 1) {
                throw new InvalidOperationException("Allmoxy id record could not be created for customer");
            }

        }

        public static async Task<Guid> InsertClosetProSettings(ClosetProSettings settings, IDbConnection connection, IDbTransaction trx) {

            Guid id = Guid.NewGuid();

            int rows = await connection.ExecuteAsync(
                """
                INSERT INTO closet_pro_settings
                    (id, toe_kick_sku, adjustable_shelf_sku, fixed_shelf_sku, l_fixed_shelf_sku, l_adjustable_shelf_sku, l_shelf_radius, diagonal_fixed_shelf_sku, diagonal_adjustable_shelf_sku, doweled_drawer_box_material_finish, vertical_panel_bottom_radius)
                VALUES
                    (@Id, @ToeKickSKU, @AdjustableShelfSKU, @FixedShelfSKU, @LFixedShelfSKU, @LAdjustableShelfSKU, @LShelfRadius, @DiagonalFixedShelfSKU, @DiagonalAdjustableShelfSKU, @DoweledDrawerBoxMaterialFinish, @VerticalPanelBottomRadius);
                """,
                new {
                    Id = id,
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

            if (rows != 1) {
                throw new InvalidOperationException("Closet pro settings record could not be created for customer");
            }

            return id;

        }

    }

}

