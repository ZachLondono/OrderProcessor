using System.Xml.Linq;

namespace OrderExporting.CNC.Programs.WSXML.Report;

public record Pattern(string Id, string Name, string MaterialId, IEnumerable<PatternPart> Parts) {
    public static Pattern FromXElement(XElement element) => new(element.AttributeValue("ID"), element.ElementValue("Name"), element.AttributeValue("MatID"), PatternPart.FromXElement(element));
}
