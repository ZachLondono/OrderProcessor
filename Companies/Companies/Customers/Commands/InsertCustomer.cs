using Domain.Companies.Entities;
using Domain.Companies.ValueObjects;
using Domain.Infrastructure.Bus;
using Companies.Infrastructure;
using Domain.Infrastructure.Data;

namespace Companies.Customers.Commands;

public class InsertCustomer {

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

            var billingContactId = InsertContact(customer.BillingContact, connection, trx);
            var billingAddressId = InsertAddress(customer.BillingAddress, connection, trx);
            var shippingContactId = InsertContact(customer.ShippingContact, connection, trx);
            var shippingAddressId = InsertAddress(customer.ShippingAddress, connection, trx);
            var cpSettingsId = InsertClosetProSettings(customer.ClosetProSettings, connection, trx);

            InsertCustomer(customer,
                           shippingContactId,
                           shippingAddressId,
                           billingContactId,
                           billingAddressId,
                           cpSettingsId,
                           connection,
                           trx);

            if (command.AllmoxyId is int allmoxyId) {
                InsertAllmoxyId(customer.Id, allmoxyId, connection, trx);
            }

            trx.Commit();
            connection.Close();

            return Response.Success();

        }

        private static void InsertCustomer(Customer customer,
                                                 Guid shippingContactId,
                                                 Guid shippingAddressId,
                                                 Guid billingContactId,
                                                 Guid billingAddressId,
                                                 Guid closetProSettingsId,
                                                 ISynchronousDbConnection connection,
                                                 ISynchronousDbTransaction trx) {
            int rows = connection.Execute(
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

        public static Guid InsertContact(Contact contact, ISynchronousDbConnection connection, ISynchronousDbTransaction trx) {

            Guid id = Guid.NewGuid();

            int rows = connection.Execute(
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

        public static Guid InsertAddress(Address address, ISynchronousDbConnection connection, ISynchronousDbTransaction trx) {

            Guid id = Guid.NewGuid();

            int rows = connection.Execute(
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

        public static void InsertAllmoxyId(Guid customerId, int allmoxyId, ISynchronousDbConnection connection, ISynchronousDbTransaction trx) {

            int rows = connection.Execute(
                "INSERT INTO allmoxy_ids (id, customer_id) VALUES (@AllmoxyId, @CustomerId)",
                new {
                    CustomerId = customerId,
                    AllmoxyId = allmoxyId
                }, trx);

            if (rows != 1) {
                throw new InvalidOperationException("Allmoxy id record could not be created for customer");
            }

        }

        public static Guid InsertClosetProSettings(ClosetProSettings settings, ISynchronousDbConnection connection, ISynchronousDbTransaction trx) {

            Guid id = Guid.NewGuid();

            int rows = connection.Execute(
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

