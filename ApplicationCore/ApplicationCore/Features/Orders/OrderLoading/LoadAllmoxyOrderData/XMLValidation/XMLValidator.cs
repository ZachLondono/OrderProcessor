using ApplicationCore.Shared.Services;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace ApplicationCore.Features.Orders.OrderLoading.LoadAllmoxyOrderData.XMLValidation;

public class XMLValidator : IXMLValidator {

    private readonly IFileReader _fileReader;

    public XMLValidator(IFileReader fileReader) {
        _fileReader = fileReader;
    }

    public IEnumerable<XMLValidationError> ValidateXML(Stream xmlDataStream, string schemaFilePath) {

        XDocument doc = XDocument.Load(xmlDataStream, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);

        using var schemaStream = _fileReader.OpenReadFileStream(schemaFilePath);
        using XmlReader schemaReader = XmlReader.Create(schemaStream);

        var schemas = new XmlSchemaSet();
        schemas.Add("", schemaReader);

        var errors = new List<XMLValidationError>();
        doc.Validate(schemas, (s, e) => errors.Add(new(e.Severity, e.Exception)));

        return errors;

    }

}