using ApplicationCore.Features.Orders.Details.Models.WorkingDirectory;
using ApplicationCore.Infrastructure.Bus;
using FluentAssertions;
using NSubstitute;
using NSubstitute.Extensions;

namespace ApplicationCore.Tests.Unit.Orders;

public class WorkingDirectoryMigrationTests {

    private readonly MigrateWorkingDirectory.Handler _sut;
    private readonly IFileHandler _fileHandler = Substitute.For<IFileHandler>();

    public WorkingDirectoryMigrationTests() {
        _sut = new(_fileHandler);
    }

    [Fact]
    public void ShouldNotMigrate_WhenOldDirectoryDoesNotExist() {

        // Arrange
        string oldDir = "path/to/existing/wd";
        string newDir = "path/to/new/wd";
        MigrationType type = MigrationType.CopyFiles;

        var command = new MigrateWorkingDirectory.Command(oldDir, newDir, type);

        _fileHandler.DirectoryExists(oldDir).Returns(false);

        // Act
        var result = _sut.Handle(command).Result;

        // Assert
        result.IsError.Should().BeTrue();

    }

    [Fact]
    public void ShouldCreateDirectory_WhenNewDirectoryDoesNotExist() {

        // Arrange
        string oldDir = "path/to/existing/wd";
        string newDir = "path/to/new/wd";
        MigrationType type = MigrationType.CopyFiles;

        var command = new MigrateWorkingDirectory.Command(oldDir, newDir, type);

        _fileHandler.DirectoryExists(oldDir).Returns(false);

        // Act
        var result = _sut.Handle(command).Result;

        _fileHandler.Received().CreateDirectory(newDir);

        // Assert
        result.IsError.Should().BeTrue();

    }

    [Fact]
    public void ShouldNotMigrate_WhenDirectoryContainsTooManyFiles() {

        // Arrange
        string oldDir = "path/to/existing/wd";
        string newDir = "path/to/new/wd";
        MigrationType type = MigrationType.CopyFiles;

        var command = new MigrateWorkingDirectory.Command(oldDir, newDir, type);

        _fileHandler.DirectoryExists("").ReturnsForAnyArgs(true);
        _fileHandler.GetFiles("", "", SearchOption.AllDirectories).ReturnsForAnyArgs(new string[MigrateWorkingDirectory.Handler.MAX_FILES + 1]);

        // Act
        var result = _sut.Handle(command).Result;

        // Assert
        result.IsError.Should().BeTrue();

    }

    [Fact]
    public void ShouldCopyAllFilesToNewDirectory() {

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
        var result = _sut.Handle(command).Result;

        // Assert
        _fileHandler.ReceivedWithAnyArgs(5).Copy("", "");
        result.IsError.Should().BeFalse();

    }

    [Fact]
    public void ShouldMoveAllFilesToNewDirectory() {

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
        var result = _sut.Handle(command).Result;

        // Assert
        _fileHandler.ReceivedWithAnyArgs(5).Move("", "");
        result.IsError.Should().BeFalse();

    }

    [Fact]
    public void ShouldDeleteAllFilesInOldDirectory() {
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
        var result = _sut.Handle(command).Result;

        // Assert
        _fileHandler.ReceivedWithAnyArgs(5).DeleteFile("");
        result.IsError.Should().BeFalse();

    }

}
