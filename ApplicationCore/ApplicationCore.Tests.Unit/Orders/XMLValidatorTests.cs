using OrderLoading.LoadAllmoxyOrderData.XMLValidation;
using OrderLoading.LoadAllmoxyOrderData.AllmoxyXMLModels;
using FluentAssertions;
using NSubstitute;
using System.Text;
using Domain.Services;

namespace ApplicationCore.Tests.Unit.Orders;

public class XMLValidatorTests {

    private XMLValidator _sut;
    private IFileReader _fileReader = Substitute.For<IFileReader>();

    public XMLValidatorTests() {
        _sut = new(_fileReader);
    }

    [Fact]
    public void ValidateSource_Should_ReturnValidResult_WhenDataMatchesSchema() {

        string schemaFilePath = "path/to/schema.xsd";
        string schema = """
            <?xml version="1.0"?>
            <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema">
                <xs:element name="test">
                  <xs:complexType>
                    <xs:sequence>
                      <xs:element name="a" type="xs:string"/>
                      <xs:element name="b" type="xs:string"/>
                      <xs:element name="c" type="xs:string"/>
                      <xs:element name="d" type="xs:string"/>
                    </xs:sequence>
                  </xs:complexType>
                </xs:element>
            </xs:schema>
            """;

        string data = """
            <?xml version="1.0"?>
            <test>
                <a>A</a>
                <b>B</b>
                <c>C</c>
                <d>D</d>
            </test>
            """;

        using var dataStream = new MemoryStream(Encoding.UTF8.GetBytes(data));
        using var schemaStream = new MemoryStream(Encoding.UTF8.GetBytes(schema));

        _fileReader.OpenReadFileStream(schemaFilePath).Returns(schemaStream);

        var errors = _sut.ValidateXML(dataStream, schemaFilePath);

        errors.Should().BeEmpty();

    }

    [Fact]
    public void ValidateSource_Should_ReturnInValidResult_WhenDataDoesNotchMatchesSchema() {

        string schemaFilePath = "path/to/schema.xsd";
        string schema = """
            <?xml version="1.0"?>
            <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema">
                <xs:element name="test">
                  <xs:complexType>
                    <xs:sequence>
                      <xs:element name="a" type="xs:string"/>
                      <xs:element name="b" type="xs:string"/>
                      <xs:element name="c" type="xs:string"/>
                      <xs:element name="d" type="xs:string"/>
                    </xs:sequence>
                  </xs:complexType>
                </xs:element>
            </xs:schema>
            """;

        string data = """
            <?xml version="1.0"?>
            <invaliddata>
                <a>A</a>
                <b>B</b>
                <c>C</c>
                <d>D</d>
            </invaliddata>
            """;

        using var dataStream = new MemoryStream(Encoding.UTF8.GetBytes(data));
        using var schemaStream = new MemoryStream(Encoding.UTF8.GetBytes(schema));

        _fileReader.OpenReadFileStream(schemaFilePath).Returns(schemaStream);

        var errors = _sut.ValidateXML(dataStream, schemaFilePath);

        errors.Should().NotBeEmpty();

    }

}
