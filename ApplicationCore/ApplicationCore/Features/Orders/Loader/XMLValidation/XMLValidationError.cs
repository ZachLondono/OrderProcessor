using System.Xml.Schema;

namespace ApplicationCore.Features.Orders.Loader.XMLValidation;

public record XMLValidationError(XmlSeverityType Severity, XmlSchemaException Exception);
