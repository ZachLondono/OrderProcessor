using System.Xml.Schema;

namespace OrderLoading.LoadAllmoxyOrderData.XMLValidation;

public record XMLValidationError(XmlSeverityType Severity, XmlSchemaException Exception);
