using System.Xml.Linq;

namespace OrderExporting.CNC.Programs.WSXML.Report;

public record PatternSchedule(string Id, string Name, IEnumerable<Pattern> Patterns) {
    public static PatternSchedule FromXElement(XElement element) => new(element.AttributeValue("ID"), element.ElementValue("Name"), element.Elements("Pattern").Select(Pattern.FromXElement));
}
