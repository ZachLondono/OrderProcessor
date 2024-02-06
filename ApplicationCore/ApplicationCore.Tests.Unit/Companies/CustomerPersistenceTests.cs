using Domain.Companies.Entities;
using Domain.Companies.ValueObjects;
using Domain.Companies.Customers.Commands;
using Domain.Companies.Customers.Queries;
using ApplicationCore.Shared.Data;
using ApplicationCore.Shared.Data.Companies;
using Domain.ValueObjects;
using Dapper;
using FluentAssertions;
using System.Data;

namespace ApplicationCore.Tests.Unit.Companies;

public class CustomerPersistenceTests {

    private readonly InsertCustomer.Handler _sut;
    private readonly ICompaniesDbConnectionFactory _factory = new TestCompaniesConnectionFactory("./Application/Schemas/companies_schema.sql");

    public CustomerPersistenceTests() {
        _sut = new(_factory);
        SqlMapping.AddSqlMaps();
    }

    [Fact]
    public void Should_InsertCustomer_WithoutAllmoxyId() {

        // Arrange
        var customer = new Customer(Guid.Empty, "Customer Name", "Shipping Method", new(), new(), new(), new(), null, new(), null);

        var connection = _factory.CreateConnection().Result;
        int startingCustomersCount = GetTableRowCount(connection, "customers");
        int startingContactsCount = GetTableRowCount(connection, "contacts");
        int startingAddressCount = GetTableRowCount(connection, "addresses");
        int startingCPSettingsCount = GetTableRowCount(connection, "closet_pro_settings");
        int startingAllmoxyIdsCount = GetTableRowCount(connection, "allmoxy_ids");

        // Act
        var result = _sut.Handle(new(customer, null)).Result;

        // Assert
        result.IsSuccess.Should().BeTrue();

        GetTableRowCount(connection, "customers").Should().Be(startingCustomersCount + 1);
        GetTableRowCount(connection, "contacts").Should().Be(startingContactsCount + 2);
        GetTableRowCount(connection, "addresses").Should().Be(startingAddressCount + 2);
        GetTableRowCount(connection, "closet_pro_settings").Should().Be(startingCPSettingsCount + 1);
        GetTableRowCount(connection, "allmoxy_ids").Should().Be(startingAllmoxyIdsCount);

    }

    [Fact]
    public void Should_InsertCustomer_WithAllmoxyId() {

        // Arrange
        var customer = new Customer(Guid.Empty, "Customer Name", "Shipping Method", new(), new(), new(), new(), null, new(), null);
        int allmoxyId = 123;

        var connection = _factory.CreateConnection().Result;
        int startingCustomersCount = GetTableRowCount(connection, "customers");
        int startingContactsCount = GetTableRowCount(connection, "contacts");
        int startingAddressCount = GetTableRowCount(connection, "addresses");
        int startingCPSettingsCount = GetTableRowCount(connection, "closet_pro_settings");
        int startingAllmoxyIdsCount = GetTableRowCount(connection, "allmoxy_ids");

        // Act
        var result = _sut.Handle(new(customer, allmoxyId)).Result;

        // Assert
        result.IsSuccess.Should().BeTrue();

        GetTableRowCount(connection, "customers").Should().Be(startingCustomersCount + 1);
        GetTableRowCount(connection, "contacts").Should().Be(startingContactsCount + 2);
        GetTableRowCount(connection, "addresses").Should().Be(startingAddressCount + 2);
        GetTableRowCount(connection, "closet_pro_settings").Should().Be(startingCPSettingsCount + 1);
        GetTableRowCount(connection, "allmoxy_ids").Should().Be(startingAllmoxyIdsCount + 1);

        var queryResult = connection.QueryFirst("SELECT * FROM allmoxy_ids WHERE id = @Id", new { Id = allmoxyId });
        ((int)queryResult.id).Should().Be(allmoxyId);
        ((string)queryResult.customer_id).Should().Be(customer.Id.ToString());

    }

    [Fact]
    public void InsertCustomer_ShouldInsertAllData() {

        // Arrange
        var customer = new Customer(Guid.NewGuid(), "Customer Name", "Shipping Method", new(), new(), new(), new(), "Order Number Prefix", new(), "Working Directory Root");

        var connection = _factory.CreateConnection().Result;

        // Act
        _ = _sut.Handle(new(customer, null)).Result;

        // Assert
        var queryResult = connection.QueryFirst("SELECT * FROM customers WHERE id = @Id", new { Id = customer.Id });

        ((string)queryResult.id).Should().Be(customer.Id.ToString());
        ((string)queryResult.name).Should().Be(customer.Name);
        ((string)queryResult.order_number_prefix).Should().Be(customer.OrderNumberPrefix);
        ((string)queryResult.shipping_method).Should().Be(customer.ShippingMethod);
        ((string?)queryResult.working_directory_root).Should().Be(customer.WorkingDirectoryRoot);

    }

    [Fact]
    public void GetCustomerByIdShouldLoadCustomerData() {

        // Arrange
        var customer = new Customer(Guid.NewGuid(),
                                    "Customer Name",
                                    "Shipping Method",
                                    new() {
                                        Name = "Contact Name 1",
                                        Email = "Contact Email 1",
                                        Phone = "Contact Phone 1"
                                    },
                                    new() {
                                        Line1 = "Line 1.1",
                                        Line2 = "Line 2.1",
                                        Line3 = "Line 3.1",
                                        City = "City 1",
                                        State = "State 1",
                                        Zip = "Zip 1",
                                        Country = "Country 1"
                                    },
                                    new() {
                                        Name = "Contact Name 2",
                                        Email = "Contact Email 2",
                                        Phone = "Contact Phone 2"
                                    },
                                    new() {
                                        Line1 = "Line 1.2",
                                        Line2 = "Line 2.2",
                                        Line3 = "Line 3.2",
                                        City = "City 2",
                                        State = "State 2",
                                        Zip = "Zip 2",
                                        Country = "Country 2"
                                    },
                                    "Order Number Prefix",
                                    new() {
                                        AdjustableShelfSKU = "SA5",
                                        DiagonalAdjustableShelfSKU = "ABC",
                                        LAdjustableShelfSKU = "DEF",
                                        DiagonalFixedShelfSKU = "GHI",
                                        FixedShelfSKU = "JKL",
                                        LFixedShelfSKU = "MNO",
                                        ToeKickSKU = "PQR",
                                        DoweledDrawerBoxMaterialFinish = "Black",
                                        LShelfRadius = Dimension.FromInches(2),
                                        VerticalPanelBottomRadius = Dimension.FromInches(2)
                                    },
                                    "Working Directory Root");

        // Act
        _ = _sut.Handle(new(customer, null)).Result;

        // Assert
        var handler = new GetCustomerById.Handler(_factory);
        var result = handler.Handle(new(customer.Id)).Result;

        result.IsSuccess.Should().BeTrue();

        result.Value.Should().BeEquivalentTo(customer);

    }

    [Fact]
    public void InsertContact_ShouldInsertAllData() {

        // Arrange
        var contact = new Contact() {
            Name = "Contact Name",
            Email = "Contact Email",
            Phone = "Contact Phone"
        };

        var connection = _factory.CreateConnection().Result;
        var trx = connection.BeginTransaction();

        // Act
        var id = InsertCustomer.Handler.InsertContact(contact, connection, trx).Result;
        trx.Commit();

        // Assert
        var queryResult = connection.QueryFirst("SELECT * FROM contacts WHERE id = @Id", new { Id = id });
        ((string)queryResult.id).Should().Be(id.ToString());
        ((string)queryResult.name).Should().Be(contact.Name.ToString());
        ((string)queryResult.phone_number).Should().Be(contact.Phone.ToString());
        ((string)queryResult.email).Should().Be(contact.Email.ToString());

    }

    [Fact]
    public void InsertAddress_ShouldInsertAllData() {

        // Arrange
        var address = new Address() {
            Line1 = "Line 1",
            Line2 = "Line 2",
            Line3 = "Line 3",
            City = "City",
            State = "State",
            Zip = "Zip",
            Country = "Country"
        };

        var connection = _factory.CreateConnection().Result;
        var trx = connection.BeginTransaction();

        // Act
        var id = InsertCustomer.Handler.InsertAddress(address, connection, trx).Result;
        trx.Commit();

        // Assert
        var queryResult = connection.QueryFirst("SELECT * FROM addresses WHERE id = @Id", new { Id = id });
        ((string)queryResult.id).Should().Be(id.ToString());
        ((string)queryResult.line1).Should().Be(address.Line1.ToString());
        ((string)queryResult.line2).Should().Be(address.Line2.ToString());
        ((string)queryResult.line3).Should().Be(address.Line3.ToString());
        ((string)queryResult.city).Should().Be(address.City.ToString());
        ((string)queryResult.state).Should().Be(address.State.ToString());
        ((string)queryResult.zip).Should().Be(address.Zip.ToString());
        ((string)queryResult.country).Should().Be(address.Country.ToString());

    }

    [Fact]
    public void InsertClosetProSettings_ShouldInsertAllData() {

        // Arrange
        var settings = new ClosetProSettings() {
            AdjustableShelfSKU = "SA5",
            DiagonalAdjustableShelfSKU = "ABC",
            LAdjustableShelfSKU = "DEF",
            DiagonalFixedShelfSKU = "GHI",
            FixedShelfSKU = "JKL",
            LFixedShelfSKU = "MNO",
            ToeKickSKU = "PQR",
            DoweledDrawerBoxMaterialFinish = "Black",
            LShelfRadius = Dimension.FromInches(2),
            VerticalPanelBottomRadius = Dimension.FromInches(2)
        };

        var connection = _factory.CreateConnection().Result;
        var trx = connection.BeginTransaction();

        // Act
        var id = InsertCustomer.Handler.InsertClosetProSettings(settings, connection, trx).Result;
        trx.Commit();

        // Assert
        var queryResult = connection.QueryFirst("SELECT * FROM closet_pro_settings WHERE id = @Id", new { Id = id });

        ((string)queryResult.toe_kick_sku).Should().Be(settings.ToeKickSKU);
        ((string)queryResult.adjustable_shelf_sku).Should().Be(settings.AdjustableShelfSKU);
        ((string)queryResult.fixed_shelf_sku).Should().Be(settings.FixedShelfSKU);
        ((string)queryResult.l_fixed_shelf_sku).Should().Be(settings.LFixedShelfSKU);
        ((string)queryResult.l_adjustable_shelf_sku).Should().Be(settings.LAdjustableShelfSKU);
        ((double)queryResult.l_shelf_radius).Should().Be(settings.LShelfRadius.AsMillimeters());
        ((string)queryResult.diagonal_fixed_shelf_sku).Should().Be(settings.DiagonalFixedShelfSKU);
        ((string)queryResult.diagonal_adjustable_shelf_sku).Should().Be(settings.DiagonalAdjustableShelfSKU);
        ((string)queryResult.doweled_drawer_box_material_finish).Should().Be(settings.DoweledDrawerBoxMaterialFinish);
        ((double)queryResult.vertical_panel_bottom_radius).Should().Be(settings.VerticalPanelBottomRadius.AsMillimeters());

    }

    private static int GetTableRowCount(IDbConnection connection, string tableName) => connection.Query($"SELECT * FROM {tableName};").Count();

}
