using ApplicationCore.Features.ProductPlanner.Contracts;
using ApplicationCore.Features.ProductPlanner.Domain;
using ApplicationCore.Features.ProductPlanner.Services;
using NSubstitute;

namespace ApplicationCore.Tests.Unit.ProductPlanner;

public class PPJobConverterTests {

    private readonly PPJobConverter _sut;
    private readonly IExtWriter _writer = Substitute.For<IExtWriter>();

    public PPJobConverterTests() {

        _sut = new(_writer);

    }

    [Fact]
    public void ConvertOrder_ShouldCreateLevel_WhenAllProductsAreSimilar() {

        // Arrange
        var products = new List<PPProduct>() {
            CreateProduct(catalog:"catalog", materialType:"material", doorType:"door", hardwareType:"hardware"),
            CreateProduct(catalog:"catalog", materialType:"material", doorType:"door", hardwareType:"hardware")
        };

        var job = new PPJob("Job Name", DateTime.Now, products);

        // Act
        _sut.ConvertOrder(job);

        // Assert
        _writer.Received(1).AddRecord(Arg.Is<JobDescriptor>(j => j.Job == "Job Name"));
        _writer.ReceivedWithAnyArgs(1).AddRecord(Arg.Any<VariableOverride>());
        _writer.Received(1).AddRecord(Arg.Is<LevelDescriptor>(l => l.Name == "Lvl1"));
        _writer.ReceivedWithAnyArgs(products.Count).AddRecord(Arg.Any<ProductRecord>());

    }

    [Fact]
    public void ConvertOrder_ShouldCreateLevels_WhenProductsContainRoomNames() {

        // Arrange
        var products = new List<PPProduct>() {
            CreateProduct(room:"Room A"),
            CreateProduct(room:"Room B")
        };

        var job = new PPJob("Job Name", DateTime.Now, products);

        // Act
        _sut.ConvertOrder(job);

        // Assert
        _writer.Received(1).AddRecord(Arg.Is<JobDescriptor>(j => j.Job == "Job Name"));
        _writer.ReceivedWithAnyArgs(2).AddRecord(Arg.Any<VariableOverride>());
        _writer.ReceivedWithAnyArgs(2).AddRecord(Arg.Any<LevelDescriptor>());
        _writer.ReceivedWithAnyArgs(products.Count).AddRecord(Arg.Any<ProductRecord>());

    }

    [Fact]
    public void ConvertOrder_ShouldCreateLevels_WhenProductsContainDifferentMaterials() {

        // Arrange
        var products = new List<PPProduct>() {
            CreateProduct(materialType:"Mat Type A"),
            CreateProduct(materialType:"Mat Type B")
        };

        var job = new PPJob("Job Name", DateTime.Now, products);

        // Act
        _sut.ConvertOrder(job);

        // Assert
        _writer.Received(1).AddRecord(Arg.Is<JobDescriptor>(j => j.Job == "Job Name"));
        _writer.ReceivedWithAnyArgs(2).AddRecord(Arg.Any<VariableOverride>());
        _writer.ReceivedWithAnyArgs(2).AddRecord(Arg.Any<LevelDescriptor>());
        _writer.ReceivedWithAnyArgs(products.Count).AddRecord(Arg.Any<ProductRecord>());

    }

    [Fact]
    public void ConvertOrder_ShouldCreateLevels_WhenProductsContainDifferentDoorTypes() {

        // Arrange
        var products = new List<PPProduct>() {
            CreateProduct(doorType:"Door Type A"),
            CreateProduct(doorType:"Door Type B")
        };

        var job = new PPJob("Job Name", DateTime.Now, products);

        // Act
        _sut.ConvertOrder(job);

        // Assert
        _writer.Received(1).AddRecord(Arg.Is<JobDescriptor>(j => j.Job == "Job Name"));
        _writer.ReceivedWithAnyArgs(2).AddRecord(Arg.Any<VariableOverride>());
        _writer.ReceivedWithAnyArgs(2).AddRecord(Arg.Any<LevelDescriptor>());
        _writer.ReceivedWithAnyArgs(products.Count).AddRecord(Arg.Any<ProductRecord>());

    }

    [Fact]
    public void ConvertOrder_ShouldCreateLevels_WhenProductsContainDifferentHardwareTypes() {

        // Arrange
        var products = new List<PPProduct>() {
            CreateProduct(hardwareType:"Hardware Type A"),
            CreateProduct(hardwareType:"Hardware Type B")
        };

        var job = new PPJob("Job Name", DateTime.Now, products);

        // Act
        _sut.ConvertOrder(job);

        // Assert
        _writer.Received(1).AddRecord(Arg.Is<JobDescriptor>(j => j.Job == "Job Name"));
        _writer.ReceivedWithAnyArgs(2).AddRecord(Arg.Any<VariableOverride>());
        _writer.ReceivedWithAnyArgs(2).AddRecord(Arg.Any<LevelDescriptor>());
        _writer.ReceivedWithAnyArgs(products.Count).AddRecord(Arg.Any<ProductRecord>());

    }

    [Fact]
    public void ConvertOrder_ShouldCreateOnly1LevelPerRoom_WhenProductsInTheSameRoomAreTheSameMaterial() {

        // Arrange
        var products = new List<PPProduct>() {
            CreateProduct(room:"Room A", materialType:"Mat Type A"),
            CreateProduct(room:"Room B", materialType:"Mat Type A")
        };

        var job = new PPJob("Job Name", DateTime.Now, products);

        // Act
        _sut.ConvertOrder(job);

        // Assert
        _writer.Received(1).AddRecord(Arg.Is<JobDescriptor>(j => j.Job == "Job Name"));
        _writer.Received(1).AddRecord(Arg.Is<LevelDescriptor>(l => l.Name == "Room A"));
        _writer.Received(1).AddRecord(Arg.Is<LevelDescriptor>(l => l.Name == "Room B"));
        _writer.ReceivedWithAnyArgs(2).AddRecord(Arg.Any<VariableOverride>());
        _writer.ReceivedWithAnyArgs(products.Count).AddRecord(Arg.Any<ProductRecord>());

    }

    [Fact]
    public void ConvertOrder_ShouldCreateSubLevels_WhenProductsContainDifferentMaterialTypesWithinDifferentRooms() {

        // Arrange
        var products = new List<PPProduct>() {
            CreateProduct(room:"Room A", materialType:"Mat Type A"),
            CreateProduct(room:"Room A", materialType:"Mat Type B"),
            CreateProduct(room:"Room B", materialType:"Mat Type B"),
            CreateProduct(room:"Room B", materialType:"Mat Type A")
        };

        var job = new PPJob("Job Name", DateTime.Now, products);

        // Act
        _sut.ConvertOrder(job);

        // Assert
        _writer.Received(1).AddRecord(Arg.Is<JobDescriptor>(j => j.Job == "Job Name"));

        _writer.Received(1).AddRecord(Arg.Is<LevelDescriptor>(l => l.Name == "Room A"));
        _writer.Received(1).AddRecord(Arg.Is<LevelDescriptor>(l => l.Name == "1-Room A"));
        _writer.Received(1).AddRecord(Arg.Is<LevelDescriptor>(l => l.Name == "2-Room A"));

        _writer.Received(1).AddRecord(Arg.Is<LevelDescriptor>(l => l.Name == "Room B"));
        _writer.Received(1).AddRecord(Arg.Is<LevelDescriptor>(l => l.Name == "1-Room B"));
        _writer.Received(1).AddRecord(Arg.Is<LevelDescriptor>(l => l.Name == "2-Room B"));

        _writer.ReceivedWithAnyArgs(4).AddRecord(Arg.Any<VariableOverride>());

        _writer.ReceivedWithAnyArgs(products.Count).AddRecord(Arg.Any<ProductRecord>());

    }

    [Fact]
    public void ConvertOrder_ShouldCreateVariableOverride_WhenProductsContainOverrideParameters() {

        // Arrange
        var products = new List<PPProduct>() {
            CreateProduct(overrideParameters: new(){
                { "Key1", "Value1" }
            })
        };

        var job = new PPJob("Job Name", DateTime.Now, products);

        // Act
        _sut.ConvertOrder(job);

        // Assert
        _writer.Received(1).AddRecord(Arg.Is<JobDescriptor>(j => j.Job == "Job Name"));
        _writer.ReceivedWithAnyArgs(2).AddRecord(Arg.Any<VariableOverride>());
        _writer.Received(1).AddRecord(Arg.Is<VariableOverride>(v => v.Parameters["Key1"] == "Value1"));
        _writer.ReceivedWithAnyArgs(1).AddRecord(Arg.Any<LevelDescriptor>());
        _writer.ReceivedWithAnyArgs(products.Count).AddRecord(Arg.Any<ProductRecord>());

    }

    private static PPProduct CreateProduct(string room = "", string name = "", string catalog = "", string materialType = "", string doorType = "", string hardwareType = "", Dictionary<string, string>? overrideParameters = null)
        => new(Guid.NewGuid(), room, name, 1, catalog, materialType, doorType, hardwareType, new(), new(), new(), overrideParameters ?? new(), new());

}
