using Domain.Orders.Entities.Products.Cabinets;
using Domain.Orders.Enums;

namespace ApplicationCore.Tests.Unit.Orders.Persistence.Cabinets;

public class CabinetPartPersistenceTest : PersistenceTests {

    [Fact]
    public async Task InsertOrderWithCabinetPart() {

        var cabPart = new CabinetPart(Guid.NewGuid(), 1, 123.0M, 123, "ABC123", "DEF456", new("White", CabinetMaterialFinishType.Melamine, CabinetMaterialCore.ParticleBoard), "Beige", "Comment", new Dictionary<string, string>() {
            { "Param A", "Value A" }
        }, new());

        await InsertAndQueryOrderWithProduct(cabPart);

    }

    [Fact]
    public async Task DeleteOrderWithCabinetPart() {

        var cabPart = new CabinetPart(Guid.NewGuid(), 1, 123.0M, 123, "ABC123", "DEF456", new("White", CabinetMaterialFinishType.Melamine, CabinetMaterialCore.ParticleBoard), "Beige", "Comment", new Dictionary<string, string>() {
            { "Param A", "Value A" }
        }, new());

        await InsertAndDeleteOrderWithProduct(cabPart);

    }

    [Fact]
    public async Task InsertOrderWithCabinetPartWithProductionNotes() {

        var cabPart = new CabinetPart(Guid.NewGuid(), 1, 123.0M, 123, "ABC123", "DEF456", new("White", CabinetMaterialFinishType.Melamine, CabinetMaterialCore.ParticleBoard), "Beige", "Comment", new Dictionary<string, string>() {
            { "Param A", "Value A" }
        }, new() {
            "Note A",
            "Note B"
        });

        await InsertAndQueryOrderWithProduct(cabPart);

    }

}
