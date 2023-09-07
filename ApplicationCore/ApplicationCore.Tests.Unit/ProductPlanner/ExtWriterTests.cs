using ApplicationCore.Features.Orders.OrderExport.Handlers.ExtExport.Domain;
using ApplicationCore.Features.Orders.OrderExport.Handlers.ExtExport.Services;
using FluentAssertions;

namespace ApplicationCore.Tests.Unit.ProductPlanner;

public class ExtWriterTests {

    [Fact]
    public void GetRecord_ShouldReturnCorrectDictionary_ForJobDescriptor() {

        // Arrange
        var job = new JobDescriptor() {
            Job = "This is the job name",
            Catalog = "This is the catalog name",
            Fronts = "This is the front style",
            Hardware = "This is the hardware style",
            Materials = "This is the materials style",
            LevelId = 456,
            Date = DateTime.Parse("05/05/1999"),
            Info1 = "Customer Name"
        };

        // Act
        var record = ExtWriter.GetRecord(job);

        // Assert
        record.Should().Contain(new KeyValuePair<string, string>("KEY", "XD"));
        record.Should().Contain(new KeyValuePair<string, string>("LEVELID", job.LevelId.ToString()));
        record.Should().Contain(new KeyValuePair<string, string>("JOB", job.Job));
        record.Should().Contain(new KeyValuePair<string, string>("DATE", job.Date.ToShortDateString()));
        record.Should().Contain(new KeyValuePair<string, string>("INFO1", job.Info1));
        record.Should().Contain(new KeyValuePair<string, string>("PCAT", job.Catalog));
        record.Should().Contain(new KeyValuePair<string, string>("MCAT", job.Materials));
        record.Should().Contain(new KeyValuePair<string, string>("SCAT", job.Fronts));
        record.Should().Contain(new KeyValuePair<string, string>("HCAT", job.Hardware));

    }

    [Fact]
    public void GetRecord_ShouldReturnCorrectDictionary_ForJobDescriptor_WithJobNameShortened() {

        // Arrange
        var job = new JobDescriptor() {
            Job = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ",
            Catalog = "This is the catalog name",
            Fronts = "This is the front style",
            Hardware = "This is the hardware style",
            Materials = "This is the materials style",
            LevelId = 456,
            Date = DateTime.Parse("05/05/1999"),
            Info1 = "Customer Name"
        };

        // Act
        var record = ExtWriter.GetRecord(job);

        // Assert
        record.Should().Contain(new KeyValuePair<string, string>("JOB", "abcdefghijklmnopqrstuvwxyzABCD"));
        
    }

    [Fact]
    public void GetRecord_ShouldReturnCorrectDictionary_ForLevelDescriptor() {

        // Arrange
        var level = new LevelDescriptor() {
            Name = "This is the job name",
            ParentId = 123,
            Catalog = "This is the catalog name",
            Fronts = "This is the front style",
            Hardware = "This is the hardware style",
            Materials = "This is the materials style",
            LevelId = 456,
        };

        // Act
        var record = ExtWriter.GetRecord(level);

        // Assert
        record.Should().Contain(new KeyValuePair<string, string>("KEY", "LD"));
        record.Should().Contain(new KeyValuePair<string, string>("LEVELID", level.LevelId.ToString()));
        record.Should().Contain(new KeyValuePair<string, string>("PARENTID", level.ParentId.ToString()));
        record.Should().Contain(new KeyValuePair<string, string>("LEVELNAME", level.Name));
        record.Should().Contain(new KeyValuePair<string, string>("PCAT", level.Catalog));
        record.Should().Contain(new KeyValuePair<string, string>("MCAT", level.Materials));
        record.Should().Contain(new KeyValuePair<string, string>("SCAT", level.Fronts));
        record.Should().Contain(new KeyValuePair<string, string>("HCAT", level.Hardware));

    }

    [Fact]
    public void GetRecord_ShouldReturnCorrectDictionary_ForLevelDescriptor_WithJobNameShortened() {

        // Arrange
        var level = new LevelDescriptor() {
            Name = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ",
            ParentId = 123,
            Catalog = "This is the catalog name",
            Fronts = "This is the front style",
            Hardware = "This is the hardware style",
            Materials = "This is the materials style",
            LevelId = 456,
        };

        // Act
        var record = ExtWriter.GetRecord(level);

        // Assert
        record.Should().Contain(new KeyValuePair<string, string>("LEVELNAME", "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZabcdefgh"));

    }

    [Fact]
    public void GetRecord_ShouldReturnCorrectDictionary_ForProductRecord() {

        // Arrange
        var product = new ProductRecord() {
            Name = "ProductName",
            ParentId = 123,
            Units = PPUnits.Inches,
            SeqText = "This is seq text",
            Qty = 1,
            ProductId = Guid.Empty,
            Pos = 1,
            CustomSpec = false,
            Parameters = new Dictionary<string, string>() {
                { "Param1", "Value1" }
            },
            Comment = "This is a comment"
        };

        var expectedUnits = "1";

        // Act
        var record = ExtWriter.GetRecord(product);

        // Assert
        record.Should().Contain(new KeyValuePair<string, string>("KEY", "PR"));
        record.Should().Contain(new KeyValuePair<string, string>("PARENTID", product.ParentId.ToString()));
        record.Should().Contain(new KeyValuePair<string, string>("NAME", product.Name));
        record.Should().Contain(new KeyValuePair<string, string>("UNT", expectedUnits));
        record.Should().Contain(new KeyValuePair<string, string>("WIDTH", "0"));
        record.Should().Contain(new KeyValuePair<string, string>("HEIGHT", "0"));
        record.Should().Contain(new KeyValuePair<string, string>("DEPTH", "0"));
        record.Should().Contain(new KeyValuePair<string, string>("QTY", product.Qty.ToString()));
        record.Should().Contain(new KeyValuePair<string, string>("POS", product.Pos.ToString()));
        record.Should().Contain(new KeyValuePair<string, string>("CABCOM", product.Comment));
        record.Should().Contain(new KeyValuePair<string, string>("CABCOM2", ""));
        record.Should().Contain(new KeyValuePair<string, string>("SEQTEXT", product.SeqText));
        record.Should().NotContainKey("CUSTSPEC");
        record.Should().Contain(new KeyValuePair<string, string>("Param1", "Value1"));

    }

    [Fact]
    public void GetRecord_ShouldReturnCorrectDictionary_ForProductRecord_WithCustomSpecs() {

        // Arrange
        var product = new ProductRecord() {
            Name = "ProductName",
            ParentId = 123,
            Units = PPUnits.Inches,
            SeqText = "This is seq text",
            Qty = 1,
            ProductId = Guid.Empty,
            Pos = 1,
            CustomSpec = true,
            Parameters = new Dictionary<string, string>() {
                { "Param1", "Value1" }
            },
            Comment = "This is a comment"
        };

        // Act
        var record = ExtWriter.GetRecord(product);

        // Assert
        record.Should().Contain(new KeyValuePair<string, string>("CUSTSPEC", "1"));

    }

    [Fact]
    public void GetRecord_ShouldReturnCorrectDictionary_ForProductRecord_WithTruncatedComment() {

        // Arrange
        var product = new ProductRecord() {
            Name = "ProductName",
            ParentId = 123,
            Units = PPUnits.Inches,
            SeqText = "This is seq text",
            Qty = 1,
            ProductId = Guid.Empty,
            Pos = 1,
            CustomSpec = true,
            Parameters = new Dictionary<string, string>() {
                { "Param1", "Value1" }
            },
            Comment = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ"
        };

        // Act
        var record = ExtWriter.GetRecord(product);

        // Assert
        record.Should().Contain(new KeyValuePair<string, string>("CABCOM", "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZabcdefgh"));

    }

    [Fact]
    public void GetRecord_ShouldReturnCorrectDictionary_ForProductRecord_WithTruncatedSeqText() {

        // Arrange
        var product = new ProductRecord() {
            Name = "ProductName",
            ParentId = 123,
            Units = PPUnits.Inches,
            SeqText = "This is a really long seq text",
            Qty = 1,
            ProductId = Guid.Empty,
            Pos = 1,
            CustomSpec = true,
            Parameters = new Dictionary<string, string>() {
                { "Param1", "Value1" }
            },
            Comment = "This is a comment"
        };

        // Act
        var record = ExtWriter.GetRecord(product);

        // Assert
        record.Should().Contain(new KeyValuePair<string, string>("SEQTEXT", "This is a really lon"));

    }

    [Fact]
    public void GetRecord_ShouldReturnCorrectDictionary_ForLevelVariableOverride() {

        // Arrange
        var variables = new LevelVariableOverride() {
            LevelId = 456,
            Units = PPUnits.Inches,
            Parameters = new Dictionary<string, string>() {
                { "Param1", "Value1" }
            }
        };

        // Act
        var record = ExtWriter.GetRecord(variables);

        // Assert
        record.Should().Contain(new KeyValuePair<string, string>("KEY", "LV"));
        record.Should().Contain(new KeyValuePair<string, string>("LEVELID", "456"));
        record.Should().Contain(new KeyValuePair<string, string>("UNT", "1"));
        record.Should().Contain(new KeyValuePair<string, string>("Param1", "Value1"));

    }

}
