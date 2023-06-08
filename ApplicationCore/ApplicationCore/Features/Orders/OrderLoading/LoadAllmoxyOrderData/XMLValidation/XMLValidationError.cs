using System.Xml.Schema;

namespace ApplicationCore.Features.Orders.OrderLoading.LoadAllmoxyOrderData.XMLValidation;

public record XMLValidationError(XmlSeverityType Severity, XmlSchemaException Exception);
