using ApplicationCore.Features.Companies;
using ApplicationCore.Features.Companies.Commands;
using ApplicationCore.Features.Companies.Domain;
using ApplicationCore.Features.Companies.Domain.ValueObjects;
using ApplicationCore.Features.Companies.Queries;
using ApplicationCore.Infrastructure;
using FluentAssertions;
using NSubstitute;

namespace ApplicationCore.Tests.Unit.Companies;

public class CompanyStateTests {

    private readonly CompanyState _sut;
    private readonly IBus _bus = Substitute.For<IBus>();

    public CompanyStateTests() {
        _sut = new(_bus);
    }

    [Fact]
    public void ShouldBeNullByDefault() {

        // Assert
        _sut.Company.Should().BeNull();
        _sut.IsDirty.Should().BeFalse();

    }

    [Fact]
    public void ReplaceCompany_ShouldSetCopmany_AndResetIsDirty() {

        // Arrange
        var company = new Company(Guid.NewGuid(), "Company", new(), "", "", "");

        // Act
        _sut.ReplaceCompany(company);

        // Assert
        _sut.Company.Should().Be(company);
        _sut.IsDirty.Should().BeFalse();

    }

    [Fact]
    public void UpdateCompany_ShouldDoNothing_WhenCompanyIsNotSet() {

        // Arrange
        string name = "NewName";
        Address addr = new() {
            Line1 = "ABC123"
        };

        // Act
        _sut.UpdateCompany(name, addr);

        // Assert
        _sut.Company.Should().BeNull();
        _sut.IsDirty.Should().BeFalse();

    }

    [Fact]
    public void UpdateCompany_ShouldReplaceCopmany_AndSetIsDirty_WhenCompanyIsSet() {

        // Arrange
        var company = new Company(Guid.NewGuid(), "Company", new(), "", "", "");
        _sut.ReplaceCompany(company);

        string name = "NewName";
        Address addr = new() {
            Line1 = "ABC123"
        };

        // Act
        _sut.UpdateCompany(name, addr);

        // Assert
        _sut.Company.Should().NotBe(company);
        _sut.Company.Should().NotBeNull();
        _sut.Company!.Address.Should().Be(addr);
        _sut.Company.Name.Should().Be(name);
        _sut.Company.Id.Should().Be(company.Id);
        _sut.IsDirty.Should().BeTrue();

    }

    [Fact]
    public void SaveChanges_ShouldDoNothing_WhenCompanyIsNotSet() {

        // Act
        _sut.SaveChanges().Wait();

        // Assert
        _sut.Company.Should().BeNull();
        _sut.IsDirty.Should().BeFalse();
        _bus.DidNotReceiveWithAnyArgs().Send<object>(default!);

    }

    [Fact]
    public void UpdateCompany_ShouldCallUpdate_AndResetIsDirty_WhenCompanyIsSet() {

        // Arrange
        var company = new Company(Guid.NewGuid(), "Company", new(), "", "", "");
        _sut.ReplaceCompany(company);

        string name = "NewName";
        Address addr = new() {
            Line1 = "ABC123"
        };
        _sut.UpdateCompany(name, addr);

        // Act
        _sut.SaveChanges().Wait();

        // Assert
        _sut.IsDirty.Should().BeFalse();
        _sut.Company.Should().NotBeNull();
        _bus.Received(1).Send(new UpdateCompany.Command(_sut.Company!));

    }

    [Fact]
    public void LoadCompany_ShouldCallQuery_AndSetCompany() {

        // Arrange
        var company = new Company(Guid.NewGuid(), "Company", new(), "", "", "");
        _bus.Send(new GetCompanyById.Query(company.Id)).Returns(company);

        // Act
        _sut.LoadCompany(company.Id).Wait();

        // Assert
        _sut.IsDirty.Should().BeFalse();
        _sut.Company.Should().Be(company);
        _bus.Received(1).Send(new GetCompanyById.Query(company.Id));

    }

}
