using ApplicationCore.Features.CNC.Contracts;
using ApplicationCore.Features.CNC.ReleasePDF.Services;
using FluentAssertions;

namespace ApplicationCore.Tests.Unit.CNC;

public class ReleaseComparerTests {

    private readonly ReleaseGroupComparer _sut;

    public ReleaseComparerTests() {
        _sut = new ReleaseGroupComparer();
    }

    [Fact]
    public void ShouldReturnTrue_WhenComparingTwoNullArguments() {

        // Arrange
        MachineRelease? a = null;
        MachineRelease? b = null;

        // Act
        var result = _sut.Equals(a, b);

        // Assert
        result.Should().BeTrue();

    }

    [Fact]
    public void ShouldReturnFalse_WhenComparingANullToAnObject() {

        // Arrange
        MachineRelease? a = null;
        MachineRelease? b = new();

        // Act
        var result = _sut.Equals(a, b);

        // Assert
        result.Should().BeFalse();

    }

    [Fact]
    public void ShouldReturnFalse_WhenComparingAnObjectToANull() {

        // Arrange
        MachineRelease? a = new();
        MachineRelease? b = null;

        // Act
        var result = _sut.Equals(a, b);

        // Assert
        result.Should().BeFalse();

    }

    [Fact]
    public void ShouldReturnTrue_WhenComparingIdenticalObjects() {

        // Arrange
        MachineRelease? a = new() {
            MachineName = "Machine Name",
            MachineTableOrientation = Features.CNC.Domain.TableOrientation.Standard,
            ToolTable = new Dictionary<int, string>() {
                [1] = "A",
                [2] = "B",
            }.AsReadOnly(),
            Programs = new List<ReleasedProgram>() {
                new() {
                    HasFace6 = true,
                    ImagePath = "path/to/image",
                    Name = "A",
                    Material = new() {
                        Name = "A",
                        IsGrained = true,
                        Width = 1,
                        Length = 1,
                        Thickness = 1,
                        Yield = 1
                    }
                }
            }
        };

        MachineRelease? b = new() {
            MachineName = "Machine Name",
            MachineTableOrientation = Features.CNC.Domain.TableOrientation.Standard,
            ToolTable = new Dictionary<int, string>() {
                [1] = "A",
                [2] = "B",
            }.AsReadOnly(),
            Programs = new List<ReleasedProgram>() {
                new() {
                    HasFace6 = true,
                    ImagePath = "path/to/image",
                    Name = "A",
                    Material = new() {
                        Name = "A",
                        IsGrained = true,
                        Width = 1,
                        Length = 1,
                        Thickness = 1,
                        Yield = 1
                    }
                }
            }
        };

        // Act
        var result = _sut.Equals(a, b);

        // Assert
        result.Should().BeTrue();

    }

    [Fact]
    public void ShouldReturnTrue_WhenComparingObjectsWhichOnlyDifferInNameAndOrientation() {

        // Arrange
        MachineRelease? a = new() {
            MachineName = "Machine Name A",
            MachineTableOrientation = Features.CNC.Domain.TableOrientation.Standard,
            ToolTable = new Dictionary<int, string>() {
                [1] = "A",
                [2] = "B",
            }.AsReadOnly(),
            Programs = new List<ReleasedProgram>() {
                new() {
                    HasFace6 = true,
                    ImagePath = "path/to/image",
                    Name = "A",
                    Material = new() {
                        Name = "A",
                        IsGrained = true,
                        Width = 1,
                        Length = 1,
                        Thickness = 1,
                        Yield = 1
                    }
                }
            }
        };

        MachineRelease? b = new() {
            MachineName = "Machine Name B",
            MachineTableOrientation = Features.CNC.Domain.TableOrientation.Rotated,
            ToolTable = new Dictionary<int, string>() {
                [1] = "A",
                [2] = "B",
            }.AsReadOnly(),
            Programs = new List<ReleasedProgram>() {
                new() {
                    HasFace6 = true,
                    ImagePath = "path/to/image",
                    Name = "A",
                    Material = new() {
                        Name = "A",
                        IsGrained = true,
                        Width = 1,
                        Length = 1,
                        Thickness = 1,
                        Yield = 1
                    }
                }
            }
        };

        // Act
        var result = _sut.Equals(a, b);

        // Assert
        result.Should().BeTrue();

    }

    [Fact]
    public void ShouldReturnFalse_WhenObjectsContainDifferentToolsInTheSamePosition() {

        // Arrange
        MachineRelease? a = new() {
            ToolTable = new Dictionary<int, string>() {
                [1] = "B",
                [2] = "A",
            }.AsReadOnly()
        };

        MachineRelease? b = new() {
            ToolTable = new Dictionary<int, string>() {
                [1] = "A",
                [2] = "B",
            }.AsReadOnly()
        };

        // Act
        var result = _sut.Equals(a, b);

        // Assert
        result.Should().BeFalse();

    }

    [Fact]
    public void ShouldReturnTrue_WhenObjectsContainDifferentNumberOfToolSlots() {

        // Arrange
        MachineRelease? a = new() {
            ToolTable = new Dictionary<int, string>() {
                [1] = "A",
                [2] = "B",
                [3] = string.Empty,
                [4] = string.Empty
            }.AsReadOnly()
        };

        MachineRelease? b = new() {
            ToolTable = new Dictionary<int, string>() {
                [1] = "A",
                [2] = "B",
            }.AsReadOnly()
        };

        // Act
        var result = _sut.Equals(a, b);

        // Assert
        result.Should().BeTrue();

    }

    [Theory]
    [InlineData("A", "B", "", "C", "A", "B", "", "")]
    [InlineData("A", "B", "", "", "A", "B", "", "C")]
    public void ShouldReturnFalse_WhenObjectsContainDifferentEmptyToolSlots(string a1, string a2, string a3, string a4, string b1, string b2, string b3, string b4) {

        // Arrange
        MachineRelease? a = new() {
            ToolTable = new Dictionary<int, string>() {
                [1] = a1,
                [2] = a2,
                [3] = a3,
                [4] = a4
            }.AsReadOnly()
        };

        MachineRelease? b = new() {
            ToolTable = new Dictionary<int, string>() {
                [1] = b1,
                [2] = b2,
                [3] = b3,
                [4] = b4
            }.AsReadOnly()
        };

        // Act
        var result = _sut.Equals(a, b);

        // Assert
        result.Should().BeFalse();

    }

    [Fact]
    public void ShouldReturnFalse_WhenObjectsContainDifferentNumberOfPrograms() {

        // Arrange
        MachineRelease? a = new() {
            Programs = new List<ReleasedProgram>() {
                new(),
                new(),
                new()
            }
        };

        MachineRelease? b = new() {
            Programs = new List<ReleasedProgram>() {
                new(),
            }
        };

        // Act
        var result = _sut.Equals(a, b);

        // Assert
        result.Should().BeFalse();

    }

    [Fact]
    public void ShouldReturnTrue_WhenMatchingProgramsHaveSameMaterials() {

        // Arrange
        MachineRelease? a = new() {
            Programs = new List<ReleasedProgram>() {
                new() {
                    Material = new() {
                        Name = "A"
                    }
                }
            }
        };

        MachineRelease? b = new() {
            Programs = new List<ReleasedProgram>() {
                new() {
                    Material = new() {
                        Name = "A"
                    }
                }
            }
        };

        // Act
        var result = _sut.Equals(a, b);

        // Assert
        result.Should().BeTrue();


    }

    [Theory]
    [InlineData("A", 1, 1, 1, true, "B", 1, 1, 1, true)]
    [InlineData("A", 1, 1, 1, true, "A", 2, 1, 1, true)]
    [InlineData("A", 1, 1, 1, true, "A", 1, 2, 1, true)]
    [InlineData("A", 1, 1, 1, true, "A", 1, 1, 2, true)]
    [InlineData("A", 1, 1, 1, true, "A", 1, 1, 1, false)]
    public void ShouldReturnFalse_WhenMatchingProgramsHaveDifferentMaterials(string name1, double width1, double length1, double thickness1, bool grained1,
                                                                            string name2, double width2, double length2, double thickness2, bool grained2) {

        // Arrange
        MachineRelease? a = new() {
            Programs = new List<ReleasedProgram>() {
                new() {
                    Material = new() {
                        Name = name1,
                        Width = width1,
                        Length = length1,
                        Thickness = thickness1,
                        IsGrained = grained1,
                    }
                }
            }
        };

        MachineRelease? b = new() {
            Programs = new List<ReleasedProgram>() {
                new() {
                    Material = new() {
                        Name = name2,
                        Width = width2,
                        Length = length2,
                        Thickness = thickness2,
                        IsGrained = grained2,
                    }
                }
            }
        };

        // Act
        var result = _sut.Equals(a, b);

        // Assert
        result.Should().BeFalse();

    }

    [Fact]
    public void ShouldReturnFalse_WhenMatchingProgramsHaveDifferentNumberOfParts() {

        // Arrange
        MachineRelease? a = new() {
            Programs = new List<ReleasedProgram>() {
                new() {
                   Parts = new List<NestedPart>() {
                       new()
                   }
                }
            }
        };

        MachineRelease? b = new() {
            Programs = new List<ReleasedProgram>() {
                new() {
                   Parts = new List<NestedPart>() {
                       new(),
                       new(),
                       new()
                   }
                }
            }
        };

        // Act
        var result = _sut.Equals(a, b);

        // Assert
        result.Should().BeFalse();

    }

}
