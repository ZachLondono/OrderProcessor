using ApplicationCore.Features.ExcelTemplates.Contracts;
using ApplicationCore.Features.ExcelTemplates.Domain;
using ApplicationCore.Features.Orders.Release.Handlers.CutListing.Models;
using ApplicationCore.Shared;
using FluentAssertions;
using NSubstitute;

namespace ApplicationCore.Tests.Unit.CutListing;

public class ClosedXMLTemplateGeneratorTests {

    private readonly ClosedXMLTemplateFiller _sut;
    private readonly ClosedXMLTemplateConfiguration _configuration = new() { TemplateFilePath = "FilePath" };
    private readonly IFileReader _fileReader = Substitute.For<IFileReader>();
    private readonly IExcelTemplateFactory _factory = Substitute.For<IExcelTemplateFactory>();
    private readonly IExcelPrinter _printer = Substitute.For<IExcelPrinter>();
    private readonly IExcelTemplate _template = Substitute.For<IExcelTemplate>();


    public ClosedXMLTemplateGeneratorTests() {
        _sut = new(_configuration, _fileReader, _factory, _printer);
    }

    [Fact]
    public void GenerateCutList_ShouldReturnFilePath_AndNotPrint_WhenValid() {

        // Arrange
        var cutlist = new CutList();
        string outputDirectory = "path\\to\\output";
        string filename = "outputfile";
        bool print = false;

        string expectedFile = Path.Combine(outputDirectory, $"{filename}.xlsx");

        var stream = new MemoryStream();
        _factory.CreateTemplate(stream).Returns(_template);
        _fileReader.OpenReadFileStream(_configuration.TemplateFilePath).Returns(stream);
        _fileReader.DoesFileExist(expectedFile).Returns(false);

        // Act
        var result = _sut.FillTemplate(cutlist, outputDirectory, filename, print).Result;

        // Assert
        result.Should().Be(expectedFile);
        _printer.DidNotReceiveWithAnyArgs().PrintFile(expectedFile);

    }

    [Fact]
    public void GenerateCutList_ShouldReturnNewFilePath_AndNotPrint_WhenValid_AndFileExists() {

        // Arrange
        var cutlist = new CutList();
        string outputDirectory = "path\\to\\output";
        string filename = "outputfile";
        bool print = false;

        string existingFile = Path.Combine(outputDirectory, $"{filename}.xlsx");
        string expectedFile = Path.Combine(outputDirectory, $"{filename} (1).xlsx");

        var stream = new MemoryStream();
        _factory.CreateTemplate(stream).Returns(_template);
        _fileReader.OpenReadFileStream(_configuration.TemplateFilePath).Returns(stream);
        _fileReader.DoesFileExist(existingFile).Returns(true);
        _fileReader.DoesFileExist(expectedFile).Returns(false);

        // Act
        var result = _sut.FillTemplate(cutlist, outputDirectory, filename, print).Result;

        // Assert
        result.Should().Be(expectedFile);
        _printer.DidNotReceiveWithAnyArgs().PrintFile(expectedFile);
    }

    [Fact]
    public void GenerateCutList_ShouldReturnFilePath_AndPrint_WhenValid() {

        // Arrange
        var cutlist = new CutList();
        string outputDirectory = "path\\to\\output";
        string filename = "outputfile";
        bool print = true;

        string expectedFile = Path.Combine(outputDirectory, $"{filename}.xlsx");

        var stream = new MemoryStream();
        _factory.CreateTemplate(stream).Returns(_template);
        _fileReader.OpenReadFileStream(_configuration.TemplateFilePath).Returns(stream);
        _fileReader.DoesFileExist(expectedFile).Returns(false);

        // Act
        var result = _sut.FillTemplate(cutlist, outputDirectory, filename, print).Result;

        // Assert
        result.Should().Be(expectedFile);
        _printer.ReceivedWithAnyArgs(1).PrintFile(expectedFile);

    }
}
