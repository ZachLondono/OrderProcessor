using Domain.Companies.Entities;
using Domain.Companies.ValueObjects;
using ApplicationCore.Shared.Data.Companies;
using ApplicationCore.Infrastructure.Bus;
using Dapper;
using Domain.ValueObjects;

namespace ApplicationCore.Features.Companies.Customers.Queries;

internal class GetCustomerById {

    public record Query(Guid Id) : IQuery<Customer?>;

    public class Handler : QueryHandler<Query, Customer?> {

        private readonly ICompaniesDbConnectionFactory _factory;

        public Handler(ICompaniesDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response<Customer?>> Handle(Query query) {

            using var connection = await _factory.CreateConnection();

            var data = await connection.QuerySingleOrDefaultAsync<CustomerDataModel>(
                """
                SELECT
                    
                    customers.id,
                    customers.name,
                    customers.order_number_prefix AS OrderNumberPrefix,
                    customers.shipping_method AS ShippingMethod,
                    customers.working_directory_root AS WorkingDirectoryRoot,

                    ship_cont.name AS ShippingContactName,
                    ship_cont.phone_number AS ShippingContactPhone,
                    ship_cont.email  AS ShippingContactEmail,
                    ship_addr.line1  AS ShippingAddrLine1,
                    ship_addr.line2  AS ShippingAddrLine2,
                    ship_addr.line3  AS ShippingAddrLine3,
                    ship_addr.city  AS ShippingAddrCity,
                    ship_addr.state  AS ShippingAddrState,
                    ship_addr.zip  AS ShippingAddrZip,
                    ship_addr.country  AS ShippingAddrCountry,
                    
                    billing_cont.name AS BillingContactName,
                    billing_cont.phone_number AS BillingContactPhone,
                    billing_cont.email AS BillingContactEmail,
                    billing_addr.line1 AS BillingAddrLine1,
                    billing_addr.line2 AS BillingAddrLine2,
                    billing_addr.line3 AS BillingAddrLine3,
                    billing_addr.city AS BillingAddrCity,
                    billing_addr.state AS BillingAddrState,
                    billing_addr.zip AS BillingAddrZip,
                    billing_addr.country AS BillingAddrCountry,

                    closet_pro_settings.toe_kick_sku AS CPToeKickSKU,
                    closet_pro_settings.adjustable_shelf_sku AS CPAdjustableShelfSKU,
                    closet_pro_settings.fixed_shelf_sku AS CPFixedShelfSKU,
                    closet_pro_settings.l_fixed_shelf_sku AS CPLFixedShelfSKU,
                    closet_pro_settings.l_adjustable_shelf_sku AS CPLAdjustableShelfSKU,
                    closet_pro_settings.l_shelf_radius AS CPLShelfRadius,
                    closet_pro_settings.diagonal_fixed_shelf_sku AS CPDiagonalFixedShelfSKU,
                    closet_pro_settings.diagonal_adjustable_shelf_sku AS CPDiagonalAdjustableShelfSKU,
                    closet_pro_settings.doweled_drawer_box_material_finish AS CPDoweledDrawerBoxMaterialFinish,
                    closet_pro_settings.vertical_panel_bottom_radius AS CPVerticalPanelBottomRadius

                FROM customers
                    
                    LEFT JOIN addresses AS ship_addr ON customers.shipping_address_id = ship_addr.id
                    LEFT JOIN addresses AS billing_addr ON customers.billing_address_id = billing_addr.id
                    
                    LEFT JOIN contacts AS ship_cont ON customers.shipping_contact_id = ship_cont.id
                    LEFT JOIN contacts AS billing_cont ON customers.billing_contact_id = billing_cont.id

                    LEFT JOIN closet_pro_settings ON customers.closet_pro_settings_id = closet_pro_settings.id

                WHERE customers.id = @Id;
                """, query);

            if (data is null) {
                return (Customer?)null;
            }

            var customer = data.AsCustomer();

            return customer;

        }

        public class CustomerDataModel {

            public Guid Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string? OrderNumberPrefix { get; set; } = null;
            public string ShippingMethod { get; set; } = string.Empty;
            public string? WorkingDirectoryRoot { get; set; } = null;

            public string ShippingContactName { get; set; } = string.Empty;
            public string? ShippingContactPhone { get; set; }
            public string? ShippingContactEmail { get; set; }

            public string ShippingAddrLine1 { get; set; } = string.Empty;
            public string ShippingAddrLine2 { get; set; } = string.Empty;
            public string ShippingAddrLine3 { get; set; } = string.Empty;
            public string ShippingAddrCity { get; set; } = string.Empty;
            public string ShippingAddrState { get; set; } = string.Empty;
            public string ShippingAddrZip { get; set; } = string.Empty;
            public string ShippingAddrCountry { get; set; } = string.Empty;

            public string BillingContactName { get; set; } = string.Empty;
            public string? BillingContactPhone { get; set; }
            public string? BillingContactEmail { get; set; }

            public string BillingAddrLine1 { get; set; } = string.Empty;
            public string BillingAddrLine2 { get; set; } = string.Empty;
            public string BillingAddrLine3 { get; set; } = string.Empty;
            public string BillingAddrCity { get; set; } = string.Empty;
            public string BillingAddrState { get; set; } = string.Empty;
            public string BillingAddrZip { get; set; } = string.Empty;
            public string BillingAddrCountry { get; set; } = string.Empty;

            public string CPToeKickSKU { get; set; } = string.Empty;
            public string CPAdjustableShelfSKU { get; set; } = string.Empty;
            public string CPFixedShelfSKU { get; set; } = string.Empty;
            public string CPLFixedShelfSKU { get; set; } = string.Empty;
            public string CPLAdjustableShelfSKU { get; set; } = string.Empty;
            public Dimension CPLShelfRadius { get; set; } = Dimension.Zero;
            public string CPDiagonalFixedShelfSKU { get; set; } = string.Empty;
            public string CPDiagonalAdjustableShelfSKU { get; set; } = string.Empty;
            public string CPDoweledDrawerBoxMaterialFinish { get; set; } = string.Empty;
            public Dimension CPVerticalPanelBottomRadius { get; set; } = Dimension.Zero;

            public Customer AsCustomer() {

                var shippingContact = new Contact() {
                    Name = ShippingContactName,
                    Phone = ShippingContactPhone,
                    Email = ShippingContactEmail
                };

                var shippingAddress = new Address() {
                    Line1 = ShippingAddrLine1,
                    Line2 = ShippingAddrLine2,
                    Line3 = ShippingAddrLine3,
                    City = ShippingAddrCity,
                    State = ShippingAddrState,
                    Zip = ShippingAddrZip,
                    Country = ShippingAddrCountry
                };

                var billingContact = new Contact() {
                    Name = BillingContactName,
                    Phone = BillingContactPhone,
                    Email = BillingContactEmail
                };

                var billingAddress = new Address() {
                    Line1 = BillingAddrLine1,
                    Line2 = BillingAddrLine2,
                    Line3 = BillingAddrLine3,
                    City = BillingAddrCity,
                    State = BillingAddrState,
                    Zip = BillingAddrZip,
                    Country = BillingAddrCountry
                };

                var closetProSettings = new ClosetProSettings() {
                    ToeKickSKU = CPToeKickSKU,
                    AdjustableShelfSKU = CPAdjustableShelfSKU,
                    FixedShelfSKU = CPFixedShelfSKU,
                    LFixedShelfSKU = CPLFixedShelfSKU,
                    LAdjustableShelfSKU = CPLAdjustableShelfSKU,
                    LShelfRadius = CPLShelfRadius,
                    DiagonalFixedShelfSKU = CPDiagonalFixedShelfSKU,
                    DiagonalAdjustableShelfSKU = CPDiagonalAdjustableShelfSKU,
                    DoweledDrawerBoxMaterialFinish = CPDoweledDrawerBoxMaterialFinish,
                    VerticalPanelBottomRadius = CPVerticalPanelBottomRadius
                };

                return new Customer(Id, Name, ShippingMethod, shippingContact, shippingAddress, billingContact, billingAddress, OrderNumberPrefix, closetProSettings, WorkingDirectoryRoot);

            }

        }

    }

}
