using System.Xml.Linq;

namespace ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.ReleasePDF.WSXML;

internal record Pattern(string Id, string Name, string MaterialId, IEnumerable<PatternPart> Parts) {
    public static Pattern FromXElement(XElement element) => new(element.AttributeValue("ID"), element.ElementValue("Name"), element.AttributeValue("MatID"), PatternPart.FromXEment(element));
}
