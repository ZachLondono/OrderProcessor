namespace ApplicationCore.Features.Orders.Loader.XMLValidation;

public interface IXMLValidator {

    public IEnumerable<XMLValidationError> ValidateXML(Stream xmlDataStream, string schemaFilePath);

}
