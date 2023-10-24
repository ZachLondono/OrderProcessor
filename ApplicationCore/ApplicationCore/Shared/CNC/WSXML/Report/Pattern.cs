using System.Xml.Linq;
using ApplicationCore.Shared.CNC.WSXML;

namespace ApplicationCore.Shared.CNC.WSXML.Report;

public record Pattern(string Id, string Name, string MaterialId, IEnumerable<PatternPart> Parts) {
    public static Pattern FromXElement(XElement element) => new(element.AttributeValue("ID"), element.ElementValue("Name"), element.AttributeValue("MatID"), PatternPart.FromXElement(element));
}
