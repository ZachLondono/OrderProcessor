using ApplicationCore.Features.Orders.Details.Models.WorkingDirectory;
using FluentAssertions;
using NSubstitute;

namespace ApplicationCore.Tests.Unit.Orders;

public class WorkingDirectoryMigrationTests {

    private readonly MigrateWorkingDirectory.Handler _sut;
    private readonly IFileHandler _fileHandler = Substitute.For<IFileHandler>();

    public WorkingDirectoryMigrationTests() {
        _sut = new(_fileHandler);
    }

    [Fact]
    public async Task ShouldNotMigrate_WhenOldDirectoryDoesNotExist() {

        // Arrange
        string oldDir = "path/to/existing/wd";
        string newDir = "path/to/new/wd";
        MigrationType type = MigrationType.CopyFiles;

        var command = new MigrateWorkingDirectory.Command(oldDir, newDir, type);

        _fileHandler.DirectoryExists(oldDir).Returns(false);

        // Act
        var result = await _sut.Handle(command);

        // Assert
        result.IsError.Should().BeTrue();

    }

    [Fact]
    public async Task ShouldCreateDirectory_WhenNewDirectoryDoesNotExist() {

        // Arrange
        string oldDir = "path/to/existing/wd";
        string newDir = "path/to/new/wd";
        MigrationType type = MigrationType.CopyFiles;

        var command = new MigrateWorkingDirectory.Command(oldDir, newDir, type);

        _fileHandler.DirectoryExists(oldDir).Returns(false);

        // Act
        var result = await _sut.Handle(command);

        _fileHandler.Received().CreateDirectory(newDir);

        // Assert
        result.IsError.Should().BeTrue();

    }

    [Fact]
    public async Task ShouldNotMigrate_WhenDirectoryContainsTooManyFiles() {

        // Arrange
        string oldDir = "path/to/existing/wd";
        string newDir = "path/to/new/wd";
        MigrationType type = MigrationType.CopyFiles;

        var command = new MigrateWorkingDirectory.Command(oldDir, newDir, type);

        _fileHandler.DirectoryExists("").ReturnsForAnyArgs(true);
        _fileHandler.GetFiles("", "", SearchOption.AllDirectories).ReturnsForAnyArgs(new string[MigrateWorkingDirectory.Handler.MAX_FILES + 1]);

        // Act
        var result = await _sut.Handle(command);

        // Assert
        result.IsError.Should().BeTrue();

    }

    [Fact]
    public async Task ShouldCopyAllFilesToNewDirectory() {

        // Arrange
        string oldDir = "path/to/existing/wd";
        string newDir = "path/to/new/wd";
        MigrationType type = MigrationType.CopyFiles;

        var command = new MigrateWorkingDirectory.Command(oldDir, newDir, type);

        _fileHandler.GetFiles(oldDir, "", SearchOption.TopDirectoryOnly).ReturnsForAnyArgs(new string[] {
            "1", "2", "3", "4", "5"
        });
        _fileHandler.DirectoryExists(oldDir).ReturnsForAnyArgs(true);

        // Act
        var result = await _sut.Handle(command);

        // Assert
        _fileHandler.ReceivedWithAnyArgs(5).Copy("", "");
        result.IsError.Should().BeFalse();

    }

    [Fact]
    public async Task ShouldMoveAllFilesToNewDirectory() {

        // Arrange
        string oldDir = "path/to/existing/wd";
        string newDir = "path/to/new/wd";
        MigrationType type = MigrationType.MoveFiles;

        var command = new MigrateWorkingDirectory.Command(oldDir, newDir, type);

        _fileHandler.GetFiles(oldDir, "", SearchOption.TopDirectoryOnly).ReturnsForAnyArgs(new string[] {
            "1", "2", "3", "4", "5"
        });
        _fileHandler.DirectoryExists(oldDir).ReturnsForAnyArgs(true);

        // Act
        var result = await _sut.Handle(command);

        // Assert
        _fileHandler.ReceivedWithAnyArgs(5).Move("", "");
        result.IsError.Should().BeFalse();

    }

    [Fact]
    public async Task ShouldDeleteAllFilesInOldDirectory() {
        // Arrange
        string oldDir = "path/to/existing/wd";
        string newDir = "path/to/new/wd";
        MigrationType type = MigrationType.DeleteFiles;

        var command = new MigrateWorkingDirectory.Command(oldDir, newDir, type);

        _fileHandler.GetFiles(oldDir, "", SearchOption.TopDirectoryOnly).ReturnsForAnyArgs(new string[] {
            "1", "2", "3", "4", "5"
        });
        _fileHandler.DirectoryExists(oldDir).ReturnsForAnyArgs(true);

        // Act
        var result = await _sut.Handle(command);

        // Assert
        _fileHandler.ReceivedWithAnyArgs(5).DeleteFile("");
        result.IsError.Should().BeFalse();

    }

}
