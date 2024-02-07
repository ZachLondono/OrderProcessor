namespace OrderLoading.LoadAllmoxyOrderData.XMLValidation;

public interface IXMLValidator {

    public IEnumerable<XMLValidationError> ValidateXML(Stream xmlDataStream, string schemaFilePath);

}
